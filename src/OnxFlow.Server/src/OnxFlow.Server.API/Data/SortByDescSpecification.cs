namespace OnxFlow.Server.API.Data;

internal class SortByDescSpecification<T>(
  Expression<Func<T, object>> expression
) : SortSpecification<T> where T : Entity
{
  private readonly Expression<Func<T, object>> _expression = expression ?? throw new ArgumentNullException(nameof(expression));

  public override Expression<Func<T, object>> ToExpression()
  {
    return _expression;
  }
}