using HappyTravel.HttpRequestAuditLogger.Extensions;
using HappyTravel.HttpRequestLogger;
using HappyTravel.JuniperConnector.Api.Services;
using HappyTravel.JuniperConnector.Common.JuniperService;
using HappyTravel.JuniperConnector.Common.Settings;
using HappyTravel.JuniperConnector.Data;
using HappyTravel.VaultClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Prometheus;
using System.Net;

namespace HappyTravel.JuniperConnector.Api.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureDatabaseOptions(this IServiceCollection services,
        IConfiguration configuration, IVaultClient vaultClient)
    {
        var databaseOptions = vaultClient.Get(configuration["Database:Options"]).GetAwaiter().GetResult();

        return services.AddDbContext<JuniperContext>(options =>
        {
            var host = databaseOptions["host"];
            var port = databaseOptions["port"];
            var password = databaseOptions["password"];
            var userId = databaseOptions["userId"];

            var connectionString = configuration["Database:ConnectionString"];
            options.UseNpgsql(string.Format(connectionString, host, port, userId, password), builder =>
            {
                builder.UseNetTopologySuite();
                builder.EnableRetryOnFailure();
            });
            options.UseInternalServiceProvider(null);
            options.EnableSensitiveDataLogging(false);
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }, ServiceLifetime.Transient, ServiceLifetime.Transient);
    }


    public static IServiceCollection ConfigureApiConnictionSettings(this IServiceCollection services, IVaultClient vaultClient)
    {
        var apiConnectionOptions = vaultClient.Get("juniper-connector/api-connection").GetAwaiter().GetResult();

        return services.Configure<ApiConnectionSettings>(options =>
            {
                options.AvailEndPoint = apiConnectionOptions["availEndPoint"];
                options.CheckTransactionsEndPoint = apiConnectionOptions["checkTransactionsEndPoint"];
                options.Email = apiConnectionOptions["email"];
                options.Password = apiConnectionOptions["password"];
            });
    }


    public static IServiceCollection ConfigureHttpClients(this IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
    {
        var fukuokaOptions = vaultClient.Get(configuration["Fukuoka:Options"]).GetAwaiter().GetResult();

        ConfigureHttpAvailClient();
        ConfigureHttpCkeckTransactionsClient();

        return services.AddTransient<JuniperClient>();


        void ConfigureHttpAvailClient()
        {
            services.AddHttpClient(Common.Constants.HttpAvailClientName, client =>
            {
                client.DefaultRequestHeaders.Add(HeaderNames.AcceptEncoding, "gzip, deflate");
            })
            .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            })
            .AddHttpClientRequestLogging(configuration: configuration)
            .UseHttpClientMetrics()
            .AddHttpRequestAudit(options =>
            {
                options.Endpoint = fukuokaOptions["endpoint"];
                options.LoggingCondition = request =>
                {
                    var url = request.Headers.GetValues("SOAPAction").FirstOrDefault();
                    return !string.IsNullOrEmpty(url) && (url.Contains("/HotelAvail"));
                };
            });
        }


        void ConfigureHttpCkeckTransactionsClient()
        {
            services.AddHttpClient(Common.Constants.HttpCkeckTransactionsClientName, client =>
            {
                client.DefaultRequestHeaders.Add(HeaderNames.AcceptEncoding, "gzip, deflate");
            })
            .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            })
            .AddHttpClientRequestLogging(configuration: configuration)
            .UseHttpClientMetrics()
            .AddHttpRequestAudit(options =>
            {
                options.Endpoint = fukuokaOptions["endpoint"];
                options.LoggingCondition = request =>
                {
                    var url = request.Headers.GetValues("SOAPAction").FirstOrDefault();
                    return !string.IsNullOrEmpty(url) && (url.Contains("/HotelCheckAvail"));
                };
            });
        }
    }
}
