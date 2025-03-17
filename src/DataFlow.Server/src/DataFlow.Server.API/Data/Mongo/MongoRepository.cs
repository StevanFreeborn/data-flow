using System.Threading.Tasks;

namespace DataFlow.Server.API.Data.Mongo;

internal abstract class MongoRepository<T>(MongoDbContext context) : IRepository<T> where T : Entity
{
  private const string CountFacetName = "count";
  private const string ItemsFacetName = "items";
  private protected readonly IMongoCollection<T> _collection = context.GetCollection<T>();

  public virtual async IAsyncEnumerable<Page<T>> GetAsync(int pageSize, FilterSpecification<T> filter, SortSpecification<T> sort)
  {
    var initialPageNumber = 1;
    int totalPages;

    do
    {
      var page = await GetAsync(initialPageNumber, pageSize, filter, sort);
      yield return page;
      initialPageNumber++;
      totalPages = page.TotalPages;
    } while (totalPages >= initialPageNumber);
  }

  public virtual async Task<Page<T>> GetAsync(int pageNumber, int pageSize, FilterSpecification<T> filter, SortSpecification<T> sort)
  {
    var match = PipelineStageDefinitionBuilder.Match(filter.ToExpression());

    var totalFacet = AggregateFacet.Create(
      CountFacetName,
      PipelineDefinition<T, AggregateCountResult>.Create([match, PipelineStageDefinitionBuilder.Count<T>()])
    );

    var itemsFacet = AggregateFacet.Create(
      ItemsFacetName,
      PipelineDefinition<T, T>.Create(
        [
          match,
          PipelineStageDefinitionBuilder.Skip<T>((pageNumber - 1) * pageSize),
          PipelineStageDefinitionBuilder.Limit<T>(pageSize)
        ]
      )
    );

    var aggregation = _collection.Aggregate();

    var sortedAggregation = sort switch
    {
      SortBySpecification<T> => aggregation.SortBy(sort.ToExpression()),
      SortByDescSpecification<T> => aggregation.SortByDescending(sort.ToExpression()),
      _ => throw new ArgumentException($"{nameof(sort)} is invalid type")
    };

    var finalAggregation = await sortedAggregation
      .Facet(totalFacet, itemsFacet)
      .FirstOrDefaultAsync();

    var countOutput = finalAggregation.Facets.First(static f => f.Name == CountFacetName).Output<AggregateCountResult>();
    var totalItems = countOutput.Count is 0 ? 0 : countOutput[0].Count;
    var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
    var items = finalAggregation.Facets.First(static f => f.Name == ItemsFacetName).Output<T>();

    return new(pageNumber, pageSize, totalPages, totalItems, items);
  }

  public virtual async Task<T?> GetAsync(FilterSpecification<T> spec)
  {
    return await _collection.Find(spec.ToExpression()).FirstOrDefaultAsync();
  }

  public virtual async Task<T> CreateAsync(T entity)
  {
    await _collection.InsertOneAsync(entity);
    return entity;
  }

  public virtual async Task<bool> DeleteAsync(FilterSpecification<T> spec)
  {
    var result = await _collection.DeleteOneAsync(spec.ToExpression());
    return result.DeletedCount > 0;
  }

  public virtual async Task<bool> UpdateAsync(FilterSpecification<T> spec, T entity)
  {
    entity.Update();
    var result = await _collection.ReplaceOneAsync(spec.ToExpression(), entity);
    return result.ModifiedCount > 0;
  }

  public virtual async Task<long> CountAsync(FilterSpecification<T> spec)
  {
    return await _collection.CountDocumentsAsync(spec.ToExpression());
  }

  public virtual async Task<bool> DeleteManyAsync(FilterSpecification<T> spec)
  {
    var result = await _collection.DeleteManyAsync(spec.ToExpression());
    return result.DeletedCount > 0;
  }
}