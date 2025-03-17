namespace DataFlow.Server.API.Data;

internal interface IRepository<T> where T : Entity
{
  IAsyncEnumerable<Page<T>> GetAsync(
    int pageSize,
    FilterSpecification<T> spec,
    SortSpecification<T> sort
  );
  Task<Page<T>> GetAsync(
    int pageNumber,
    int pageSize,
    FilterSpecification<T> spec,
    SortSpecification<T> sort
  );
  Task<T?> GetAsync(FilterSpecification<T> spec);
  Task<T> CreateAsync(T entity);
  Task<bool> DeleteAsync(FilterSpecification<T> spec);
  Task<bool> DeleteManyAsync(FilterSpecification<T> spec);
  Task<bool> UpdateAsync(FilterSpecification<T> spec, T entity);
  Task<long> CountAsync(FilterSpecification<T> spec);
}