namespace OnxFlow.Server.API.Authentication;

internal class VerificationToken : BaseToken
{
  internal VerificationToken() : base()
  {
    TokenType = TokenTypes.Verification;
  }

  internal VerificationToken(BaseToken token) : base(token)
  {
    TokenType = TokenTypes.Verification;
  }

  internal VerificationToken(VerificationToken token) : base(token)
  {
  }
}