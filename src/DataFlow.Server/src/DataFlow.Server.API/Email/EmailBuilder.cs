namespace DataFlow.Server.API.Email;

internal static class EmailBuilder
{
  internal static EmailMessage BuildVerificationEmail(string email, string token, string origin)
  {
    // TODO: Use a templating engine for email content
    return new()
    {
      To = email,
      Subject = "Welcome to DataFlow! Verify your account to get started.",
      HtmlContent = $"""
        <h1>Welcome to DataFlow!</h1>
        <p>We're excited to welcome you to DataFlow! Before you begin we need to verify your account. Follow these steps to complete the verification process:</p>
        <p>Click the link below to verify your account:</p>
        <a href='{origin}/open/verify-account?t={token}'>Verify Account</a>
        <p>If you didn't create an account with DataFlow, please ignore this email.</p>
      """
    };
  }
}