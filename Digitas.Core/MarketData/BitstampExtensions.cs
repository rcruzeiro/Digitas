using Digitas.Core.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Digitas.MarketData.Bitstamp;

public static class BitstampExtensions
{
    public static string PairWith(this Currency baseCurrency, Currency quoteCurrency)
    {
        return $"{baseCurrency}{quoteCurrency}".ToLower();
    }

    public static (Currency? BaseCurrency, Currency? QuoteCurrency) BrokePair(string pair)
    {
        if (string.IsNullOrEmpty(pair)) throw new ArgumentNullException(nameof(pair));

        var baseCurrency = Enum.Parse<Currency>(pair[0..3], true);
        var quoteCurrency = Enum.Parse<Currency>(pair[3..], true);

        return (baseCurrency, quoteCurrency);
    }

    private static readonly JsonSerializerSettings Settings = new()
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        Formatting = Formatting.None,
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        }
    };

    public static readonly JsonSerializer Serializer = JsonSerializer.Create(Settings);

    public static string Serialize(object data)
    {
        return JsonConvert.SerializeObject(data, Settings);
    }

    public static T? Deserialize<T>(string data)
    {
        return JsonConvert.DeserializeObject<T>(data, Settings);
    }
}
