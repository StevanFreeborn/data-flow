namespace OnxFlow.Server.API.Data.Mongo;

internal class MongoDbContext
{
  private const string UserCollectionName = "users";
  private const string TokenCollectionName = "tokens";

  private readonly IMongoDatabase _database;

  public MongoDbContext(IOptions<MongoDbOptions> options)
  {
    MongoClassMap.RegisterMappings();
    using var client = new MongoClient(options.Value.ConnectionString);
    _database = client.GetDatabase(options.Value.DatabaseName);
  }

  public IMongoCollection<T> GetCollection<T>() where T : Entity
  {
    return typeof(T) switch
    {
      Type t when t == typeof(User) => _database.GetCollection<T>(UserCollectionName),
      Type t when t == typeof(BaseToken) => _database.GetCollection<T>(TokenCollectionName),
      _ => throw new ArgumentException($"Collection for type {typeof(T).Name} not found.")
    };
  }
}