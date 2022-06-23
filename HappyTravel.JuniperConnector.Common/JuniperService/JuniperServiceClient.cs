using HappyTravel.JuniperConnector.Common.Settings;
using JuniperServiceReference;
using System.ServiceModel;

namespace HappyTravel.JuniperConnector.Common.JuniperService
{
    public abstract class JuniperServiceClient
    {
        public JuniperServiceClient(IHttpMessageHandlerFactory factory, ApiConnectionSettings options)
        {
            _factory = factory;
            _options = options;
            _login = GetLogin();
        }


        protected void ConfigureClient<T>(ClientBase<T> client, string clientName) where T : class
        {
            client.Endpoint.EndpointBehaviors.Add(new HttpMessageHandlerBehavior(_factory, clientName));
        }


        protected BasicHttpBinding GetBasicHttpBinding(string name)
            => new BasicHttpBinding(BasicHttpSecurityMode.Transport)
            {
                Name = name,
                MaxReceivedMessageSize = MaxReceivedMessageSizeBytes,
                MaxBufferPoolSize = MaxBufferPoolSizeBytes,
                MaxBufferSize = MaxBufferSizeBytes,
                SendTimeout = TimeSpan.FromMinutes(5)
            };


        protected EndpointAddress GetEndpointAddress(string endPoint)
            => new EndpointAddress(new Uri(endPoint));


        protected string GetErrorMessage(JP_ErrorType[] errors)
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


        private const long MaxReceivedMessageSizeBytes = 20000000;
        private const long MaxBufferPoolSizeBytes = 20000000;
        private const int MaxBufferSizeBytes = 20000000;


        private readonly IHttpMessageHandlerFactory _factory;
        protected readonly ApiConnectionSettings _options;
        protected readonly JP_Login _login;
    }
}
