namespace DataFlow.Server.API.Email;

internal class SmtpEmailClient : IEmailClient, IDisposable
{
  private readonly SmtpClient _smtpClient;

  public SmtpEmailClient(IOptions<SmtpOptions> smtpOptions)
  {
    var smtpOptionsValue = smtpOptions.Value;
    _smtpClient = new SmtpClient(smtpOptionsValue.SmtpAddress, smtpOptionsValue.SmtpPort);

    if (string.IsNullOrEmpty(smtpOptionsValue.SenderPassword) is false)
    {
      _smtpClient.Credentials = new NetworkCredential(smtpOptionsValue.SenderEmail, smtpOptionsValue.SenderPassword);
      _smtpClient.EnableSsl = true;
    }
  }

  public void Dispose()
  {
    _smtpClient.Dispose();
  }

  public async Task SendMailAsync(MailMessage message)
  {
    await _smtpClient.SendMailAsync(message);
  }
}