using System.Net.Http.Json;

namespace GitHubRepoFetcher.Application
{
    public interface IGitHubClient
    {
        Task<IEnumerable<GitHubCommitItem>> GetCommits(string userName, string repositoryName, CancellationToken cancellationToken);
    }

    public class GitHubClient(HttpClient httpClient) : IGitHubClient
    {
        public async Task<IEnumerable<GitHubCommitItem>> GetCommits(string userName, string repositoryName, CancellationToken cancellationToken)
        {
            var url = $"/repos/{userName}/{repositoryName}/commits";
            return await httpClient.GetFromJsonAsync<IEnumerable<GitHubCommitItem>>(url, cancellationToken);
        }
    }
}
