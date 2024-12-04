namespace GitHubRepoFetcher.Domain
{
    public class Commit
    {
        public Guid Id { get; private set; }
        public string UserName { get; private set; }
        public string RepositoryName { get; private set; }
        public string Sha { get; private set; }
        public string Message { get; private set; }
        public string Committer { get; private set; }
        public DateTimeOffset CommittedAt { get; private set; } //TODO: verify if needed

        public static Commit Create(string userName, string repositoryName, string sha, string message, string committer, DateTimeOffset committedAt)
        {
            return new Commit(userName, repositoryName, sha, message, committer, committedAt);
        }

        private Commit(string userName, string repositoryName, string sha, string message, string committer, DateTimeOffset committedAt)
        {
            UserName = userName;
            RepositoryName = repositoryName;
            Sha = sha;
            Message = message;
            Committer = committer;
            CommittedAt = committedAt;
        }

        public void Update(string message, DateTimeOffset committedAt) 
        {
            //TODO: check date (amend)
            Message = message;
            CommittedAt = committedAt;
        }
    }
}
