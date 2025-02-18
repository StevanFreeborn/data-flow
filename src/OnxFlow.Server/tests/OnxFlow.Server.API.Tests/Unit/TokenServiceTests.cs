namespace OnxFlow.Server.API.Tests.Unit;

public class TokenServiceTests
{
  private readonly Mock<TimeProvider> _timeProviderMock = new();
  private readonly Mock<IRepository<BaseToken>> _tokenRepositoryMock = new();
  private readonly TokenService _sut;

  public TokenServiceTests()
  {
    _sut = new TokenService(
      _timeProviderMock.Object,
      _tokenRepositoryMock.Object
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
}