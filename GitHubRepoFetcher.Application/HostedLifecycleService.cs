using GitHubRepoFetcher.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace GitHubRepoFetcher.Application
{
    public sealed class HostedLifecycleService(
        DatabaseContext dbContext,
        IMainLoopService mainLoopService,
        IUIHandler uiHandler,
        IGitHubRepositoryService gitHubRepositoryService)
        : IHostedService
    {
        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            uiHandler.DisplayTitle();
            uiHandler.DisplayInitializing();

            await dbContext.Database.MigrateAsync(cancellationToken);

            await RunLoop(cancellationToken);
        }

        private async Task RunLoop(CancellationToken cancellationToken)
        {
            var userName = string.Empty;
            var repositoryName = string.Empty;

            while (true)
            {
                uiHandler.DisplayDescription();

                userName = await mainLoopService.GetValidatedUserName(userName, cancellationToken);
                repositoryName = await mainLoopService.GetValidatedRepositoryName(userName, repositoryName, cancellationToken);

                var gitHubCommits = (await mainLoopService.GetCommits(userName, repositoryName, cancellationToken)).ToArray();

                mainLoopService.DisplayResult(gitHubCommits, repositoryName);

                await mainLoopService.SaveData(userName, repositoryName, gitHubCommits, cancellationToken);

                userName = string.Empty;
                repositoryName = string.Empty;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
