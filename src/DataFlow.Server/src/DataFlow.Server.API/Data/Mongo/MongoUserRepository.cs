namespace DataFlow.Server.API.Data.Mongo;

internal class MongoUserRepository(MongoDbContext context) : MongoRepository<User>(context)
{
}