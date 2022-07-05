using JuniperServiceReference;
using System.Reflection;

namespace HappyTravel.JuniperConnector.Common.Infrastructure;

public static class JuniperServiceExtensions
{
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
}
