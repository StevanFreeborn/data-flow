namespace DataFlow.Server.API.Email;

internal interface IEmailService
{
  Task<Result> SendEmailAsync(EmailMessage message);
}