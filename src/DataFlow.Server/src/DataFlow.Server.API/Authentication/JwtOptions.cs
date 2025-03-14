namespace DataFlow.Server.API.Authentication;

internal class JwtOptions
{
  public string Secret { get; set; } = string.Empty;
  public string Issuer { get; set; } = string.Empty;
  public string Audience { get; set; } = string.Empty;
  public int ExpiryInMinutes { get; set; }
}

internal class JwtOptionsSetup(IConfiguration configuration) : IConfigureOptions<JwtOptions>
{
  private const string SectionName = nameof(JwtOptions);
  private readonly IConfiguration _configuration = configuration;

  public void Configure(JwtOptions options)
  {
    _configuration
      .GetSection(SectionName)
      .Bind(options);
  }
}