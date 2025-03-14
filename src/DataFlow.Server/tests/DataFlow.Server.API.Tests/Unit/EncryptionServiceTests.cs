namespace DataFlow.Server.API.Tests.Unit;

public class EncryptionServiceTests
{
  private readonly EncryptionService _encryptionService;
  private readonly IOptions<EncryptionOptions> _encryptionOptions;

  public EncryptionServiceTests()
  {
    _encryptionOptions = Options.Create(new EncryptionOptions
    {
      Key = "1234567890123456",
    });

    _encryptionService = new EncryptionService(_encryptionOptions);
  }

  [Fact]
  public async Task Encrypt_WhenCalledWithPlainText_ReturnsEncryptedData()
  {
    var data = "Hello, World!";

    var encryptedData = await _encryptionService.EncryptAsync(data);

    encryptedData.Should().NotBeNullOrEmpty();
    encryptedData.Should().NotBe(data);
  }

  [Fact]
  public async Task Decrypt_WhenCalledWithEncryptedData_ReturnsDecryptedData()
  {
    var data = "Hello, World!";
    var encryptedData = await _encryptionService.EncryptAsync(data);

    var decryptedData = await _encryptionService.DecryptAsync(encryptedData);

    data.Should().Be(decryptedData);
  }

  [Fact]
  public async Task EncryptForUser_WhenCalledWithPlainTextAndUser_ReturnsEncryptedData()
  {
    var userEncryptionKey = Encoding.UTF8.GetString(new byte[16]);
    var encryptedUserKey = await _encryptionService.EncryptAsync(userEncryptionKey);
    var user = new User { EncryptionKey = encryptedUserKey };
    var data = "Hello, World!";
    var encryptedData = await _encryptionService.EncryptForUserAsync(data, user);

    encryptedData.Should().NotBeNullOrEmpty();
    encryptedData.Should().NotBe(data);
  }

  [Fact]
  public async Task DecryptForUser_WhenCalledWithEncryptedDataAndUser_ReturnsDecryptedData()
  {
    var userEncryptionKey = Encoding.UTF8.GetString(new byte[16]);
    var encryptedUserKey = await _encryptionService.EncryptAsync(userEncryptionKey);
    var user = new User { EncryptionKey = encryptedUserKey };
    var data = "Hello, World!";
    var encryptedDataForUser = await _encryptionService.EncryptForUserAsync(data, user);

    var decryptedData = await _encryptionService.DecryptForUserAsync(encryptedDataForUser, user);

    decryptedData.Should().Be(data);
  }

  [Fact]
  public void GenerateKey_WhenCalled_ReturnsRandomKey()
  {
    var key1 = _encryptionService.GenerateKey();
    var key2 = _encryptionService.GenerateKey();

    key1.Should().NotBeNullOrEmpty();
    key2.Should().NotBeNullOrEmpty();
    key1.Should().NotBe(key2);
  }
}