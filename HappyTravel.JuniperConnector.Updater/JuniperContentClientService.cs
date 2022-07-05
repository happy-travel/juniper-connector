using CSharpFunctionalExtensions;
using HappyTravel.JuniperConnector.Common;
using HappyTravel.JuniperConnector.Common.Infrastructure;
using HappyTravel.JuniperConnector.Common.JuniperService;
using HappyTravel.JuniperConnector.Common.Settings;
using HappyTravel.JuniperConnector.Updater.Infrastructure.Logging;
using JuniperServiceReference;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyTravel.JuniperConnector.Updater;

public class JuniperContentClientService : JuniperServiceClient
{
    public JuniperContentClientService(IHttpMessageHandlerFactory factory, IOptions<ApiConnectionSettings> options,
        ILogger<JuniperContentClientService> logger) : base(factory, options.Value)
    {
        _staticDataClient = InitializeStaticDataClient();
        _logger = logger;
    }


    public async Task<Result<List<JP_HotelContent>>> GetHotelContents(IEnumerable<string> hotelCodes)
    {
        var request = new JP_HotelContentRQ
        {
            HotelContentList = hotelCodes.Select(c =>
                    new JP_HotelSimpleContent
                    {
                        Code = c
                    })
                .ToArray(),
            AdvancedOptions = new JP_HotelDataAdvancedOptions
            {
                ShowGiataCode = true
            }
        }
        .SetDefaultProperty(_login);

        try
        {
            var response = await _staticDataClient.HotelContentAsync(request);

            if (response.Errors?.Length > 0)
            {
                var errorMessage = GetErrorMessage(response.Errors);

                throw new Exception(errorMessage);
            }

            return response.Contents.HotelContent.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogHotelContentRequestFailed(ex);
            return Result.Failure<List<JP_HotelContent>>("HotelContent request failed");
        }
    }


    public async Task<Result<JP_HotelPortfolio>> GetHotelPortfolio(int recordsPerPage, string nextToken)
    {
        var request = new JP_HotelPortfolioRQ
        {
            RecordsPerPage = recordsPerPage,
            RecordsPerPageSpecified = true,
            Token = nextToken
        }
        .SetDefaultProperty(_login);

        try
        {
            var response = await _staticDataClient.HotelPortfolioAsync(request);

            if (response.Errors?.Length > 0)
            {
                var errorMessage = GetErrorMessage(response.Errors);

                throw new Exception(errorMessage);
            }

            return response.HotelPortfolio;
        }
        catch (Exception ex)
        {
            _logger.LogHotelPortfolioRequestFailed(ex);
            return Result.Failure<JP_HotelPortfolio>("HotelPortfolio request failed");
        }
    }


    public async Task<Result<List<JP_Zone>>> GetZoneList()
    {
        var request = new JP_ZoneListRQ
        {
            ZoneListRequest = new JP_ZoneListRequest
            {
                ProductType = JP_ProductType.HOT,
                ShowIATA = true,
                MaxLevel = 1
            }
        }
        .SetDefaultProperty(_login);

        try
        {
            var response = await _staticDataClient.ZoneListAsync(request);

            if (response.Errors?.Length > 0)
            {
                var errorMessage = GetErrorMessage(response.Errors);

                throw new Exception(errorMessage);
            }

            return response.ZoneList.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogZoneListRequestFailed(ex);
            return Result.Failure<List<JP_Zone>>("ZoneList request failed");
        }
    }


    private StaticDataTransactionsClient InitializeStaticDataClient()
    {
        var client = new StaticDataTransactionsClient(GetBasicHttpBinding("JuniperStaticDataServiceSoap"), GetEndpointAddress(_options.StaticDataEndPoint));
        ConfigureClient(client, Constants.HttpStaticDataClientNames);

        return client;
    }


    private readonly StaticDataTransactionsClient _staticDataClient;
    private readonly ILogger<JuniperContentClientService> _logger;
}
