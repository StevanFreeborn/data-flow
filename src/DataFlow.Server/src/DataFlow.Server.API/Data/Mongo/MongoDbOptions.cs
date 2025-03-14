namespace DataFlow.Server.API.Data.Mongo;

internal class MongoDbOptions
{
  private string _connectionString = string.Empty;
  public string Username { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
  public string Host { get; set; } = string.Empty;
  public int Port { get; set; } = 27017;
  public string DatabaseName { get; set; } = string.Empty;

  public string ConnectionString
  {
    get => string.IsNullOrWhiteSpace(_connectionString) is false
        ? _connectionString
        : $"mongodb://{Username}:{Password}@{Host}:{Port}/{DatabaseName}?authMechanism=SCRAM-SHA-256&authSource=admin&retryWrites=true&w=majority";
    set => _connectionString = value;
  }
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