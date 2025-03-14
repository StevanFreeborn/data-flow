namespace DataFlow.Server.API.Tests.Integration;

public class IntegrationTest : IClassFixture<AppFactory>
{
  protected AppFactory Factory { get; }
  protected HttpClient Client { get; }
  internal MongoDbContext Context { get; }
  internal MailHogService MailHogService { get; }

  public IntegrationTest(AppFactory factory)
  {
    ArgumentNullException.ThrowIfNull(factory);

    Factory = factory;
    Client = factory.CreateClient();
    Context = factory.Services.GetRequiredService<MongoDbContext>();
    MailHogService = factory.Services.GetRequiredService<MailHogService>();
  }
}