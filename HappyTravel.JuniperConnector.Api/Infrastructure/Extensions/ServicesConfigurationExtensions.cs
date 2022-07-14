using System.Reflection;
using HappyTravel.BaseConnector.Api.Infrastructure.Environment;
using HappyTravel.BaseConnector.Api.Infrastructure.Extensions;
using HappyTravel.BaseConnector.Api.Services.Accommodations;
using HappyTravel.BaseConnector.Api.Services.Availabilities.AccommodationAvailabilities;
using HappyTravel.BaseConnector.Api.Services.Availabilities.Cancellations;
using HappyTravel.BaseConnector.Api.Services.Availabilities.RoomContractSetAvailabilities;
using HappyTravel.BaseConnector.Api.Services.Availabilities.WideAvailabilities;
using HappyTravel.BaseConnector.Api.Services.Bookings;
using HappyTravel.BaseConnector.Api.Services.Locations;
using HappyTravel.ErrorHandling.Extensions;
using HappyTravel.HttpRequestLogger;
using HappyTravel.JuniperConnector.Api.Services.Accommodations;
using HappyTravel.JuniperConnector.Api.Services.Availabilities.AccommodationAvailabilities;
using HappyTravel.JuniperConnector.Api.Services.Availabilities.Cancellations;
using HappyTravel.JuniperConnector.Api.Services.Availabilities.RoomContractSetAvailabilities;
using HappyTravel.JuniperConnector.Api.Services.Availabilities.WideAvailabilities;
using HappyTravel.JuniperConnector.Api.Services.Bookings;
using HappyTravel.JuniperConnector.Api.Services.Locations;
using HappyTravel.VaultClient;
using Microsoft.OpenApi.Models;

namespace HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;

public static class ServicesConfigurationExtensions
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddProblemDetailsErrorHandling();

        using var vaultClient = new VaultClient.VaultClient(new VaultOptions
        {
            BaseUrl = new Uri(EnvironmentVariableHelper.Get("Vault:Endpoint", builder.Configuration) ??
                                throw new Exception("Could not find vault endpoint environment variable")),
            Engine = builder.Configuration["Vault:Engine"],
            Role = builder.Configuration["Vault:Role"]
        });

        vaultClient.Login(EnvironmentVariableHelper.Get("Vault:Token", builder.Configuration), LoginMethods.Token)?.GetAwaiter().GetResult();

        builder.Services.AddBaseConnectorServices(builder.Configuration, builder.Environment, vaultClient, Connector.Name);

        builder.Services.AddTransient<HttpRequestLoggingHandler>();

        builder.Services.AddTransient<IAccommodationService, AccommodationService>()
          .AddTransient<IAccommodationAvailabilityService, AccommodationAvailabilityService>()
          .AddTransient<IDeadlineService, DeadlineService>()
          .AddTransient<IRoomContractSetAvailabilityService, RoomContractSetAvailabilityService>()
          .AddTransient<IWideAvailabilitySearchService, WideAvailabilitySearchService>()
          .AddTransient<IBookingService, BookingService>()
          .AddTransient<ILocationService, LocationService>();

        builder.Services.AddHealthChecks();

        builder.Services.AddSwaggerGen(options =>
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
        });

        builder.Services.AddSwaggerGenNewtonsoftSupport();
    }
}