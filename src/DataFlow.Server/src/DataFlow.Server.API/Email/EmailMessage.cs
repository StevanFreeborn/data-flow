namespace DataFlow.Server.API.Email;

internal class EmailMessage
{
  public string To { get; set; } = string.Empty;
  public string Subject { get; set; } = string.Empty;
  public string HtmlContent { get; set; } = string.Empty;
}