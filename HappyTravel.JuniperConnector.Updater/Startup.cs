
using HappyTravel.JuniperConnector.Common;
using HappyTravel.JuniperConnector.Common.Infrastructure.Environment;
using HappyTravel.JuniperConnector.Data;
using HappyTravel.JuniperConnector.Updater.Infrastructure;
using HappyTravel.VaultClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.JuniperConnector.Updater;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }


    public void ConfigureServices(IServiceCollection services)
    {
        using var vaultClient = new VaultClient.VaultClient(new VaultOptions
        {
            BaseUrl = new Uri(EnvironmentVariableHelper.Get("Vault:Endpoint", Configuration)),
            Engine = Configuration["Vault:Engine"],
            Role = Configuration["Vault:Role"]
        });

        vaultClient.Login(EnvironmentVariableHelper.Get("Vault:Token", Configuration), LoginMethods.Token)?.GetAwaiter().GetResult();
        
        
        services.AddTransient<JuniperSerializer>();        

        services.AddHostedService<StaticDataUpdateHostedService>();
        services.AddTransient<DateTimeProvider>();

        ConfigureDatabaseOptions(services, vaultClient);
        ConfigureWorkers(services);

        services.AddHealthChecks();        
    }


    private void ConfigureWorkers(IServiceCollection services)
    {
        var workersToRun = Configuration.GetSection("Workers:WorkersToRun").Value;
        if (string.IsNullOrWhiteSpace(workersToRun))
        {
            
        }
        else
        {
            foreach (var workerName in workersToRun.Split(';').Select(s => s.Trim()))
            {
                
            }
        }
    }


    private void ConfigureDatabaseOptions(IServiceCollection services, VaultClient.VaultClient vaultClient)
    {
        var databaseOptions = vaultClient.Get(Configuration["Database:Options"]).GetAwaiter().GetResult();

        services.AddDbContext<JuniperContext>(options =>
        {
            var host = databaseOptions["host"];
            var port = databaseOptions["port"];
            var password = databaseOptions["password"];
            var userId = databaseOptions["userId"];

            var connectionString = Configuration["Database:ConnectionString"];
            options.UseNpgsql(string.Format(connectionString, host, port, userId, password), builder =>
            {
                builder.UseNetTopologySuite();
                builder.EnableRetryOnFailure();
            });
            options.UseInternalServiceProvider(null);
            options.EnableSensitiveDataLogging(false);
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }, ServiceLifetime.Singleton, ServiceLifetime.Singleton);
    }


    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseHealthChecks("/health");
    }


    public IConfiguration Configuration { get; }
}
