using CSharpFunctionalExtensions;
using HappyTravel.JuniperConnector.Api.Infrastructure.Logging;
using HappyTravel.JuniperConnector.Common.Infrastructure;
using HappyTravel.JuniperConnector.Common.JuniperService;
using HappyTravel.JuniperConnector.Common.Settings;
using JuniperServiceReference;
using Microsoft.Extensions.Options;

namespace HappyTravel.JuniperConnector.Api.Services;

public class JuniperAvailTransactionsClient : JuniperServiceClient
{
    public JuniperAvailTransactionsClient (IHttpMessageHandlerFactory factory, IOptions<ApiConnectionSettings> options,
        ILogger<JuniperAvailTransactionsClient> logger) : base (factory, options.Value)
    {
        _availTransactionsClient = CreateAvailTransactionsClient();
        _logger = logger;
    }
   

    public async Task<Result<JP_Results>> GetHotelAvailability(JP_HotelAvail request)
    {
        request.SetDefaultProperty(_login);

        try
        {
            var response = await _availTransactionsClient.HotelAvailAsync(request);

            if (response.Errors?.Length > 0)
            {
                var errorMessage = GetErrorMessage(response.Errors);

                return Result.Failure<JP_Results>(errorMessage);
            }

            return response.Results;
        }
        catch (Exception e)
        {
            _logger.LogSearchRequestFailed(e);
            return Result.Failure<JP_Results>("Request failed");
        }
    }


    private AvailTransactionsClient CreateAvailTransactionsClient()
    {
        var client = new AvailTransactionsClient(GetBasicHttpBinding("JuniperAvailServiceSoap"), GetEndpointAddress(_options.AvailTransactionsEndPoint));
        ConfigureClient(client, Common.Constants.HttpAvailClientName);

        return client;
    }


    private readonly AvailTransactionsClient _availTransactionsClient;
    private readonly ILogger<JuniperAvailTransactionsClient> _logger;
}
