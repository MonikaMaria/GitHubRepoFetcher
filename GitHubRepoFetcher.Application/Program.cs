using GitHubRepoFetcher.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Refit;
using GitHubRepoFetcher.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Reflection;
using System;

//RefitSettings settings = new()
//{
//    AuthorizationHeaderValueGetter = (message, cancellationToken) => GetTokenAsync() 
//}

DisplayTitle();
AnsiConsole.MarkupLine("[yellow]Initializing...[/]");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", true, true);
        config.AddCommandLine(args);
    })
    .ConfigureLogging((context, config) => config.AddConfiguration(context.Configuration.GetSection("Logging")))
    .ConfigureServices(services =>
    {
        services
            .AddRefitClient<IGitHubApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.github.com"));

        services.AddTransient<IGitHubRepositoryService, GitHubRepositoryService>(); 
        //services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.AddDbContext<DatabaseContext>((sp, options) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();

            var migrationAssemblyName = typeof(DatabaseContext).GetTypeInfo().Assembly.GetName().Name;
            var connectionString = config.GetSection("Database")["ConnectionString"];
            options.UseLoggerFactory(new LoggerFactory());
            options.UseSqlServer(connectionString,
                sqlOptions => { sqlOptions.MigrationsAssembly(migrationAssemblyName); });
        });
    })
    .Build();

host.Start();

using var scope = host.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

// Apply migrations
dbContext.Database.Migrate();

var gitHubRepositoryService = host.Services.GetRequiredService<IGitHubRepositoryService>();

var userName = string.Empty;
var repositoryName = string.Empty;

while (true)
{
    var cts = new CancellationTokenSource();
    AnsiConsole.MarkupLine("[deepskyblue1]→ Fetch commits from GitHub repository. Please provide data below.[/]");

    userName = await GetProperUserName(gitHubRepositoryService, userName, cts.Token);
    repositoryName = await GetProperRepositoryName(gitHubRepositoryService, userName, repositoryName, cts.Token);

    var gitHubCommits = (await gitHubRepositoryService.GetGitHubCommitsAsync(userName, repositoryName, cts.Token)).ToArray();

    DisplayResult(gitHubRepositoryService, gitHubCommits);

    await gitHubRepositoryService.SaveCommits(userName, repositoryName, gitHubCommits, cts.Token);

    cts.Dispose();

    userName = string.Empty;
    repositoryName = string.Empty;
}

void DisplayTitle()
{
    var titleRule = new Rule("GitHub Repo Fetcher")
    {
        Justification = Justify.Left
    };

    AnsiConsole.Write(titleRule);
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

void DisplayResult(IGitHubRepositoryService gitHubRepositoryService1, IEnumerable<GitHubCommitItem> gitHubCommitItems)
{
    Console.WriteLine("---");
    var commitsToDisplay = gitHubRepositoryService1.MapCommitsToDisplay(gitHubCommitItems, repositoryName);
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