namespace DataFlow.Server.API.Encryption;

internal interface IEncryptionService
{
  Task<string> EncryptAsync(string plainText);
  Task<string> EncryptForUserAsync(string plainText, User user);
  Task<string> DecryptAsync(string cipherText);
  Task<string> DecryptForUserAsync(string cipherText, User user);
  string GenerateKey();
}