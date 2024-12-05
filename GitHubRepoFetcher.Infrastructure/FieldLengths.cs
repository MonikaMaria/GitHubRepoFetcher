namespace GitHubRepoFetcher.Infrastructure;

public class FieldLengths
{
    public static class Commit
    {
        public static int Sha = 50;
        public static int Message = 300;
        public static int CommitterName = 50;
        public static int CommitterEmail = 150;
    }
}