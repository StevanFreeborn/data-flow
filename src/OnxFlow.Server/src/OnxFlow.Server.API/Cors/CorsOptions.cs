namespace OnxFlow.Server.API.Cors;

internal class CorsOptionsSetup(IConfiguration configuration) : IConfigureOptions<CorsOptions>
{
  private const string SectionName = nameof(CorsOptions);
  private readonly IConfiguration _configuration = configuration;

  public void Configure(CorsOptions options)
  {
    _configuration
      .GetSection(SectionName)
      .Bind(options);
  }
}

internal class CorsOptions
{
  public string[] AllowedOrigins { get; set; } = [];
}