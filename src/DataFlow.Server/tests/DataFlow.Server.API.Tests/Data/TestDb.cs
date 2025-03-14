namespace DataFlow.Server.API.Tests.Data;

public sealed class TestDb : IAsyncLifetime
{
  private const string Image = "mongo:latest";
  private readonly MongoDbContainer _container;
  internal MongoDbContext Context { get; private set; } = null!;

  public TestDb()
  {
    _container = new MongoDbBuilder().WithImage(Image).Build();
  }

  public async Task InitializeAsync()
  {
    await _container.StartAsync();
    Context = new(
      Options.Create(new MongoDbOptions
      {
        ConnectionString = _container.GetConnectionString(),
        DatabaseName = "test",
      })
    );
  }

  async Task IAsyncLifetime.DisposeAsync()
  {
    await _container.DisposeAsync();
  }
}