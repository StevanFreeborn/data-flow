namespace OnxFlow.Server.API.Tests.Unit;

public class TokenServiceTests
{
  private readonly Mock<TimeProvider> _timeProviderMock = new();
  private readonly Mock<IRepository<BaseToken>> _tokenRepositoryMock = new();
  private readonly TokenService _sut;
  private readonly Mock<IOptions<JwtOptions>> _jwtOptionsMock = new();

  public TokenServiceTests()
  {
    var jwtOptions = FakeDataFactory.JwtOption.Generate();

    _jwtOptionsMock
      .Setup(static j => j.Value)
      .Returns(jwtOptions);

    _sut = new TokenService(
      _timeProviderMock.Object,
      _tokenRepositoryMock.Object,
      _jwtOptionsMock.Object
    );
  }

  [Fact]
  public async Task GenerateVerificationToken_WhenCalled_ItShouldReturnVerificationToken()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    var now = DateTimeOffset.UtcNow;

    _timeProviderMock
      .Setup(static t => t.GetUtcNow())
      .Returns(now);

    var result = await _sut.GenerateVerificationToken(user.Id);

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Should().BeOfType<VerificationToken>();
    result.Value.UserId.Should().Be(user.Id);
    result.Value.Token.Should().NotBeNullOrEmpty();
    result.Value.ExpiresAt.Should().Be(now.AddMinutes(15).UtcDateTime);
  }

  [Fact]
  public async Task GenerateVerificationToken_WhenCalledAndTokenRepositoryThrowsException_ShouldReturnError()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    _timeProviderMock
      .Setup(static t => t.GetUtcNow())
      .Returns(DateTimeOffset.UtcNow);

    _tokenRepositoryMock
      .Setup(static t => t.CreateAsync(It.IsAny<VerificationToken>()))
      .ThrowsAsync(new Exception());

    var result = await _sut.GenerateVerificationToken(user.Id);

    result.IsFailed.Should().BeTrue();
    result.Errors.Should().ContainSingle();
    result.Errors.First().Should().BeOfType<GenerateVerificationTokenError>();
  }

  [Fact]
  public void GenerateAccessToken_WhenCalled_ShouldReturnAccessToken()
  {
    _timeProviderMock
      .Setup(static t => t.GetUtcNow())
      .Returns(DateTimeOffset.UtcNow);

    var (_, user) = FakeDataFactory.TestUser.Generate();

    var result = _sut.GenerateAccessToken(user);

    result.Should().NotBeNullOrEmpty();

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.ReadJwtToken(result);

    token.Issuer.Should().Be(_jwtOptionsMock.Object.Value.Issuer);
    token.Audiences.Should().Contain(_jwtOptionsMock.Object.Value.Audience);

    token.ValidTo
      .Should()
      .BeCloseTo(
        token.ValidFrom.AddMinutes(_jwtOptionsMock.Object.Value.ExpiryInMinutes),
        TimeSpan.FromSeconds(5)
      );

    token.Claims.Should().Contain(static c => c.Type == JwtRegisteredClaimNames.Jti);
    token.Claims.Should().Contain(static c => c.Type == JwtRegisteredClaimNames.Sub);
    token.Claims.Should().Contain(static c => c.Type == JwtRegisteredClaimNames.NameId);
    token.Claims.Should().Contain(static c => c.Type == JwtRegisteredClaimNames.Email);

    var subClaim = token.Claims.First(static c => c.Type == JwtRegisteredClaimNames.Sub);
    subClaim.Value.Should().Be(user.Id);

    var nameIdClaim = token.Claims.First(static c => c.Type == JwtRegisteredClaimNames.NameId);
    nameIdClaim.Value.Should().Be(user.Username);

    var emailClaim = token.Claims.First(static c => c.Type == JwtRegisteredClaimNames.Email);
    emailClaim.Value.Should().Be(user.Email);
  }

  [Fact]
  public async Task GenerateRefreshToken_WhenCalled_ShouldReturnRefreshToken()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    var now = DateTimeOffset.UtcNow;

    _timeProviderMock
      .Setup(static t => t.GetUtcNow())
      .Returns(now);

    var result = await _sut.GenerateRefreshToken(user.Id);

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Should().BeOfType<RefreshToken>();
    result.Value.UserId.Should().Be(user.Id);
    result.Value.Token.Should().NotBeNullOrEmpty();
    result.Value.ExpiresAt.Should().Be(now.AddHours(12).UtcDateTime);
  }

  [Fact]
  public async Task GenerateRefreshToken_WhenCalledAndTokenRepositoryThrowsException_ShouldReturnError()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    _timeProviderMock
      .Setup(static t => t.GetUtcNow())
      .Returns(DateTimeOffset.UtcNow);

    _tokenRepositoryMock
      .Setup(static t => t.CreateAsync(It.IsAny<RefreshToken>()))
      .ThrowsAsync(new Exception());

    var result = await _sut.GenerateRefreshToken(user.Id);

    result.IsFailed.Should().BeTrue();
    result.Errors.Should().ContainSingle();
    result.Errors.First().Should().BeOfType<GenerateRefreshTokenError>();
  }
}