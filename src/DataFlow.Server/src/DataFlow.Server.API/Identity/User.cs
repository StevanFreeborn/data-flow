namespace DataFlow.Server.API.Identity;

internal class User : Entity
{
  public string Email { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
  public string Username { get; set; } = string.Empty;
  public bool IsVerified { get; set; }
  public string EncryptionKey { get; set; } = string.Empty;
  public bool HasMFAEnabled { get; set; }
  public string[] RecoveryCodes { get; set; } = [];
}