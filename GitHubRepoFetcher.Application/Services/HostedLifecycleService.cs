using GitHubRepoFetcher.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GitHubRepoFetcher.Application.Services;

public sealed class HostedLifecycleService(IServiceScopeFactory scopeFactory) : IHostedService
{
    private AsyncServiceScope _asyncServiceScope;

    private DatabaseContext _dbContext = null!;
    private IMainLoopService _mainLoopService = null!;
    private IUIHandler _uiHandler = null!;

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        _asyncServiceScope = scopeFactory.CreateAsyncScope();
        _dbContext = _asyncServiceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
        _mainLoopService = _asyncServiceScope.ServiceProvider.GetRequiredService<IMainLoopService>();
        _uiHandler = _asyncServiceScope.ServiceProvider.GetRequiredService<IUIHandler>();

        _uiHandler.DisplayTitle();
        _uiHandler.DisplayInitializing();

        await _dbContext.Database.MigrateAsync(cancellationToken);

        await RunLoop(cancellationToken);
    }

    private async Task RunLoop(CancellationToken cancellationToken)
    {
        var userName = string.Empty;
        var repositoryName = string.Empty;

        while (true)
        {
            _uiHandler.DisplayDescription();

            userName = await _mainLoopService.GetValidatedUserName(userName, cancellationToken);
            repositoryName = await _mainLoopService.GetValidatedRepositoryName(userName, repositoryName, cancellationToken);

            var gitHubCommits = (await _mainLoopService.GetCommits(userName, repositoryName, cancellationToken)).ToArray();

            _mainLoopService.DisplayResult(gitHubCommits, repositoryName);

            await _mainLoopService.SaveData(userName, repositoryName, gitHubCommits, cancellationToken);

            _uiHandler.DisplayLine();

            userName = string.Empty;
            repositoryName = string.Empty;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _asyncServiceScope.DisposeAsync();
    }
}