namespace OnxFlow.Server.API.Tests.Data;

internal static class FakeDataFactory
{
  internal static readonly Faker<SmtpOptions> SmtpOptions = new Faker<SmtpOptions>()
    .RuleFor(static t => t.SmtpAddress, static f => f.Internet.Ip())
    .RuleFor(static t => t.SmtpPort, static f => f.Random.Int(1, 65535))
    .RuleFor(static t => t.SenderEmail, static f => f.Internet.Email())
    .RuleFor(static t => t.SenderPassword, static f => string.Empty);

  internal static readonly Faker<EmailMessage> EmailMessage = new Faker<EmailMessage>()
    .RuleFor(static t => t.Subject, static f => f.Lorem.Sentence())
    .RuleFor(static t => t.HtmlContent, static f => f.Lorem.Paragraphs(3))
    .RuleFor(static t => t.To, static f => f.Person.Email);
}