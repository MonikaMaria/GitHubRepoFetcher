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

//RefitSettings settings = new()
//{
//    AuthorizationHeaderValueGetter = (message, cancellationToken) => GetTokenAsync() 
//}

var host = CreateConfiguredBuilder(args);
host.Start();

IHost CreateConfiguredBuilder(string[] strings)
{
    return Host.CreateDefaultBuilder(strings)
        .ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.json", true, true);
            config.AddCommandLine(strings);
        })
        .ConfigureLogging((context, config) => config.AddConfiguration(context.Configuration.GetSection("Logging")))
        .ConfigureServices(services =>
        {
            services
                .AddRefitClient<IGitHubApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.github.com"));

            services.AddTransient<IGitHubRepositoryService, GitHubRepositoryService>();
            services.AddTransient<IUIHandler, UIHandler>();
            services.AddTransient<IMainLoopService, MainLoopService>();

            services.AddDbContext<DatabaseContext>((sp, options) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var migrationAssemblyName = typeof(DatabaseContext).GetTypeInfo().Assembly.GetName().Name;
                var connectionString = config.GetSection("Database")["ConnectionString"];
                options.UseLoggerFactory(new LoggerFactory());
                options.UseSqlServer(connectionString,
                    sqlOptions => { sqlOptions.MigrationsAssembly(migrationAssemblyName); });
            });

           services.AddHostedService<HostedLifecycleService>();
        })
        .Build();
}