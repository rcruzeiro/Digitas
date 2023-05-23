using Digitas.MarketData.Bitstamp;
using Newtonsoft.Json.Linq;

namespace Digitas.Core.MarketData.Service.Responses;

public sealed record SubscribeResponse : ResponseBase
{
    public override MessageType Event => MessageType.Subscribe;

    internal static bool TryHandle(JObject response)
    {
        if (response is null) return false;

        var eventName = response["event"];

        if (eventName is not null
         && !string.IsNullOrEmpty(eventName.Value<string>())
         && eventName.Value<string>() == "bts:subscription_succeeded")
        {
            var parsed = response.ToObject<SubscribeResponse>(BitstampExtensions.Serializer);

            if (parsed is not null)
            {
                var channelName = response["channel"];

                if (channelName is not null)
                {
                    parsed.Channel = channelName.Value<string>();

                    var symbol = BitstampExtensions.BrokePair(parsed.Channel?.Split('_').LastOrDefault() ?? string.Empty);

                    parsed.BaseCurrency = symbol.BaseCurrency;
                    parsed.QuoteCurrency = symbol.QuoteCurrency;
                }

                return true;
            }
        }

        return false;
    }
}
