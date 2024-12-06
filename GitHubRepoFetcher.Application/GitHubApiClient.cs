using Refit;

namespace GitHubRepoFetcher.Application;

[Headers(
    "User-Agent: GitHubRepoFetcher",
    "Authorization: Bearer",
    "Accept: application/vnd.github+json", 
    "X-GitHub-Api-Version: 2022-11-28"
)]
public interface IGitHubApiClient
{
    [Get("/users/{userName}")]
    Task<ApiResponse<string>> GetUserAsync(string userName, CancellationToken cancellationToken);

    [Get("/repos/{userName}/{repositoryName}")]
    Task<ApiResponse<string>> GetRepositoryAsync(string userName, string repositoryName, CancellationToken cancellationToken);

    [Get("/repos/{userName}/{repositoryName}/commits?per_page=100")] // Parameterize page size if needed
    Task<ApiResponse<IEnumerable<GitHubCommitItem>>> GetCommitsAsync(string userName, string repositoryName, int page, CancellationToken cancellationToken);
}