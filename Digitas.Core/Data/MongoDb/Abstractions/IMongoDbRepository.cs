using Digitas.Core.Data.Models;
using MongoDB.Driver.Linq;

namespace Digitas.Core.Data.MongoDb.Abstractions;

public interface IMongoDbRepository : IRepository
{
    Task<IEnumerable<T>> GetAsync<T>() where T : MongoModelBase, new();

    Task<IMongoQueryable<T>> GetAsQueryableAsync<T>() where T : MongoModelBase, new();

    Task CreateAsync<T>(T item, CancellationToken cancellationToken = default) where T : MongoModelBase, new();
}
