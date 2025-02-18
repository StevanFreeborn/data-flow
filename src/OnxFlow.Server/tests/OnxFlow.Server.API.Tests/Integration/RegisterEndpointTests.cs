namespace OnxFlow.Server.API.Tests.Integration;

public partial class RegisterEndpointTests(AppFactory factory) : IntegrationTest(factory)
{
  private const string RegisterEndpoint = "/register";

  [Fact]
  public async Task Register_WhenCalledAndGivenValidEmailAndPassword_ItShouldReturn201StatusCodeWithRegisteredUsersId()
  {
    var (password, user) = FakeDataFactory.TestUser.Generate();

    var registerResponse = await Client.PostAsJsonAsync(RegisterEndpoint, new
    {
      email = user.Email,
      password
    });

    registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

    var registerResponseBody = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();

    registerResponseBody.Should().NotBeNull();
    registerResponseBody!.Id.Should().NotBeNullOrEmpty();

    var createdUser = await Context.GetCollection<User>()
      .Find(u => u.Id == registerResponseBody.Id)
      .SingleOrDefaultAsync();

    createdUser.Should().NotBeNull();
    createdUser.Password.Should().NotBe(password);
    createdUser.EncryptionKey.Should().NotBeNullOrEmpty();
  }

  [GeneratedRegex(@"\/open\/verify-account\?t=[a-zA-Z0-9]+")]
  private static partial Regex VerifyAccountLinkRegex();

  [Fact]
  public async Task Register_WhenCalledAndGivenValidEmailAndPassword_ItShouldSendVerificationEmail()
  {
    var (password, _) = FakeDataFactory.TestUser.Generate();
    var testEmail = $"test.user.{Guid.NewGuid()}@test.com";
    var emailParts = testEmail.Split('@');
    var testMailbox = emailParts[0];
    var testDomain = emailParts[1];

    var registerResponse = await Client.PostAsJsonAsync(RegisterEndpoint, new
    {
      email = testEmail,
      password
    });

    registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

    var registerResponseBody = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();

    registerResponseBody.Should().NotBeNull();

    registerResponseBody?.Id.Should().NotBeNullOrEmpty();

    var emailSearchResult = await MailHogService.SearchEmailAsync(new(MailHogSearchKind.To, testEmail));
    emailSearchResult.Count.Should().Be(1);
    emailSearchResult.Items.Should().NotBeNullOrEmpty();
    emailSearchResult.Items.First().To
      .Should()
      .ContainSingle(t =>
        t.Mailbox == testMailbox && t.Domain == testDomain
      );

    var email = await MailHogService.GetEmailAsync(emailSearchResult.Items.First().Id);
    email.Content.Body.Should().Contain("Verify Account");
    email.Content.Body.Should().MatchRegex(VerifyAccountLinkRegex());
  }

  [Fact]
  public async Task Register_WhenCalledAndGivenInvalidPassword_ItShouldReturn400StatusCodeWithValidationProblemDetails()
  {
    var (_, user) = FakeDataFactory.TestUser.Generate();

    var registerResponse = await Client.PostAsJsonAsync(RegisterEndpoint, new
    {
      email = user.Email,
      password = "invalid_password",
    });

    registerResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var registerResponseBody = await registerResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    registerResponseBody.Should().NotBeNull();
    registerResponseBody!.Errors.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task Register_WhenCalledAndGivenInvalidEmail_ItShouldReturn400StatusCodeWithValidationProblemDetails()
  {
    var (password, _) = FakeDataFactory.TestUser.Generate();

    var registerResponse = await Client.PostAsJsonAsync(RegisterEndpoint, new
    {
      email = "invalid_email",
      password
    });

    registerResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var registerResponseBody = await registerResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    registerResponseBody.Should().NotBeNull();
    registerResponseBody!.Errors.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task Register_WhenCalledAndGivenEmailForExistingUser_ItShouldReturn409StatusCodeWithProblemDetails()
  {
    var (userPassword, alreadyExistingUser) = FakeDataFactory.TestUser.Generate();

    await Context.GetCollection<User>().InsertOneAsync(alreadyExistingUser);

    var registerResponse = await Client.PostAsJsonAsync(RegisterEndpoint, new
    {
      email = alreadyExistingUser.Email,
      password = userPassword,
    });

    registerResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

    var registerResponseBody = await registerResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    registerResponseBody.Should().NotBeNull();
  }
}