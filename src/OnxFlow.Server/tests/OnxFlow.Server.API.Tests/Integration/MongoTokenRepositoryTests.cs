namespace OnxFlow.Server.API.Tests.Integration;

public class MongoTokenRepositoryTests : IClassFixture<TestDb>
{
  private readonly MongoDbContext _context;
  private readonly MongoTokenRepository _sut;

  public MongoTokenRepositoryTests(TestDb testDb)
  {
    ArgumentNullException.ThrowIfNull(testDb);

    _context = testDb.Context;
    _sut = new MongoTokenRepository(_context);
  }


  [Fact]
  public async Task CreateAsync_WhenCalled_ItShouldCreateToken()
  {
    var newToken = FakeDataFactory.VerificationToken.Generate();

    var result = await _sut.CreateAsync(newToken);

    result.Id
      .Should()
      .NotBeNullOrEmpty();

    var createdToken = await _context.GetCollection<BaseToken>()
      .Find(t => t.Id == result.Id)
      .SingleOrDefaultAsync();

    createdToken.Should().NotBeNull();
  }
}