using System.Globalization;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;

namespace DataFlow.Server.API.Tests.Infrastructure;

public class AppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
  private const string MongoDbImage = "mongo:latest";
  private readonly MongoDbContainer _mongoDbContainer = new MongoDbBuilder()
      .WithImage(MongoDbImage)
      .Build();

  private readonly IContainer _mailHogContainer = new ContainerBuilder()
    .WithImage("mailhog/mailhog")
    .WithPortBinding(1025, true)
    .WithPortBinding(8025, true)
    .Build();

  public async Task InitializeAsync()
  {
    await _mongoDbContainer.StartAsync();
    await _mailHogContainer.StartAsync();
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    var settings = $$"""
    {
      "JwtOptions": {
      "Secret": {{TestJwtTokenBuilder.TestJwtSecret}},
      "Issuer": "TestIssuer",
      "Audience": "TestAudience",
      "ExpiryInMinutes": 5
      }
    }
    """;

    var config = new ConfigurationBuilder()
      .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(settings)))
      .Build();

    builder.ConfigureAppConfiguration((_, c) => c.AddConfiguration(config));

    builder.ConfigureLogging(l => l.ClearProviders());

    builder.ConfigureTestServices(services =>
    {
      services.Configure<JwtOptions>(options =>
      {
        options.Secret = TestJwtTokenBuilder.TestJwtSecret;
        options.Audience = TestJwtTokenBuilder.TestJwtAudience;
        options.Issuer = TestJwtTokenBuilder.TestJwtIssuer;
        options.ExpiryInMinutes = TestJwtTokenBuilder.TestJwtExpiryInMinutes;
      });

      services.Configure<MongoDbOptions>(options =>
      {
        options.ConnectionString = _mongoDbContainer.GetConnectionString();
        options.DatabaseName = "tests";
      });

      services.Configure<SmtpOptions>(options =>
      {
        options.SmtpAddress = _mailHogContainer.Hostname;
        options.SmtpPort = _mailHogContainer.GetMappedPublicPort(1025);
        options.SenderEmail = "DataFlowTesting@test.com";
        options.SenderPassword = string.Empty;
      });

      services.Configure<CorsOptions>(options => options.AllowedOrigins = ["https://localhost:3001"]);

      var mailHogBaseUrl = $"http://{_mailHogContainer.Hostname}:{_mailHogContainer.GetMappedPublicPort(8025)}";
      services.AddHttpClient<MailHogService>(client => client.BaseAddress = new Uri(mailHogBaseUrl));

      var encryptionService = services.BuildServiceProvider().GetRequiredService<IEncryptionService>();
      services.Configure<EncryptionOptions>(options => options.Key = encryptionService.GenerateKey());
    });
  }

  public new async Task DisposeAsync()
  {
    await _mongoDbContainer.StopAsync();
    await _mailHogContainer.StopAsync();

    await _mongoDbContainer.DisposeAsync();
    await _mailHogContainer.DisposeAsync();
  }
}