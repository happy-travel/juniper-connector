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
