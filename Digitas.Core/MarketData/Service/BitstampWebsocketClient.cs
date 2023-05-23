using System.Net.WebSockets;
using Websocket.Client;

namespace Digitas.Core.MarketData.Service;

public sealed class BitstampWebsocketClient : WebsocketClient, IBitstampWebsocketClient
{
    public BitstampWebsocketClient(Uri url, Func<ClientWebSocket>? clientFactory = null)
        : base(url, clientFactory)
    { }
}
