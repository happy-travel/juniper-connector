using HappyTravel.HttpRequestAuditLogger.Extensions;
using HappyTravel.HttpRequestLogger;
using HappyTravel.JuniperConnector.Api.Services;
using HappyTravel.JuniperConnector.Common.Settings;
using HappyTravel.JuniperConnector.Data;
using HappyTravel.VaultClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Prometheus;
using System.Net;
using System.Reflection;

namespace HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureSwagger(this IServiceCollection services)    
        => services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1.0", new OpenApiInfo { Title = "HappyTravel.com Juniper API", Version = "v1.0" });

            var xmlCommentsFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlCommentsFilePath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFileName);

            options.IncludeXmlComments(xmlCommentsFilePath);
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    Array.Empty<string>()
                }
            });
        })
        .AddSwaggerGenNewtonsoftSupport();


    public static IServiceCollection ConfigureDatabaseOptions(this IServiceCollection services, IVaultClient vaultClient, IConfiguration configuration)
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
        }, ServiceLifetime.Singleton, ServiceLifetime.Singleton);
    }


    public static IServiceCollection ConfigureApiConnictionSettings(this IServiceCollection services, IVaultClient vaultClient)
    {
        var apiConnectionOptions = vaultClient.Get("juniper-connector/api-connection").GetAwaiter().GetResult();

        return services.Configure<ApiConnectionSettings>(options =>
        {
            options.AvailTransactionsEndPoint = apiConnectionOptions["availTransactionsEndPoint"];
            options.Email = apiConnectionOptions["email"];
            options.Password = apiConnectionOptions["password"];
        });
    }


    public static IServiceCollection ConfigureHttpClients(this IServiceCollection services, IConfiguration configuration, IVaultClient vaultClient)
    {
        var fukuokaOptions = vaultClient.Get(configuration["Fukuoka:Options"]).GetAwaiter().GetResult();

        services.AddTransient<JuniperAvailTransactionsClient>()
            .AddHttpClient(Common.Constants.HttpAvailClientName, client =>
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

        return services;
    }
}
