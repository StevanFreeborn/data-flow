namespace OnxFlow.Server.API.Data;

internal abstract class FilterSpecification<T> where T : Entity
{
  public static FilterSpecification<T> All => new IdentitySpecification<T>();
  public abstract Expression<Func<T, bool>> ToExpression();

  public bool IsSatisfiedBy(T entity)
  {
    var predicate = ToExpression().Compile();
    return predicate(entity);
  }

  public FilterSpecification<T> And(FilterSpecification<T> spec)
  {
    if (this == All)
    {
      return spec;
    }

    if (spec == All)
    {
      return this;
    }

    return new AndSpecification<T>(this, spec);
  }

  public static FilterSpecification<T> From(Expression<Func<T, bool>> expression)
  {
    return new ExpressionSpecification(expression);
  }

  private class ExpressionSpecification(Expression<Func<T, bool>> expression) : FilterSpecification<T>
  {
    private readonly Expression<Func<T, bool>> _expression = expression ?? throw new ArgumentNullException(nameof(expression));

    public override Expression<Func<T, bool>> ToExpression()
    {
      return _expression;
    }
  }
}