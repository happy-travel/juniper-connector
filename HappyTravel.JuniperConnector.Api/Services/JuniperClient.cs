using CSharpFunctionalExtensions;
using HappyTravel.JuniperConnector.Common.Infrastructure;
using HappyTravel.JuniperConnector.Common.JuniperService;
using HappyTravel.JuniperConnector.Common.Settings;
using JuniperServiceReference;
using Microsoft.Extensions.Options;
using System.ServiceModel;

namespace HappyTravel.JuniperConnector.Api.Services;

public class JuniperClient
{
    public JuniperClient (IHttpMessageHandlerFactory factory, IOptions<ApiConnectionSettings> options)
    {
        _factory = factory;
        _options = options.Value;
        _login = GetLogin();
    }
   

    public async Task<Result<JP_Results>> GetHotelAvailability(JP_HotelAvail request)
    {
        request.SetDefaultProperty(_login);

        var client = CreateAvailTransactionsClient();
        var response = await client.HotelAvailAsync(request);

        if (response.Errors?.Length > 0)
        {
            var errorMessage = GetErrorMessage(response.Errors);

            return Result.Failure<JP_Results>(errorMessage);
        }

        return response.Results;
    }


    private AvailTransactionsClient CreateAvailTransactionsClient()
    {
        var client = new AvailTransactionsClient(GetBasicHttpBinding("JuniperAvailServiceSoap"), GetEndpointAddress(_options.AvailEndPoint));
        ConfigureClient(client, Common.Constants.HttpAvailClientName);

        return client;
    }


    private void ConfigureClient<T>(ClientBase<T> client, string clientName) where T : class
    {
        client.Endpoint.EndpointBehaviors.Add(new HttpMessageHandlerBehavior(_factory, clientName));
    }



    private BasicHttpBinding GetBasicHttpBinding(string name)
        => new BasicHttpBinding(BasicHttpSecurityMode.Transport)
        {
            Name = name,
            MaxReceivedMessageSize = MaxReceivedMessageSize,
            MaxBufferPoolSize = MaxBufferPoolSize,
            MaxBufferSize = MaxBufferSize,
            SendTimeout = TimeSpan.FromMinutes(5)
        };


    private EndpointAddress GetEndpointAddress(string endPoint)
        => new EndpointAddress(new Uri(endPoint));


    private string GetErrorMessage(JP_ErrorType[] errors)
    {
        const string errorMessage = "Code: `{0}`, Text: `{1}`, Type: `{2}`";
        var errorMessages = errors.Select(error =>
             string.Format(errorMessage, error.Code, error.Text, error.Type))
        .ToList();

        return string.Join("; ", errorMessages);
    }


    private JP_Login GetLogin()
      => new JP_Login
      {
          Email = _options.Email,
          Password = _options.Password
      };


    private const long MaxReceivedMessageSize = 20000000;
    private const long MaxBufferPoolSize = 20000000;
    private const int MaxBufferSize = 20000000;


    private readonly IHttpMessageHandlerFactory _factory;
    private readonly ApiConnectionSettings _options;
    private readonly JP_Login _login;
}
