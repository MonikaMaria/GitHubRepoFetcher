using GitHubRepoFetcher.Domain;

namespace GitHubRepoFetcher.Application;

public static class CommitModelsMapperExtensions
{
    public static IOrderedEnumerable<CommitDisplayModel> MapToCommitsToDisplay(this IEnumerable<GitHubCommitItem> gitHubCommits, string repositoryName)
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

    public static IEnumerable<Commit> MapToEntities(this IEnumerable<GitHubCommitItem> gitHubCommits, string userName, string repositoryName)
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