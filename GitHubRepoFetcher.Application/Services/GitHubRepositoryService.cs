using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using EFCore.BulkExtensions;
using GitHubRepoFetcher.Infrastructure;
using static System.Text.RegularExpressions.Regex;
using Commit = GitHubRepoFetcher.Domain.Commit;

namespace GitHubRepoFetcher.Application.Services;

public interface IGitHubRepositoryService
{
    Task<bool> CheckUserExistsAsync(string userName, CancellationToken cancellationToken);

    Task<bool> CheckRepositoryExistsAsync(string userName, string repositoryName, CancellationToken cancellationToken);

    Task<IEnumerable<GitHubCommitItem>> GetGitHubCommitsAsync(string userName, string repositoryName, CancellationToken cancellationToken);

    Task SaveCommitsAsync(string userName, string repositoryName, IEnumerable<GitHubCommitItem> gitHubCommits, CancellationToken cancellationToken);
}

public class GitHubRepositoryService(DatabaseContext dbContext, IGitHubApiClient gitHubApiClient) : IGitHubRepositoryService
{
    public async Task<bool> CheckRepositoryExistsAsync(string userName, string repositoryName, CancellationToken cancellationToken)
    {
        var response = await gitHubApiClient.GetRepositoryAsync(userName, repositoryName, cancellationToken);
        return response.IsSuccessful;
    }

    public async Task<bool> CheckUserExistsAsync(string userName, CancellationToken cancellationToken)
    {
        var response = await gitHubApiClient.GetUserAsync(userName, cancellationToken);
        return response.IsSuccessful;
    }

    public async Task<IEnumerable<GitHubCommitItem>> GetGitHubCommitsAsync(string userName, string repositoryName,
        CancellationToken cancellationToken)
    {
        var commits = new List<GitHubCommitItem>();
        var firstPageResult = await gitHubApiClient.GetCommitsAsync(userName, repositoryName, page: 1, cancellationToken);

        if (firstPageResult.IsSuccessful)
        {
            commits.AddRange(firstPageResult.Content);

            var lastPageNumber = GetLastPageNumber(firstPageResult.Headers);

            if (lastPageNumber is > 1)
            {
                await GetNextGitHubCommitsAsync(userName, repositoryName, lastPageNumber, commits, cancellationToken);
            }
        }

        return commits;
    }

    public async Task SaveCommitsAsync(string userName, string repositoryName, IEnumerable<GitHubCommitItem> gitHubCommits,
        CancellationToken cancellationToken)
    {
        var mappedCommits = gitHubCommits.MapToEntities(userName, repositoryName);

        await dbContext.BulkInsertOrUpdateAsync(mappedCommits,
            cfg =>
            {
                cfg.UpdateByProperties = [nameof(Commit.UserName), nameof(Commit.RepositoryName), nameof(Commit.Sha)];
                cfg.PropertiesToExcludeOnUpdate = [nameof(Commit.Id)];
            },
            cancellationToken: cancellationToken);
    }

    private async Task GetNextGitHubCommitsAsync(string userName, string repositoryName,
        [DisallowNull] int? lastPageNumber, List<GitHubCommitItem> commits,
        CancellationToken cancellationToken)
    {
        for (var page = 2; page <= lastPageNumber; page++)
        {
            var currentPageResult = await gitHubApiClient.GetCommitsAsync(userName, repositoryName, page, cancellationToken);
            if (currentPageResult.IsSuccessful)
            {
                commits.AddRange(currentPageResult.Content);
            }
        }
    }

    private int? GetLastPageNumber(HttpResponseHeaders headers)
    {
        if (!headers.TryGetValues("link", out var values))
            return null;

        var linkHeader = values.First();
        var lastPageNumber = ExtractLastPageNumberFromLinkHeader(linkHeader);

        return lastPageNumber;

    }

    private int? ExtractLastPageNumberFromLinkHeader(string linkHeaderValue)
    {
        if (string.IsNullOrWhiteSpace(linkHeaderValue))
            return null;

        var lastPageMatch = Match(linkHeaderValue, @"(?<=page=)(\d+)(?=>; rel=""last"")", RegexOptions.IgnoreCase);
        var lastPageNumber = Convert.ToInt32(lastPageMatch.Value);
        return lastPageNumber;
    }
}