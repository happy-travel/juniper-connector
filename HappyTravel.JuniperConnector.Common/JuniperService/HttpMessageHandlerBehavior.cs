using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace HappyTravel.JuniperConnector.Common.JuniperService;

public class HttpMessageHandlerBehavior : IEndpointBehavior
{
    public HttpMessageHandlerBehavior(IHttpMessageHandlerFactory factory, string serviceName)
    {
        _httpMessageHandler = () => factory.CreateHandler(serviceName);
    }


    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
    {
        bindingParameters.Add(new Func<HttpClientHandler, HttpMessageHandler>(handler => _httpMessageHandler()));
    }


    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) { }


    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }


    public void Validate(ServiceEndpoint endpoint) { }


    private readonly Func<HttpMessageHandler> _httpMessageHandler;
}
