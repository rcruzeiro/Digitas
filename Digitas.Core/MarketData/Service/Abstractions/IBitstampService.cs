using Digitas.Core.MarketData.Service.Requests;

namespace Digitas.Core.MarketData.Service;

public interface IBitstampService : IDisposable
{
    void Send<T>(T request) where T : RequestBase;
}
