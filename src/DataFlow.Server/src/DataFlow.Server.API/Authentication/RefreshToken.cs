namespace DataFlow.Server.API.Authentication;

internal class RefreshToken : BaseToken
{
  internal RefreshToken() : base()
  {
    TokenType = TokenTypes.Refresh;
  }

  internal RefreshToken(BaseToken token) : base(token)
  {
    TokenType = TokenTypes.Refresh;
  }

  internal RefreshToken(RefreshToken token) : base(token)
  {
  }
}