namespace GitHubRepoFetcher.Application;

public static class AuthBearerTokenFactory
{
    private static Func<CancellationToken, Task<string>>? _getBearerTokenAsyncFunc;

    public static void SetBearerTokenGetterFunc(Func<CancellationToken, Task<string>> getBearerTokenAsyncFunc)
        => _getBearerTokenAsyncFunc = getBearerTokenAsyncFunc;

    public static Task<string> GetBearerTokenAsync(CancellationToken cancellationToken)
    {
        if (_getBearerTokenAsyncFunc is null)
        {
            throw new InvalidOperationException("Bearer token is null");
        }
        return _getBearerTokenAsyncFunc!(cancellationToken);
    }
}