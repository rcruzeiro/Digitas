using Digitas.Core.Data.MongoDb;

namespace Digitas.Api.Web;

public sealed class ApiSettings
{
    public MongoDbSettings Mongo { get; set; } = new();
}
