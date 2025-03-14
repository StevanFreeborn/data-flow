namespace DataFlow.Server.API.Tests.Integration;

public class TokenServiceTests : IClassFixture<TestDb>
{
  private readonly Mock<TimeProvider> _timeProviderMock = new();
  private readonly Mock<IOptions<JwtOptions>> _jwtOptionsMock = new();
  private readonly MongoTokenRepository _tokenRepository;
  private readonly Mock<ILogger<TokenService>> _loggerMock = new();
  private readonly TokenService _tokenService;

  public TokenServiceTests(TestDb testDb)
  {
    ArgumentNullException.ThrowIfNull(testDb);

    var jwtOptions = FakeDataFactory.JwtOption.Generate();

    _jwtOptionsMock
      .Setup(static j => j.Value)
      .Returns(jwtOptions);

    _tokenRepository = new MongoTokenRepository(testDb.Context);
    _tokenService = new TokenService(
      _timeProviderMock.Object,
      _tokenRepository,
      _jwtOptionsMock.Object,
      _loggerMock.Object
    );
  }

  [Fact]
  public async Task RemoveAllInvalidVerificationTokensAsync_WhenCalled_ItShouldDeleteCorrectTokens()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var refreshToken = FakeDataFactory.RefreshToken.Generate();
    var verificationToken = FakeDataFactory.VerificationToken.Generate();
    var verificationTokenForAnotherUser = FakeDataFactory.VerificationToken.Generate();

    var revokedVerificationToken = new VerificationToken(FakeDataFactory.VerificationToken.Generate())
    {
      UserId = user.Id,
      Revoked = true
    };

    var expiredVerificationToken = new VerificationToken(FakeDataFactory.VerificationToken.Generate())
    {
      UserId = user.Id,
      ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1)
    };

    var tokens = new BaseToken[]
    {
      refreshToken,
      verificationToken,
      revokedVerificationToken,
      expiredVerificationToken
    };

    foreach (var token in tokens)
    {
      await _tokenRepository.CreateAsync(token);
    }

    await _tokenService.RemoveAllInvalidVerificationTokensAsync(user.Id);

    var countOfTokens = await _tokenRepository.CountAsync(FilterSpecification<BaseToken>.All);
    var insertedRefreshToken = await _tokenRepository.GetAsync(FilterSpecification<BaseToken>.From(t => t.Id == refreshToken.Id));
    var insertedVerificationToken = await _tokenRepository.GetAsync(FilterSpecification<BaseToken>.From(t => t.Id == verificationToken.Id));
    var insertedVerificationTokenForAnotherUser = await _tokenRepository.GetAsync(FilterSpecification<BaseToken>.From(t => t.Id == verificationTokenForAnotherUser.Id));

    refreshToken.Should().NotBeNull();
    verificationToken.Should().NotBeNull();
    revokedVerificationToken.Should().NotBeNull();
    countOfTokens.Should().Be(3);

    await _tokenRepository.DeleteManyAsync(FilterSpecification<BaseToken>.All);
  }

  [Fact]
  public async Task RemoveAllInvalidRefreshTokensAsync_WhenCalled_ItShouldDeleteCorrectTokens()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();
    var refreshToken = FakeDataFactory.RefreshToken.Generate();
    var verificationToken = FakeDataFactory.VerificationToken.Generate();
    var verificationTokenForAnotherUser = FakeDataFactory.VerificationToken.Generate();

    var revokedRefreshToken = new RefreshToken(FakeDataFactory.RefreshToken.Generate())
    {
      UserId = user.Id,
      Revoked = true
    };

    var expiredRefreshToken = new RefreshToken(FakeDataFactory.RefreshToken.Generate())
    {
      UserId = user.Id,
      ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1)
    };

    var tokens = new BaseToken[]
    {
      refreshToken,
      verificationToken,
      revokedRefreshToken,
      expiredRefreshToken
    };

    foreach (var token in tokens)
    {
      await _tokenRepository.CreateAsync(token);
    }

    await _tokenService.RemoveAllInvalidRefreshTokensAsync(user.Id);

    var countOfTokens = await _tokenRepository.CountAsync(FilterSpecification<BaseToken>.All);
    var insertedRefreshToken = await _tokenRepository.GetAsync(FilterSpecification<BaseToken>.From(t => t.Id == refreshToken.Id));
    var insertedVerificationToken = await _tokenRepository.GetAsync(FilterSpecification<BaseToken>.From(t => t.Id == verificationToken.Id));
    var insertedVerificationTokenForAnotherUser = await _tokenRepository.GetAsync(FilterSpecification<BaseToken>.From(t => t.Id == verificationTokenForAnotherUser.Id));

    refreshToken.Should().NotBeNull();
    verificationToken.Should().NotBeNull();
    revokedRefreshToken.Should().NotBeNull();
    countOfTokens.Should().Be(3);

    await _tokenRepository.DeleteManyAsync(FilterSpecification<BaseToken>.All);
  }
}