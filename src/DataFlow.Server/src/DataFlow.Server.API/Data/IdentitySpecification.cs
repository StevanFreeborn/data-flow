namespace DataFlow.Server.API.Data;

internal sealed class IdentitySpecification<T> : FilterSpecification<T> where T : Entity
{
  public override Expression<Func<T, bool>> ToExpression()
  {
    return static x => true;
  }
}