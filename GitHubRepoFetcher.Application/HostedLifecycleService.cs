using GitHubRepoFetcher.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GitHubRepoFetcher.Application
{
    public sealed class HostedLifecycleService : IHostedService
    {
        private readonly DatabaseContext _dbContext;
        private readonly IGitHubRepositoryService _gitHubRepositoryService;
        private readonly ILogger _logger;

        public HostedLifecycleService(DatabaseContext dbContext,
            IGitHubRepositoryService gitHubRepositoryService,
            ILogger<HostedLifecycleService> logger,
            IHostApplicationLifetime appLifetime)
        {
            _dbContext = dbContext;
            _gitHubRepositoryService = gitHubRepositoryService;
            _logger = logger;

            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);
        }

        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            //DisplayTitle();
            AnsiConsole.MarkupLine("[yellow]Initializing...[/]");
            _dbContext.Database.Migrate();


            var userName = string.Empty;
            var repositoryName = string.Empty;

            while (true)
            {
                var cts = new CancellationTokenSource();
                AnsiConsole.MarkupLine("[deepskyblue1]→ Fetch commits from GitHub repository. Please provide data below.[/]");

                userName = await GetProperUserName(_gitHubRepositoryService, userName, cts.Token);
                repositoryName = await GetProperRepositoryName(_gitHubRepositoryService, userName, repositoryName, cts.Token);

                var gitHubCommits = (await _gitHubRepositoryService.GetGitHubCommitsAsync(userName, repositoryName, cts.Token)).ToArray();

                DisplayResult(gitHubCommits, repositoryName);

                await _gitHubRepositoryService.SaveCommits(userName, repositoryName, gitHubCommits, cts.Token);

                cts.Dispose();

                userName = string.Empty;
                repositoryName = string.Empty;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void OnStarted()
        {
            _logger.LogInformation("4. OnStarted has been called.");
        }

        private void OnStopping()
        {
            _logger.LogInformation("5. OnStopping has been called.");
        }

        private void OnStopped()
        {
            _logger.LogInformation("9. OnStopped has been called.");
        }


        static async Task<string> GetProperUserName(IGitHubRepositoryService gitHubRepositoryService, string userName, CancellationToken cancellationToken)
        {
            while (string.IsNullOrEmpty(userName))
            {
                userName = AnsiConsole.Prompt(new TextPrompt<string>("[lightseagreen]GitHub user name:[/]"));

                var userExists = await gitHubRepositoryService.CheckUserExistsAsync(userName, cancellationToken);
                if (userExists is false)
                {
                    AnsiConsole.MarkupLine($"[red3]User[/] [yellow]{userName}[/] [red3]does not exist.[/]");
                    userName = string.Empty;
                }
            }

            return userName;
        }

        static async Task<string> GetProperRepositoryName(IGitHubRepositoryService gitHubRepositoryService, string userName, string repositoryName, CancellationToken cancellationToken)
        {
            while (string.IsNullOrEmpty(repositoryName))
            {
                repositoryName = AnsiConsole.Prompt(new TextPrompt<string>("[lightseagreen]GitHub repository name:[/]"));

                var repositoryExists = await gitHubRepositoryService.CheckRepositoryExistsAsync(userName, repositoryName, cancellationToken);
                if (repositoryExists is false)
                {
                    AnsiConsole.MarkupLine($"[red3]Repository[/] [yellow]{repositoryName}[/] [red3]does not exist.[/]");
                    repositoryName = string.Empty;
                }
            }

            return repositoryName;
        }

        void DisplayResult(IEnumerable<GitHubCommitItem> gitHubCommitItems, string repositoryName)
        {
            Console.WriteLine("---");
            var commitsToDisplay = gitHubCommitItems.MapToCommitsToDisplay(repositoryName);
            AnsiConsole.MarkupLine("ALL COMMITS ON DEFAULT BRANCH:");
            AnsiConsole.MarkupLine("[gray][[repository name]]/[[sha]]: message [[committer]][/]");
            Console.WriteLine("---");
            if (commitsToDisplay.Any())
            {
                foreach (var commitToDisplay in commitsToDisplay)
                {
                    AnsiConsole.WriteLine(commitToDisplay.ToString());
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]No commits to display.[/]");
            }

            AnsiConsole.Write(new Rule());
        }
    }
}
