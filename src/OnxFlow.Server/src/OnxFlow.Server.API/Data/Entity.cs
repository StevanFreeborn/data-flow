namespace OnxFlow.Server.API.Data;

internal abstract class Entity
{
  public string Id { get; init; } = null!;
  public DateTimeOffset CreatedDate { get; init; } = DateTimeOffset.UtcNow;
  public DateTimeOffset UpdatedDate { get; private set; } = DateTimeOffset.UtcNow;

  public void Update()
  {
    UpdatedDate = DateTimeOffset.UtcNow;
  }

  public static Dictionary<string, Expression<Func<T, object>>> SortMap<T>()
  {
    var map = typeof(T).GetProperties()
      .Where(static prop => prop.CanRead)
      .ToDictionary(
        static prop => prop.Name.ToUpperInvariant(),
        static prop =>
        {
          var parameter = Expression.Parameter(typeof(T), "x");
          var propertyAccess = Expression.Property(parameter, prop);
          var convertToObject = Expression.Convert(propertyAccess, typeof(object));

          return Expression.Lambda<Func<T, object>>(convertToObject, parameter);
        }
      );

    return map;
  }
}