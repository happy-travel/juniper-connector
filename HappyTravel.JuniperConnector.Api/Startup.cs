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
using System.Reflection;

namespace HappyTravel.JuniperConnector.Api;

public class Startup
{
    public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        Configuration = configuration;
        HostEnvironment = hostEnvironment;
    }


    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddProblemDetailsErrorHandling();

        using var vaultClient = new VaultClient.VaultClient(new VaultOptions
        {
            BaseUrl = new Uri(EnvironmentVariableHelper.Get("Vault:Endpoint", Configuration) ??
                                throw new Exception("Could not find vault endpoint environment variable")),
            Engine = Configuration["Vault:Engine"],
            Role = Configuration["Vault:Role"]
        });

        vaultClient.Login(EnvironmentVariableHelper.Get("Vault:Token", Configuration), LoginMethods.Token)?.GetAwaiter().GetResult();

        services.AddBaseConnectorServices(Configuration, HostEnvironment, vaultClient, Program.ConnectorName);

        services.AddTransient<HttpRequestLoggingHandler>();

        services.AddTransient<IAccommodationService, AccommodationService>()
          .AddTransient<IAccommodationAvailabilityService, AccommodationAvailabilityService>()
          .AddTransient<IDeadlineService, DeadlineService>()
          .AddTransient<IRoomContractSetAvailabilityService, RoomContractSetAvailabilityService>()
          .AddTransient<IWideAvailabilitySearchService, WideAvailabilitySearchService>()
          .AddTransient<IBookingService, BookingService>()
          .AddTransient<ILocationService, LocationService>();

        services.AddHealthChecks();

        services.AddSwaggerGen(options =>
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

        services.AddSwaggerGenNewtonsoftSupport();
    }


    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<Startup>();

        app.UseProblemDetailsExceptionHandler(env, logger);

        app.ConfigureBaseConnector();

        app.UseSwagger()
            .UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1.0/swagger.json", "HappyTravel.com Juniper Connector API");
                options.RoutePrefix = string.Empty;
            });
    }


    public IConfiguration Configuration { get; }
    public IHostEnvironment HostEnvironment { get; }
}
