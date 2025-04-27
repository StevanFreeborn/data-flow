namespace DataFlow.Server.API.Tests.Integration;

public class ResendVerificationEmailEndpointTests(AppFactory factory) : IntegrationTest(factory)
{
  private const string VerifyAccountEndpoint = "/resend-verification-email";

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  [InlineData(null)]
  [InlineData("invalid-email")]
  public async Task ResendVerificationEmail_WhenEmailIsInvalid_ItShould400StatusCodeWithValidationProblemDetails(string? email)
  {
    var response = await Client.PostAsJsonAsync(VerifyAccountEndpoint, new
    {
      Email = email,
    });

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var responseBody = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
    responseBody.Should().NotBeNull();
    responseBody!.Errors.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task ResendVerificationEmail_WhenEmailIsValidButNoExistingUser_ItShouldReturn404StatusCodeWithProblemDetails()
  {
    var response = await Client.PostAsJsonAsync(VerifyAccountEndpoint, new
    {
      Email = "test@test.com",
    });

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);

    var responseBody = await response.Content.ReadFromJsonAsync<ProblemDetails>();
    responseBody.Should().NotBeNull();
    responseBody!.Extensions.Should().ContainKey("Errors");
  }

  [Fact]
  public async Task ResendVerificationEmail_WhenEmailIsForExistingVerifiedUser_ItShouldReturn409StatusCodeWithProblemDetails()
  {
    var testUserEmail = "test@test.com";
    var (_, testUser) = FakeDataFactory.TestUser.Generate();
    testUser.Email = testUserEmail;
    testUser.IsVerified = true;

    await Context.GetCollection<User>().InsertOneAsync(testUser);

    var response = await Client.PostAsJsonAsync(VerifyAccountEndpoint, new
    {
      Email = testUserEmail,
    });

    response.StatusCode.Should().Be(HttpStatusCode.Conflict);

    var responseBody = await response.Content.ReadFromJsonAsync<ProblemDetails>();
    responseBody.Should().NotBeNull();
    responseBody!.Extensions.Should().ContainKey("Errors");
  }

  [Fact]
  public async Task ResendVerificationEmail_WhenEmailIsForExistingUnverifiedUser_ItShouldReturn204StatusCodeAndSendVerificationEmail()
  {
    var testUserEmail = $"test.user.{Guid.NewGuid()}@test.com";
    var emailParts = testUserEmail.Split('@');
    var testMailbox = emailParts[0];
    var testDomain = emailParts[1];
    var (_, testUser) = FakeDataFactory.TestUser.Generate();
    testUser.Email = testUserEmail;

    await Context.GetCollection<User>().InsertOneAsync(testUser);

    var response = await Client.PostAsJsonAsync(VerifyAccountEndpoint, new
    {
      Email = testUserEmail,
    });

    response.StatusCode.Should().Be(HttpStatusCode.NoContent);

    var emailSearchResult = await MailHogService.SearchEmailAsync(new(MailHogSearchKind.To, testUserEmail));
    emailSearchResult.Count.Should().Be(1);
    emailSearchResult.Items.Should().NotBeNullOrEmpty();
    emailSearchResult.Items.First().To
      .Should()
      .ContainSingle(t =>
        t.Mailbox == testMailbox && t.Domain == testDomain
      );

    var email = await MailHogService.GetEmailAsync(emailSearchResult.Items.First().Id);
    email.Content.Body.Should().Contain("Verify Account");
  }
}