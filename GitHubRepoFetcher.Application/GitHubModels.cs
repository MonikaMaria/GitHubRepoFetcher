namespace GitHubRepoFetcher.Application;

public class GitHubCommitItem
{
    public string Sha { get; set; } = null!;
    public GitHubCommit Commit { get; set; } = null!;
}

public class GitHubCommit
{
    public GitHubCommitter Committer { get; set; } = null!;
    public string Message { get; set; } = null!;
}

public class GitHubCommitter
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTimeOffset Date { get; set; }
}