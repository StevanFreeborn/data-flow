using FluentResults;

namespace OnxFlow.Server.API.Tests.Unit;

public class UserServiceTests
{
  private readonly Mock<IRepository<User>> _userRepositoryMock = new();
  private readonly Mock<IEncryptionService> _encryptionServiceMock = new();
  private readonly Mock<ILogger<UserService>> _loggerMock = new();
  private readonly Mock<ITokenService> _tokenServiceMock = new();
  private readonly UserService _sut;

  public UserServiceTests()
  {
    _sut = new UserService(
      _userRepositoryMock.Object,
      _encryptionServiceMock.Object,
      _loggerMock.Object,
      _tokenServiceMock.Object
    );
  }

  [Fact]
  public async Task RegisterUserAsync_WhenUserAlreadyExists_ItShouldReturnUserAlreadyExistError()
  {
    _userRepositoryMock
      .Setup(static u => u.GetAsync(It.IsAny<FilterSpecification<User>>()))
      .ReturnsAsync(Mock.Of<User>());

    var result = await _sut.RegisterUserAsync(Mock.Of<User>());

    result.IsFailed.Should().BeTrue();
    result.Errors.Should().Contain(static e => e is UserAlreadyExistError);
  }

  [Fact]
  public async Task RegisterUserAsync_WhenUserDoesNotExist_ItShouldCreateNewUserWithUsernameHashedPasswordAndEncryptionKeyThenReturnUserId()
  {
    var plainTextKey = "test123";
    var encryptedKey = "EncryptedKey";
    var unHashedPassword = "@Password1234";

    var newUser = new User
    {
      Email = "test@test.com",
      Password = unHashedPassword,
    };

    var createdUser = new User
    {
      Id = ObjectId.GenerateNewId().ToString(),
      Email = newUser.Email,
    };

    _userRepositoryMock
      .Setup(u => u.GetAsync(It.IsAny<FilterSpecification<User>>()))
      .ReturnsAsync(null as User);

    _userRepositoryMock
      .Setup(u => u.GetAsync(It.IsAny<FilterSpecification<User>>()))
      .ReturnsAsync(null as User);

    _encryptionServiceMock
      .Setup(e => e.GenerateKey())
      .Returns(plainTextKey);

    _encryptionServiceMock
      .Setup(e => e.EncryptAsync(plainTextKey))
      .ReturnsAsync(encryptedKey);

    _userRepositoryMock
      .Setup(u => u.CreateAsync(It.IsAny<User>()))
      .ReturnsAsync(createdUser);

    var result = await _sut.RegisterUserAsync(newUser);

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeEmpty();
    result.Value.Should().Be(createdUser.Id);

    _userRepositoryMock
      .Verify(
        u => u.CreateAsync(
          It.Is<User>(
            u =>
              string.IsNullOrWhiteSpace(u.Username) == false &&
              u.EncryptionKey == encryptedKey &&
              u.Password != unHashedPassword
          )
        ),
        Times.Once
      );
  }

  [Fact]
  public async Task LoginUserAsync_WhenUserDoesNotExist_ItShouldReturnInvalidLoginError()
  {
    _userRepositoryMock
      .Setup(static u => u.GetAsync(It.IsAny<FilterSpecification<User>>()))
      .ReturnsAsync(null as User);

    var result = await _sut.LoginUserAsync("test@test.com", "@Password1234");

    result.IsFailed.Should().BeTrue();
    result.Errors.Should().Contain(static e => e is InvalidLoginError);
  }

  [Fact]
  public async Task LoginUserAsync_WhenPasswordIsInvalid_ItShouldReturnInvalidLoginError()
  {
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();
    var password = existingUser.Password;
    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(existingUser.Password);
    existingUser.Password = hashedPassword;

    _userRepositoryMock
      .Setup(static u => u.GetAsync(It.IsAny<FilterSpecification<User>>()))
      .ReturnsAsync(existingUser);

    var result = await _sut.LoginUserAsync(existingUser.Email, "not the password");

    result.IsFailed.Should().BeTrue();
    result.Errors.Should().Contain(static e => e is InvalidLoginError);
  }

  [Fact]
  public async Task LoginUserAsync_WhenRefreshTokenFailsToGenerate_ItShouldReturnLoginFailedError()
  {
    var (password, existingUser) = FakeDataFactory.TestUser.Generate();
    existingUser.IsVerified = true;

    _userRepositoryMock
      .Setup(static u => u.GetAsync(It.IsAny<FilterSpecification<User>>()))
      .ReturnsAsync(existingUser);

    _tokenServiceMock
      .Setup(static t => t.GenerateRefreshToken(It.IsAny<string>()))
      .ReturnsAsync(Result.Fail("Failed to generate refresh token."));

    var result = await _sut.LoginUserAsync(existingUser.Email, password);

    result.IsFailed.Should().BeTrue();
    result.Errors.Should().Contain(static e => e is LoginFailedError);
  }

  [Fact]
  public async Task LoginUserAsync_WhenPasswordIsValidAndUserIsVerified_ItShouldReturnAccessTokenAndRefreshToken()
  {
    var (password, existingUser) = FakeDataFactory.TestUser.Generate();
    existingUser.IsVerified = true;

    _userRepositoryMock
      .Setup(static u => u.GetAsync(It.IsAny<FilterSpecification<User>>()))
      .ReturnsAsync(existingUser);

    _tokenServiceMock
      .Setup(static t => t.GenerateAccessToken(It.IsAny<User>()))
      .Returns("AccessToken");

    _tokenServiceMock
      .Setup(static t => t.GenerateRefreshToken(It.IsAny<string>()))
      .ReturnsAsync(Result.Ok(new RefreshToken
      {
        Token = "RefreshToken",
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
      }));

    var result = await _sut.LoginUserAsync(existingUser.Email, password);

    result.IsSuccess.Should().BeTrue();
    result.Value.AccessToken.Should().NotBeEmpty();
    result.Value.RefreshToken.Should().NotBeNull();
    result.Value.RefreshToken.Token.Should().NotBeEmpty();
    result.Value.RefreshToken.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
  }

  [Fact]
  public async Task LoginUserAsync_WhenUserIsNotVerified_ItShouldReturnUserNotVerifiedError()
  {
    var (password, existingUser) = FakeDataFactory.TestUser.Generate();

    _userRepositoryMock
      .Setup(static u => u.GetAsync(It.IsAny<FilterSpecification<User>>()))
      .ReturnsAsync(existingUser);

    var result = await _sut.LoginUserAsync(existingUser.Email, password);

    result.IsFailed.Should().BeTrue();
    result.Errors.Should().Contain(static e => e is UserNotVerifiedError);
  }
}