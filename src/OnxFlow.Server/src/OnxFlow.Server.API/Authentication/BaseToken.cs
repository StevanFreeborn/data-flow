namespace OnxFlow.Server.API.Authentication;

internal class BaseToken : Entity
{
  public string UserId { get; init; } = string.Empty;
  public string Token { get; init; } = string.Empty;
  public DateTime ExpiresAt { get; init; } = DateTime.UtcNow;
  public bool Revoked { get; init; }
  public string TokenType { get; init; } = string.Empty;
}