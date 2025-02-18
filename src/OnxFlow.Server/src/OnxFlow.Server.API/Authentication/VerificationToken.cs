namespace OnxFlow.Server.API.Authentication;

internal class VerificationToken : BaseToken
{
  internal VerificationToken() : base()
  {
    TokenType = "Verification";
  }
}