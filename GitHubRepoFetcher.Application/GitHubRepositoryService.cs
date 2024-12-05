using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using EFCore.BulkExtensions;
using GitHubRepoFetcher.Domain;
using GitHubRepoFetcher.Infrastructure;
using static System.Text.RegularExpressions.Regex;

namespace GitHubRepoFetcher.Application;

public interface IGitHubRepositoryService
{
    Task<bool> CheckUserExistsAsync(string userName, CancellationToken cancellationToken);

    Task<bool> CheckRepositoryExistsAsync(string userName, string repositoryName, CancellationToken cancellationToken);

    Task<IEnumerable<GitHubCommitItem>> GetGitHubCommitsAsync(string userName, string repositoryName, CancellationToken cancellationToken);

    Task SaveCommits(string userName, string repositoryName, IEnumerable<GitHubCommitItem> gitHubCommits, CancellationToken cancellationToken);
}

public class GitHubRepositoryService(DatabaseContext dbContext, IGitHubApi api) : IGitHubRepositoryService
{
    public async Task<bool> CheckRepositoryExistsAsync(string userName, string repositoryName, CancellationToken cancellationToken)
    {
        var response = await api.GetRepositoryAsync(userName, repositoryName, cancellationToken);
        return response.IsSuccessful;
    }

    public async Task<bool> CheckUserExistsAsync(string userName, CancellationToken cancellationToken)
    {
        var response = await api.GetUserAsync(userName, cancellationToken);
        return response.IsSuccessful;
    }

    public async Task<IEnumerable<GitHubCommitItem>> GetGitHubCommitsAsync(string userName, string repositoryName,
        CancellationToken cancellationToken)
    {
        var commits = new List<GitHubCommitItem>();
        var firstPageResult = await api.GetCommitsAsync(userName, repositoryName, page: 1, cancellationToken);

        if (firstPageResult.IsSuccessful)
        {
            commits.AddRange(firstPageResult.Content);

            HttpHeaders headers = firstPageResult.Headers;
            if (headers.TryGetValues("link", out var values))
            {
                string linkHeader = values.First();
                var lastPageNumber = GetLastPageNumber(linkHeader);

                if (lastPageNumber is > 1)
                {
                    for (int page = 2; page <= lastPageNumber; page++)
                    {
                        var currentPageResult = await api.GetCommitsAsync(userName, repositoryName, page, cancellationToken);
                        if (currentPageResult.IsSuccessful)
                        {
                            commits.AddRange(currentPageResult.Content);
                        }
                    }
                }
            }
        }

        return commits;
    }

    private int? GetLastPageNumber(string linkHeaderValue)
    {
        if (string.IsNullOrWhiteSpace(linkHeaderValue)) 
            return null;

        var lastPageMatch = Match(linkHeaderValue, @"(?<=page=)(\d+)(?=>; rel=""last"")", RegexOptions.IgnoreCase);
        var lastPageNumber = Convert.ToInt32(lastPageMatch.Value);
        return lastPageNumber;

    }

    public async Task SaveCommits(string userName, string repositoryName, IEnumerable<GitHubCommitItem> gitHubCommits,
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
}