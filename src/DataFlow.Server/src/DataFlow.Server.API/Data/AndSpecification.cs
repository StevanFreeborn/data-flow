namespace DataFlow.Server.API.Data;

internal sealed class AndSpecification<T>(FilterSpecification<T> left, FilterSpecification<T> right) : FilterSpecification<T> where T : Entity
{
  private readonly FilterSpecification<T> _left = left;
  private readonly FilterSpecification<T> _right = right;

  public override Expression<Func<T, bool>> ToExpression()
  {
    var leftExpression = _left.ToExpression();
    var rightExpression = _right.ToExpression();

    var paramExpression = Expression.Parameter(typeof(T));
    var expressionBody = Expression.AndAlso(leftExpression.Body, rightExpression.Body);
    var andExpression = (BinaryExpression)new ParameterReplacer(paramExpression).Visit(expressionBody);

    return Expression.Lambda<Func<T, bool>>(andExpression, paramExpression);
  }
}