namespace OnxFlow.Server.API.Encryption;

internal class EncryptionOptions
{
  public string Key { get; set; } = string.Empty;
}

internal class EncryptionOptionsSetup(IConfiguration configuration) : IConfigureOptions<EncryptionOptions>
{
  private const string SectionName = nameof(EncryptionOptions);
  private readonly IConfiguration _configuration = configuration;

  public void Configure(EncryptionOptions options)
  {
    _configuration
      .GetSection(SectionName)
      .Bind(options);
  }
}