namespace OnxFlow.Server.API.Encryption;

internal class EncryptionService(IOptions<EncryptionOptions> options) : IEncryptionService
{
  private readonly IOptions<EncryptionOptions> _options = options;

  private static async Task<string> EncryptCore(string plainText, string key)
  {
    using var aes = Aes.Create();
    aes.Key = Encoding.UTF8.GetBytes(key);

    aes.GenerateIV();

    using var ms = new MemoryStream();
    await ms.WriteAsync(aes.IV.AsMemory(0, aes.IV.Length));


    using var encryptor = aes.CreateEncryptor();
    using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);

    var bytes = Encoding.UTF8.GetBytes(plainText);
    await cs.WriteAsync(bytes);
    await cs.FlushFinalBlockAsync();

    return Convert.ToBase64String(ms.ToArray());
  }

  private static async Task<string> DecryptCore(string cipherText, string key)
  {
    var bytes = Convert.FromBase64String(cipherText);

    using var aes = Aes.Create();
    aes.Key = Encoding.UTF8.GetBytes(key);

    var iv = new byte[16];
    Array.Copy(bytes, iv, iv.Length);
    aes.IV = iv;

    using var decryptor = aes.CreateDecryptor();
    using var ms = new MemoryStream(bytes, iv.Length, bytes.Length - iv.Length);
    using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
    using var sr = new StreamReader(cs);

    return await sr.ReadToEndAsync();
  }

  public async Task<string> EncryptAsync(string plainText)
  {
    return await EncryptCore(plainText, _options.Value.Key);
  }

  public async Task<string> DecryptAsync(string cipherText)
  {
    return await DecryptCore(cipherText, _options.Value.Key);
  }

  public async Task<string> EncryptForUserAsync(string plainText, User user)
  {
    var decryptedUserKey = await DecryptCore(user.EncryptionKey, _options.Value.Key);
    return await EncryptCore(plainText, decryptedUserKey);
  }

  public async Task<string> DecryptForUserAsync(string cipherText, User user)
  {
    var decryptedUserKey = await DecryptCore(user.EncryptionKey, _options.Value.Key);
    return await DecryptCore(cipherText, decryptedUserKey);
  }

  public string GenerateKey()
  {
    var key = new byte[16];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(key);

    return Convert.ToBase64String(key);
  }
}