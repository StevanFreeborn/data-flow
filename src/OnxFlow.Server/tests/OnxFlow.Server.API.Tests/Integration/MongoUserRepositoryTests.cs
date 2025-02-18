namespace OnxFlow.Server.API.Tests.Integration;

public class MongoUserRepositoryTests : IClassFixture<TestDb>
{
  private readonly MongoDbContext _context;
  private readonly MongoUserRepository _sut;

  public MongoUserRepositoryTests(TestDb testDb)
  {
    ArgumentNullException.ThrowIfNull(testDb);

    _context = testDb.Context;
    _sut = new MongoUserRepository(_context);
  }

  [Fact]
  public async Task CreateAsync_WhenCalled_ItShouldCreateUser()
  {
    var (_, newUser) = FakeDataFactory.TestUser.Generate();

    var result = await _sut.CreateAsync(newUser);

    result.Id
      .Should()
      .NotBeNullOrEmpty();

    var createdUser = await _context.GetCollection<User>()
      .Find(u => u.Id == result.Id)
      .SingleOrDefaultAsync();

    createdUser.Should().NotBeNull();
  }
}