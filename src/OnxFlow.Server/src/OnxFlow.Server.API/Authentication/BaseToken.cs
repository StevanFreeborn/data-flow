namespace OnxFlow.Server.API.Authentication;

internal class BaseToken : Entity
{
  public string UserId { get; init; } = string.Empty;
  public string Token { get; init; } = string.Empty;
  public DateTimeOffset ExpiresAt { get; init; } = DateTimeOffset.UtcNow;
  public bool Revoked { get; init; }
  public string TokenType { get; init; } = string.Empty;

  internal BaseToken()
  {
  }

  internal BaseToken(BaseToken token)
  {
    Id = token.Id;
    UserId = token.UserId;
    Token = token.Token;
    ExpiresAt = token.ExpiresAt;
    Revoked = token.Revoked;
    TokenType = token.TokenType;
  }
}