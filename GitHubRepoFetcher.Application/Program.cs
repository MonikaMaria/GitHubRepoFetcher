using GitHubRepoFetcher.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Refit;
using GitHubRepoFetcher.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using GitHubRepoFetcher.Application.Services;

RefitSettings refitSettings = new()
{
    AuthorizationHeaderValueGetter = (_, cancellationToken) => AuthBearerTokenFactory.GetBearerTokenAsync(cancellationToken)
};

var host = CreateConfiguredBuilder(args);
host.Start();

IHost CreateConfiguredBuilder(string[] strings)
{
    return Host.CreateDefaultBuilder(strings)
        .ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile(path: "appsettings.json", optional: true, reloadOnChange: true);
            config.AddUserSecrets<Program>();
            config.AddCommandLine(strings);
        })
        .ConfigureLogging((context, config) => config.AddConfiguration(context.Configuration.GetSection("Logging")))
        .ConfigureServices((context, services) =>
        {
            var connectionString = context.Configuration.GetSection("Database")["ConnectionString"];
            var gitHubBaseAddress = context.Configuration.GetSection("GitHub")["BaseAddress"];

            services
                .AddRefitClient<IGitHubApiClient>(refitSettings)
                .ConfigureHttpClient(c =>
                {
                    if (gitHubBaseAddress != null)
                    {
                        c.BaseAddress = new Uri(gitHubBaseAddress);
                    }
                });

            services.AddTransient<IGitHubRepositoryService, GitHubRepositoryService>();
            services.AddTransient<IUIHandler, UIHandler>();
            services.AddTransient<IMainLoopService, MainLoopService>();

            services.AddDbContext<DatabaseContext>((_, options) =>
            {
                var migrationAssemblyName = typeof(DatabaseContext).GetTypeInfo().Assembly.GetName().Name;
                options.UseLoggerFactory(new LoggerFactory());
                options.UseSqlServer(connectionString,
                    sqlOptions => { sqlOptions.MigrationsAssembly(migrationAssemblyName); });
            });

           services.AddHostedService<HostedLifecycleService>();
        })
        .Build();
}