using Digitas.Core.Data.Models;
using Digitas.Core.Data.MongoDb.Abstractions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Digitas.Core.Data.MongoDb;

public class MongoDbRepository : IMongoDbRepository
{
    private readonly IMongoDatabase _database;

    public MongoDbRepository(MongoDbSettings settings)
    {
        // create a new mongo client
        var client = new MongoClient(settings.ConnectionString);

        // get database from mongo client instance
        _database = client.GetDatabase(settings.Database);
    }

    public async Task<IEnumerable<T>> GetAsync<T>()
        where T : MongoModelBase, new()
    {
        // create a instance of T to get the collection name
        var item = new T();

        // get (or create) the collection
        var collection = _database.GetCollection<T>(item.CollectionName);

        // return all collection values
        return await collection.Find(_ => true).ToListAsync();
    }

    public Task<IMongoQueryable<T>> GetAsQueryableAsync<T>()
        where T : MongoModelBase, new()
    {
        // create a instance of T to get the collection name
        var item = new T();

        // get (or create) the collection
        var collection = _database.GetCollection<T>(item.CollectionName);

        // return all collection values
        return Task.FromResult(collection.AsQueryable());
    }

    public async Task CreateAsync<T>(T item, CancellationToken cancellationToken = default)
        where T : MongoModelBase, new()
    {
        // get (or create) the collection
        var collection = _database.GetCollection<T>(item.CollectionName);

        // insert item into the collection
        await collection.InsertOneAsync(item, cancellationToken: cancellationToken);
    }
}
