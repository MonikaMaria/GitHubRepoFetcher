namespace GitHubRepoFetcher.Domain
{
    public class Commit
    {
        public Guid Id { get; private set; }
        public string Sha { get; private set; }
        public string Message { get; private set; }
        public string Committer { get; private set; }
        public DateTimeOffset CommittedAt { get; private set; } //TODO: verify if needed
        //public virtual Committer Committer { get; private set; }

        public static Commit Create(string sha, string message, string committer, DateTimeOffset committedAt)
        {
            return new Commit(sha, message, committer, committedAt);
        }

        private Commit(string sha, string message, string committer, DateTimeOffset committedAt)
        {
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
