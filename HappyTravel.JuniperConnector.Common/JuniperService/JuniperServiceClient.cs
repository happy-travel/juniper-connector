using HappyTravel.JuniperConnector.Common.Settings;
using JuniperServiceReference;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.ServiceModel;

namespace HappyTravel.JuniperConnector.Common.JuniperService;

public class JuniperServiceClient : IJuniperServiceClient
{
    public JuniperServiceClient(IHttpMessageHandlerFactory factory, IOptions<ApiConnectionSettings> options)
    {
        _options = options.Value;
        _staticDataClient = new StaticDataTransactionsClient(GetBasicHttpBinding("JuniperStaticDataServiceSoap"), GetEndpointAddress(_options.StaticDataEndPoint));
        _staticDataClient.Endpoint.EndpointBehaviors.Add(new HttpMessageHandlerBehavior(factory, Constants.HttpStaticDataClientNames));
        _availClient = new AvailTransactionsClient(GetBasicHttpBinding("JuniperAvailServiceSoap"), GetEndpointAddress(_options.AvailEndPoint));
        _availClient.Endpoint.EndpointBehaviors.Add(new HttpMessageHandlerBehavior(factory, Constants.HttpAvailClientNames));
        _login = GetLogin();
    }


    private BasicHttpBinding GetBasicHttpBinding(string name)
            => new BasicHttpBinding(BasicHttpSecurityMode.Transport)
            {
                Name = name,
                MaxReceivedMessageSize = 20000000,
                MaxBufferPoolSize = 20000000,
                MaxBufferSize = 20000000,
                SendTimeout = TimeSpan.FromMinutes(5)
            };


    private EndpointAddress GetEndpointAddress(string endPoint)
        => new EndpointAddress(new Uri(endPoint));


    private JP_Login GetLogin()
            => new JP_Login
            {
                Email = _options.Email,
                Password = _options.Password
            };    


    private readonly StaticDataTransactionsClient _staticDataClient;
    private readonly AvailTransactionsClient _availClient;
    private readonly ApiConnectionSettings _options;
    private readonly JP_Login _login;
}
