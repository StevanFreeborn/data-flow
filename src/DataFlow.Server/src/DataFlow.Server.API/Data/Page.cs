namespace DataFlow.Server.API.Data;

internal class Page<T>(
  int pageNumber,
  int pageSize,
  int totalPages,
  long totalItems,
  IEnumerable<T> items
)
{
  public int PageNumber { get; init; } = pageNumber;
  public int PageSize { get; init; } = pageSize;
  public int TotalPages { get; init; } = totalPages;
  public long TotalItems { get; init; } = totalItems;
  public IEnumerable<T> Items { get; init; } = items;
}