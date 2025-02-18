namespace OnxFlow.Server.API.Data.Mongo;

internal class MongoDbOptions
{
  public string Username { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
  public string Host { get; set; } = string.Empty;
  public int Port { get; set; } = 27017;
  public string DatabaseName { get; set; } = string.Empty;
  public string ConnectionString => $"mongodb://{Username}:{Password}@{Host}:{Port}/{DatabaseName}?authMechanism=SCRAM-SHA-256&authSource=admin&retryWrites=true&w=majority";
}

internal class MongoDbOptionsSetup(IConfiguration configuration) : IConfigureOptions<MongoDbOptions>
{
  private const string SectionName = nameof(MongoDbOptions);
  private readonly IConfiguration _configuration = configuration;

  public void Configure(MongoDbOptions options)
  {
    _configuration
      .GetSection(SectionName)
      .Bind(options);
  }
}