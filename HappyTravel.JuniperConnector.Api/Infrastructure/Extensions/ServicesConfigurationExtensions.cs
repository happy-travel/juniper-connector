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
using HappyTravel.JuniperConnector.Api.Services.Availabilities;
using HappyTravel.JuniperConnector.Api.Services.Availabilities.AccommodationAvailabilities;
using HappyTravel.JuniperConnector.Api.Services.Availabilities.Cancellations;
using HappyTravel.JuniperConnector.Api.Services.Availabilities.RoomContractSetAvailabilities;
using HappyTravel.JuniperConnector.Api.Services.Availabilities.WideAvailabilities;
using HappyTravel.JuniperConnector.Api.Services.Bookings;
using HappyTravel.JuniperConnector.Api.Services.Caching;
using HappyTravel.JuniperConnector.Api.Services.Locations;
using HappyTravel.JuniperConnector.Common;
using HappyTravel.JuniperConnector.Data;
using HappyTravel.VaultClient;

namespace HappyTravel.JuniperConnector.Api.Infrastructure.Extensions;

public static class ServicesConfigurationExtensions
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
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
        builder.Services.AddTransient<MultilingualAccommodationMapper>();
        builder.Services.AddTransient<JuniperSerializer>();
        builder.Services.AddTransient<JuniperContext>();

        builder.Services.AddTransient<IAccommodationService, AccommodationService>()
          .AddTransient<IAccommodationAvailabilityService, AccommodationAvailabilityService>()
          .AddTransient<IDeadlineService, DeadlineService>()
          .AddTransient<IRoomContractSetAvailabilityService, RoomContractSetAvailabilityService>()
          .AddTransient<IWideAvailabilitySearchService, WideAvailabilitySearchService>()
          .AddTransient<IBookingService, BookingService>()
          .AddTransient<ILocationService, LocationService>();

        builder.Services.AddTransient<WideAvailabilitySearchRequestExecutor>()
            .AddTransient<AvailabilitySearchMapper>();

        builder.Services.AddTransient<AvailabilityRequestStorage>()
            .AddTransient<AvailabilitySearchResultStorage>();

        builder.Services.ConfigureApiConnictionSettings(vaultClient)
            .ConfigureHttpClients(builder.Configuration, vaultClient);

        builder.Services.AddHealthChecks()
            .AddDbContextCheck<JuniperContext>();

        builder.Services.AddProblemDetailsErrorHandling()
           .ConfigureSwagger()
           .ConfigureDatabaseOptions(vaultClient, builder.Configuration);
    }
}