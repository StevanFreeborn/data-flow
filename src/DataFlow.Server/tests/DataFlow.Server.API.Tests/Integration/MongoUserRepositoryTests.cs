namespace DataFlow.Server.API.Tests.Integration;

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

  [Fact]
  public async Task GetAsync_WhenCalled_ItShouldReturnUser()
  {
    var (_, newUser) = FakeDataFactory.TestUser.Generate();

    await _context.GetCollection<User>()
      .InsertOneAsync(newUser);

    var result = await _sut.GetAsync(FilterSpecification<User>.From(u => u.Id == newUser.Id));

    result.Should().NotBeNull();
    result!.Id.Should().Be(newUser.Id);
  }

  [Theory]
  [InlineData(10, 1, 10)]
  [InlineData(10, 5, 2)]
  [InlineData(10, 10, 1)]
  public async Task GetAsync_WhenCalledWithPageSize_ItShouldReturnAllPagesOfUsers(
    int numberOfUsers,
    int pageSize,
    int expectedPages
  )
  {
    var (_, users) = FakeDataFactory.TestUser.Generate(numberOfUsers);

    await _context.GetCollection<User>().InsertManyAsync(users);

    var retrievedPages = new List<Page<User>>();
    var filter = FilterSpecification<User>.All;
    var sort = SortSpecification<User>.SortBy(static u => u.CreatedDate);

    await foreach (var page in _sut.GetAsync(pageSize, filter, sort))
    {
      retrievedPages.Add(page);
    }

    retrievedPages.Count.Should().Be(expectedPages);
    retrievedPages.SelectMany(static p => p.Items).Count().Should().Be(numberOfUsers);

    await _context.GetCollection<User>().DeleteManyAsync(FilterDefinition<User>.Empty);
  }

  [Fact]
  public async Task GetAsync_WhenCalledWithPageSizeAscendingSort_ItShouldReturnProperOrder()
  {
    var (_, users) = FakeDataFactory.TestUser.Generate(2);
    var firstUser = users.First();
    var secondUser = users.Last();

    firstUser.Username = "A";
    secondUser.Username = "Z";

    await _context.GetCollection<User>().InsertManyAsync(users);

    var retrievedPages = new List<Page<User>>();
    var filter = FilterSpecification<User>.All;
    var sort = SortSpecification<User>.SortBy(static u => u.Username);

    await foreach (var page in _sut.GetAsync(1, filter, sort))
    {
      retrievedPages.Add(page);
    }

    retrievedPages.Count.Should().Be(2);
    retrievedPages.First().Items.First().Id.Should().Be(firstUser.Id);
    retrievedPages.Last().Items.First().Id.Should().Be(secondUser.Id);
  }

  [Fact]
  public async Task GetAsync_WhenCalledWithPageSizeDescendingSort_ItShouldReturnProperOrder()
  {
    var (_, users) = FakeDataFactory.TestUser.Generate(2);
    var firstUser = users.First();
    var secondUser = users.Last();

    firstUser.Update(DateTimeOffset.UtcNow);
    secondUser.Update(DateTimeOffset.UtcNow.AddDays(1));

    await _context.GetCollection<User>().InsertManyAsync(users);

    var retrievedPages = new List<Page<User>>();
    var filter = FilterSpecification<User>.All;
    var sort = SortSpecification<User>.SortByDesc(static u => u.UpdatedDate);

    await foreach (var page in _sut.GetAsync(1, filter, sort))
    {
      retrievedPages.Add(page);
    }

    retrievedPages.Count.Should().Be(2);
    retrievedPages.First().Items.First().Id.Should().Be(secondUser.Id);
    retrievedPages.Last().Items.First().Id.Should().Be(firstUser.Id);
  }
}