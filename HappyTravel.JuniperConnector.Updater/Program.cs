using HappyTravel.ConsulKeyValueClient.ConfigurationProvider.Extensions;
using HappyTravel.JuniperConnector.Common.Infrastructure.Environment;
using HappyTravel.JuniperConnector.Updater;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using HappyTravel.StdOutLogger.Extensions;
using Microsoft.Extensions.Logging;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }


    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => webBuilder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                var environment = hostingContext.HostingEnvironment;

                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true,
                        reloadOnChange: true);

                var consulHttpAddr = Environment.GetEnvironmentVariable("CONSUL_HTTP_ADDR") ?? throw new InvalidOperationException("Consul endpoint is not set");
                var consulHttpToken = Environment.GetEnvironmentVariable("CONSUL_HTTP_TOKEN") ?? throw new InvalidOperationException("Consul HTTP token is not set");

                config.AddConsulKeyValueClient(consulHttpAddr, ConnectorUpdaterConsulName, consulHttpToken, environment.EnvironmentName, optional: environment.IsLocal());

                config.AddEnvironmentVariables();
            })
                 .ConfigureLogging((hostingContext, logging) =>
                 {
                     logging.ClearProviders()
                        .AddConfiguration(hostingContext.Configuration.GetSection("Logging"));

                     var env = hostingContext.HostingEnvironment;
                     if (env.IsEnvironment("Local"))
                         logging.AddConsole();
                     else
                     {
                         logging.AddStdOutLogger(setup =>
                         {
                             setup.IncludeScopes = false;
                             setup.UseUtcTimestamp = true;
                         });
                     }

                 }).UseStartup<Startup>());


    public const string ConnectorUpdaterConsulName = "juniper-connector-updater";
}