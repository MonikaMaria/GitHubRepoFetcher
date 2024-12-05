namespace GitHubRepoFetcher.Domain;

public class Commit
{
    public Guid Id { get; private set; }
    public string UserName { get; private set; }
    public string RepositoryName { get; private set; }
    public string Sha { get; private set; }
    public string Message { get; private set; }
    public string CommitterName { get; private set; }
    public string CommiterEmail {  get; private set; }
    public DateTimeOffset CommittedAt { get; private set; } //TODO: verify if needed

    public static Commit Create(string userName, string repositoryName, string sha, string message, string committerName, string committerEmail, DateTimeOffset committedAt)
    {
        return new Commit(userName, repositoryName, sha, message, committerName, committerEmail, committedAt);
    }

    private Commit(string userName, string repositoryName, string sha, string message, string committerName, string committerEmail, DateTimeOffset committedAt)
    {
        UserName = userName;
        RepositoryName = repositoryName;
        Sha = sha;
        Message = message;
        CommitterName = committerName;
        CommiterEmail = committerEmail;
        CommittedAt = committedAt;
    }
}