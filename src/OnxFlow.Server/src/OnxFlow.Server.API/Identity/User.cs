namespace OnxFlow.Server.API.Identity;

internal class User
{
  public string Id { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
  public string Username { get; set; } = string.Empty;
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
  public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
  public bool IsVerified { get; set; }
  public string EncryptionKey { get; set; } = string.Empty;
  public bool HasMFAEnabled { get; set; }
  public string[] RecoveryCodes { get; set; } = [];
}