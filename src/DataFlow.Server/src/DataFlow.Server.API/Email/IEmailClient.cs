namespace DataFlow.Server.API.Email;

internal interface IEmailClient
{
  Task SendMailAsync(MailMessage message);
}