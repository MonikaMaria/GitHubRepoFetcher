namespace GitHubRepoFetcher.Application
{
    public class GitHubCommitItem
    {
        public string Sha { get; set; }
        public GitHubCommit Commit { get; set; }
    }

    public class GitHubCommit
    {
        public GitHubCommitter Committer { get; set; } //TODO: committer or author?
        public string Message { get; set; }
    }

    public class GitHubCommitter
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTimeOffset Date { get; set; } //TODO: to check
    }
}
