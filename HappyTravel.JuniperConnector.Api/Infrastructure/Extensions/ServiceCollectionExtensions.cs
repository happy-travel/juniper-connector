using HappyTravel.JuniperConnector.Data;
using HappyTravel.VaultClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
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
        }, ServiceLifetime.Transient, ServiceLifetime.Transient);
    }
}
