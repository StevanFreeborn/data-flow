namespace OnxFlow.Server.API.Email;

internal class EmailFailedError : Error
{
  public EmailFailedError() : base("Failed to send email")
  {
  }
}