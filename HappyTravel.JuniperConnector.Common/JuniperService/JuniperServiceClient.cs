using HappyTravel.JuniperConnector.Common.Infrastructure;
using HappyTravel.JuniperConnector.Common.Settings;
using JuniperServiceReference;
using Microsoft.Extensions.Options;

namespace HappyTravel.JuniperConnector.Common.JuniperService;

public class JuniperServiceClient : IJuniperServiceClient
{
    public JuniperServiceClient(IHttpMessageHandlerFactory factory, IOptions<ApiConnectionSettings> options)
    {
        _options = options.Value;
        _staticDataClient = InitializeStaticDataClient(factory);        
        _availClient = InitializeAvailClient(factory);
        _login = GetLogin();
    }
    


    public async Task<List<JP_Zone>> GetZoneList()
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

        var response = await GetZoneListResponse(request);

        return response.ZoneList.ToList();
    }


    public async Task<List<JP_HotelContent>> GetHotelContents(IEnumerable<string> hotelCodes)
    {
        var request = new JP_HotelContentRQ
        {
            HotelContentList = hotelCodes.Select(x => GetHotelContentRequest(x)).ToArray(),
            AdvancedOptions = new JP_HotelDataAdvancedOptions
            {
                ShowGiataCode = true
            }
        }
        .SetDefaultProperty(_login);

        var response = await GetHotelContentResponse(request);

        return response.Contents.HotelContent.ToList();

        JP_HotelSimpleContent GetHotelContentRequest(string hotelCode)
            => new JP_HotelSimpleContent
            {
                Code = hotelCode
            };
    }


    public async Task<JP_HotelPortfolio> GetHotelPortfolio(int recordsPerPage, string nextToken)
    {
        var request = new JP_HotelPortfolioRQ
        {
            RecordsPerPage = recordsPerPage,
            RecordsPerPageSpecified = true,
            Token = nextToken
        }
        .SetDefaultProperty(_login);

        var response = await GetHotelPortfolioResponse(request);

        return response.HotelPortfolio;
    }


    private async Task<JP_StaticDataRS> GetZoneListResponse(JP_ZoneListRQ request)
    {
        return await _staticDataClient.ZoneListAsync(request);
    }


    private async Task<JP_ContentRS> GetHotelContentResponse(JP_HotelContentRQ request)
    {
        return await _staticDataClient.HotelContentAsync(request);
    }


    private async Task<JP_StaticDataRS> GetHotelPortfolioResponse(JP_HotelPortfolioRQ request)
    {
        return await _staticDataClient.HotelPortfolioAsync(request);
    }    


    private JP_Login GetLogin()
        => new JP_Login
        {
            Email = _options.Email,
            Password = _options.Password
        };


    public StaticDataTransactionsClient InitializeStaticDataClient(IHttpMessageHandlerFactory factory)
        => JuniperServiceExtensions.InitializeClient<StaticDataTransactionsClient, StaticDataTransactions>(
            client: _staticDataClient,
            basicHttpBindingName: "JuniperStaticDataServiceSoap",
            endPoint: _options.StaticDataEndPoint,
            factory: factory,
            clientName: Constants.HttpStaticDataClientNames);    


    public AvailTransactionsClient InitializeAvailClient(IHttpMessageHandlerFactory factory)
        => JuniperServiceExtensions.InitializeClient<AvailTransactionsClient, AvailTransactions>(
            client: _availClient,
            basicHttpBindingName: "JuniperAvailServiceSoap",
            endPoint: _options.AvailEndPoint,
            factory: factory,
            clientName: Constants.HttpAvailClientNames);    


    private readonly StaticDataTransactionsClient _staticDataClient;
    private readonly AvailTransactionsClient _availClient;
    private readonly ApiConnectionSettings _options;
    private readonly JP_Login _login;
}
