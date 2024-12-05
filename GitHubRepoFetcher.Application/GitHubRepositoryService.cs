using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using EFCore.BulkExtensions;
using GitHubRepoFetcher.Domain;
using GitHubRepoFetcher.Infrastructure;

namespace GitHubRepoFetcher.Application;

public interface IGitHubRepositoryService
{
    Task<bool> CheckUserExistsAsync(string userName, CancellationToken cancellationToken);

    Task<bool> CheckRepositoryExistsAsync(string userName, string repositoryName, CancellationToken cancellationToken);

    Task<IEnumerable<GitHubCommitItem>> GetGitHubCommitsAsync(string userName, string repositoryName, CancellationToken cancellationToken);

    IOrderedEnumerable<CommitDisplayModel> MapCommitsToDisplay(IEnumerable<GitHubCommitItem> gitHubCommits, string repositoryName);
    
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

    public IOrderedEnumerable<CommitDisplayModel> MapCommitsToDisplay(IEnumerable<GitHubCommitItem> gitHubCommits, string repositoryName)
    {
        return gitHubCommits.Select(c => 
                new CommitDisplayModel(
                    RepositoryName: repositoryName,
                    Sha: c.Sha,
                    Message: c.Commit.Message,
                    CommitterName: c.Commit.Committer.Name,
                    CommittedAt: c.Commit.Committer.Date)
            )
            .OrderByDescending(c => c.CommittedAt);
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
                        commits.AddRange(currentPageResult.Content);
                    }
                }
            }
        }

        return commits;
    }

    private int? GetLastPageNumber(string linkHeaderStr)
    {
        if (!string.IsNullOrWhiteSpace(linkHeaderStr))
        {
            var lastPageMatch = Regex.Match(linkHeaderStr, @"(?<=page=)(\d+)(?=>; rel=""last"")", RegexOptions.IgnoreCase);
            int lastPageNumber = Convert.ToInt32(lastPageMatch.Value);
            return lastPageNumber;
        }

        return null;
    }

    public async Task SaveCommits(string userName, string repositoryName, IEnumerable<GitHubCommitItem> gitHubCommits,
        CancellationToken cancellationToken)
    {
        var mappedCommits = MapGitHubCommitsToEntities(userName, repositoryName, gitHubCommits);

        await dbContext.BulkInsertOrUpdateAsync(mappedCommits, 
            cfg =>
            {
                cfg.UpdateByProperties = new List<string> { nameof(Commit.UserName), nameof(Commit.RepositoryName), nameof(Commit.Sha) };
                cfg.PropertiesToExcludeOnUpdate = new List<string> { nameof(Commit.Id) };
            },
            cancellationToken: cancellationToken);
    }

    private IEnumerable<Commit> MapGitHubCommitsToEntities(string userName, string repositoryName, IEnumerable<GitHubCommitItem> gitHubCommits)
    {
        return gitHubCommits.Select(c =>
            Commit.Create(
                userName: userName,
                repositoryName: repositoryName,
                sha: c.Sha,
                message: c.Commit.Message,
                committerName: c.Commit.Committer.Name,
                committerEmail: c.Commit.Committer.Email,
                committedAt: c.Commit.Committer.Date));
    }
}