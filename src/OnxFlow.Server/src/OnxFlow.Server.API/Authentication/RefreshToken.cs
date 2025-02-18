namespace OnxFlow.Server.API.Authentication;

internal class RefreshToken : BaseToken
{
  internal RefreshToken() : base()
  {
    TokenType = "Refresh";
  }
}