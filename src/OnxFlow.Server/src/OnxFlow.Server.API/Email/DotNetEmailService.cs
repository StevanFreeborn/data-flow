namespace OnxFlow.Server.API.Email;

internal class DotNetEmailService(
  IOptions<SmtpOptions> smtpOptions,
  ILogger<DotNetEmailService> logger,
  IEmailClient client
) : IEmailService
{
  private readonly SmtpOptions _smtpOptions = smtpOptions.Value;
  private readonly ILogger<DotNetEmailService> _logger = logger;
  private readonly IEmailClient _client = client;

  public async Task<Result> SendEmailAsync(EmailMessage message)
  {
    var attempts = 0;

    while (attempts < 3)
    {
      try
      {
        using var email = new MailMessage(_smtpOptions.SenderEmail, message.To)
        {
          Subject = message.Subject,
          Body = message.HtmlContent,
          IsBodyHtml = true
        };

        await _client.SendMailAsync(email);
        return Result.Ok();
      }
      catch (Exception ex) when (ex is SmtpFailedRecipientException or SmtpException)
      {
        attempts++;
        _logger.LogError(ex, "Failed to send email. Attempt {AttemptNumber}", attempts);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to send email");
        break;
      }
    }

    return Result.Fail(new EmailFailedError());
  }
}