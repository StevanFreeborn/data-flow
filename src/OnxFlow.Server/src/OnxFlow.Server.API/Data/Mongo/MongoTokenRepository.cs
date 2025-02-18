namespace OnxFlow.Server.API.Data.Mongo;

internal class MongoTokenRepository(MongoDbContext context) : MongoRepository<BaseToken>(context)
{
}