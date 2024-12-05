namespace GitHubRepoFetcher.Application;

public record CommitDisplayModel(string RepositoryName, string Sha, string Message, string CommitterName, DateTimeOffset CommittedAt)
{
    public override string ToString()
    {
        return $"[{RepositoryName}]/[{Sha}]: {Message} [{CommitterName}]";
    }
}