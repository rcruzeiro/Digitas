using Digitas.Core.MarketData.Service.Requests;
using Digitas.Core.MarketData.Service.Responses;
using Digitas.MarketData.Bitstamp;
using Newtonsoft.Json.Linq;
using Websocket.Client;

namespace Digitas.Core.MarketData.Service;

public sealed class BitstampService : IBitstampService
{
    private readonly IBitstampWebsocketClient _socketClient;
    private readonly IDisposable _messageFromSubscription;

    public BitstampService(IBitstampWebsocketClient socketClient)
    {
        _socketClient = socketClient;
        _messageFromSubscription = _socketClient.MessageReceived.Subscribe(HandleMessage);
    }

    public void Send<T>(T request)
        where T : RequestBase
    {
        try
        {
            var serialized = BitstampExtensions.Serialize(new
            {
                request.Event,
                request.Data
            });

            _socketClient.Send(serialized);
        }
        catch (Exception ex)
        {
            //TODO: error treatment
            throw;
        }
    }

    private void HandleMessage(ResponseMessage response)
    {
        try
        {
            var message = (response.Text ?? string.Empty).Trim();

            if (message.StartsWith("{"))
            {
                HandleMessageObject(message);
            }
        }
        catch (Exception ex)
        {
            //TODO: error treatment
            throw;
        }
    }

    private static bool HandleMessageObject(string message)
    {
        var response = BitstampExtensions.Deserialize<JObject>(message);

        if (response is null) return false;

        return SubscribeResponse.TryHandle(response)
            || UnsubscribeResponse.TryHandle(response)
            || OrderBookResponse.TryHandle(response)
            || false;
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _messageFromSubscription?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
}
