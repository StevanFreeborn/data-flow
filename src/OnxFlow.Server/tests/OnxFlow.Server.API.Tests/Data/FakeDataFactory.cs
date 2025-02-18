using System.Security.Cryptography;

namespace OnxFlow.Server.API.Tests.Data;

internal static class FakeDataFactory
{
  internal static readonly UserGenerator TestUser = new();

  internal static readonly Faker<SmtpOptions> SmtpOptions = new Faker<SmtpOptions>()
    .RuleFor(static t => t.SmtpAddress, static f => f.Internet.Ip())
    .RuleFor(static t => t.SmtpPort, static f => f.Random.Int(1, 65535))
    .RuleFor(static t => t.SenderEmail, static f => f.Internet.Email())
    .RuleFor(static t => t.SenderPassword, static f => string.Empty);

  internal static readonly Faker<EmailMessage> EmailMessage = new Faker<EmailMessage>()
    .RuleFor(static t => t.Subject, static f => f.Lorem.Sentence())
    .RuleFor(static t => t.HtmlContent, static f => f.Lorem.Paragraphs(3))
    .RuleFor(static t => t.To, static f => f.Person.Email);

  internal static readonly Faker<VerificationToken> VerificationToken = new Faker<VerificationToken>()
    .CustomInstantiator(static f => new VerificationToken())
    .RuleFor(static t => t.Id, static f => ObjectId.GenerateNewId().ToString())
    .RuleFor(static t => t.UserId, static f => ObjectId.GenerateNewId().ToString())
    .RuleFor(static t => t.Token, static f => f.Random.AlphaNumeric(32))
    .RuleFor(static t => t.ExpiresAt, static f => DateTime.UtcNow.AddMinutes(15))
    .RuleFor(static t => t.Revoked, false)
    .RuleFor(static t => t.TokenType, "Verification");

  internal static readonly Faker<JwtOptions> JwtOption = new Faker<JwtOptions>()
    .RuleFor(static j => j.Audience, f => f.Internet.DomainName())
    .RuleFor(static j => j.ExpiryInMinutes, f => f.Random.Int(1, 60))
    .RuleFor(static j => j.Issuer, f => f.Internet.DomainName())
    .RuleFor(static j => j.Secret, GenerateJwtSecret());

  private static string GenerateJwtSecret()
  {
    var secret = new byte[32];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(secret);
    return Convert.ToBase64String(secret);
  }
}

internal sealed class UserGenerator
{
  private const string Password = "Password123!";

  private readonly Faker<User> _userFaker = new Faker<User>()
    .RuleFor(static u => u.Id, static f => ObjectId.GenerateNewId().ToString())
    .RuleFor(static u => u.Email, static f => f.Person.Email)
    .RuleFor(static u => u.Username, static f => f.Person.UserName)
    .RuleFor(
      static u => u.Password,
      static f => BCrypt.Net.BCrypt.HashPassword(Password)
    );

  internal (string userPassword, User user) Generate()
  {
    return (Password, _userFaker.Generate());
  }
}