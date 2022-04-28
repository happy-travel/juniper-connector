using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace HappyTravel.JuniperConnector.Common;

public class JuniperSerializer
{
    public JuniperSerializer()
    {
        _serializer = new JsonSerializer
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };
    }


    public string Serialize<TData>(TData data)
    {
        using var stringWriter = new StringWriter();

        _serializer.Serialize(stringWriter, data);

        return stringWriter.ToString();
    }


    public TData Deserialize<TData>(JsonTextReader textReader)
    {
        return _serializer.Deserialize<TData>(textReader);
    }


    public TData Deserialize<TData>(string data)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        using var streamReader = new StreamReader(stream);
        using var jsonTextReader = new JsonTextReader(streamReader);

        return _serializer.Deserialize<TData>(jsonTextReader);
    }


    private readonly JsonSerializer _serializer;
}
