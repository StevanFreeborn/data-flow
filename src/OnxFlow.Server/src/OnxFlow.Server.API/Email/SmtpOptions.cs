namespace OnxFlow.Server.API.Email;

internal class SmtpOptions
{
  public string SmtpAddress { get; set; } = string.Empty;
  public int SmtpPort { get; set; }
  public string SenderEmail { get; set; } = string.Empty;
  public string SenderPassword { get; set; } = string.Empty;
}

internal class SmtpOptionsSetup(IConfiguration configuration) : IConfigureOptions<SmtpOptions>
{
  private const string SectionName = nameof(SmtpOptions);
  private readonly IConfiguration _configuration = configuration;

  public void Configure(SmtpOptions options)
  {
    _configuration
      .GetSection(SectionName)
      .Bind(options);
  }
}
