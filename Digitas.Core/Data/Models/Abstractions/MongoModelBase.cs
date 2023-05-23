using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Digitas.Core.Data.Models;

public abstract record MongoModelBase
{
    public abstract string CollectionName { get; }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }
}
