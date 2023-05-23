using System.Text;

namespace Digitas.Api.Web.Services;

public static class SseService
{
    public static void ConfigureHeaders(IHeaderDictionary headers)
    {
        headers.TryAdd("Content-Type", "text/event-stream;charset=UTF-8");
        headers.TryAdd("Cache-control", "no-cache, no-store");
        headers.TryAdd("Connection", "keep-alive");
    }

    public static async Task WriteMessageAsync(HttpResponse response, string data, CancellationToken cancellationToken = default)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            if (response is null || !response.Body.CanWrite) return;

            var message = $"data: {data}\n\n";
            var messageBytes = Encoding.UTF8.GetBytes(message);

            await response.BodyWriter.WriteAsync(messageBytes, cancellationToken);
            await response.Body.FlushAsync(cancellationToken);
        }
    }

    public static async Task WriteMessageAsync(ILogger logger, HttpResponse response, string data, CancellationToken cancellationToken = default)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            if (response is null || !response.Body.CanWrite)
            {
                logger.LogWarning("Cannot write data into body.");
                return;
            }

            var message = $"data: {data}\n\n";
            var messageBytes = Encoding.UTF8.GetBytes(message);

            logger.LogInformation("Writing data into body: {data}", data);

            await response.BodyWriter.WriteAsync(messageBytes, cancellationToken);
            await response.Body.FlushAsync(cancellationToken);
        }
    }
}
