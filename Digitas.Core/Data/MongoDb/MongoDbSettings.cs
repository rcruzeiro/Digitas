namespace Digitas.Core.Data.MongoDb;

public sealed record MongoDbSettings
{
    public string? ConnectionString { get; set; }

    public string? Database { get; set; }
}
