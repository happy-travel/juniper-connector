using HappyTravel.JuniperConnector.Common.JuniperService;
using JuniperServiceReference;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace HappyTravel.JuniperConnector.Common.Infrastructure;

public static class JuniperServiceExtensions
{
    public static T InitializeClient<T, M>(T client, string basicHttpBindingName, string endPoint, IHttpMessageHandlerFactory factory, string clientName)
    where T : ClientBase<M>
    where M : class
    {
        client = GetClient<T>(basicHttpBindingName, endPoint);
        ConfigureClient<T, M>(client, factory, clientName);
        return client;
    }


    private static T GetClient<T>(string basicHttpBindingName, string endPoint)
    {
        Type type = typeof(T);

        ConstructorInfo constructor = type.GetConstructor(new Type[] { typeof(Binding), typeof(EndpointAddress) });

        object result = constructor.Invoke(new object[] { GetBasicHttpBinding(basicHttpBindingName), GetEndpointAddress(endPoint) });

        return (T)result;
    }


    private static void ConfigureClient<T, M>(T client, IHttpMessageHandlerFactory factory, string clientName) where T : ClientBase<M> where M : class
    {
        client.Endpoint.EndpointBehaviors.Add(new HttpMessageHandlerBehavior(factory, clientName));
    }


    private static BasicHttpBinding GetBasicHttpBinding(string name)
        => new BasicHttpBinding(BasicHttpSecurityMode.Transport)
        {
            Name = name,
            MaxReceivedMessageSize = MaxReceivedMessageSize,
            MaxBufferPoolSize = MaxBufferPoolSize,
            MaxBufferSize = MaxBufferSize,
            SendTimeout = TimeSpan.FromMinutes(5)
        };


    private static EndpointAddress GetEndpointAddress(string endPoint)
        => new EndpointAddress(new Uri(endPoint));


    public static T SetDefaultProperty<T>(this T request, JP_Login login)
    {
        PropertyInfo[] propertyInfo = request.GetType().GetProperties();

        var loginProperty = propertyInfo.First(x => x.PropertyType == typeof(JP_Login));
        loginProperty.SetValue(request, login);

        var languageProperty = propertyInfo.First(x => x.Name == Constants.LanguageFieldName);
        languageProperty.SetValue(request, Constants.DefaultLanguageCode);

        var versionProperty = propertyInfo.First(x => x.Name == Constants.VersionFieldName);
        versionProperty.SetValue(request, Constants.Version);

        return request;
    }


    private const long MaxReceivedMessageSize = 20000000;
    private const long MaxBufferPoolSize = 20000000;
    private const int MaxBufferSize = 20000000;
}
