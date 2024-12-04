namespace GitHubRepoFetcher.Application
{
    public record CommitDisplayModel(string RepositoryName, string Sha, string Message, string Commiter)
    {
        public override string ToString()
        {
            return $"[{RepositoryName}]/[{Sha}]: {Message} [{Commiter}]";
        }
    }
}
