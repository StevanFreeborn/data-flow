namespace OnxFlow.Server.API.Tests.Integration;

public class LoginEndpointTests(AppFactory factory) : IntegrationTest(factory)
{
  private const string LoginEndpoint = "/login";

  [Fact]
  public async Task Login_WhenCalledAndGivenValidEmailAndPassword_ItShouldReturn200StatusCodeWithAccessTokenAndRefreshToken()
  {
    var (userPassword, existingUser) = FakeDataFactory.TestUser.Generate();
    existingUser.IsVerified = true;

    await Context.GetCollection<User>().InsertOneAsync(existingUser);

    var loginResponse = await Client.PostAsJsonAsync(LoginEndpoint, new
    {
      email = existingUser.Email,
      password = userPassword,
    });

    loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

    loginResponse.Headers
      .Should()
      .Contain(static h => h.Key == "Set-Cookie" && h.Value.Any(static v => v.Contains("onxRefreshToken", StringComparison.OrdinalIgnoreCase)));

    var loginResponseBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
    loginResponseBody.Should().NotBeNull();
    loginResponseBody!.AccessToken.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task Login_WhenCalledAndGivenValidEmailAndPasswordButUserIsNotVerified_ItShouldReturn403StatusCodeWithProblemDetails()
  {
    var (userPassword, existingUser) = FakeDataFactory.TestUser.Generate();

    await Context.GetCollection<User>().InsertOneAsync(existingUser);

    var loginResponse = await Client.PostAsJsonAsync(LoginEndpoint, new
    {
      email = existingUser.Email,
      password = userPassword,
    });

    loginResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

    var loginResponseBody = await loginResponse.Content.ReadFromJsonAsync<ProblemDetails>();
    loginResponseBody.Should().NotBeNull();
    loginResponseBody!.Title.Should().Be("Login failed");
    loginResponseBody!.Detail.Should().Be("Unable to login user. See errors for details.");
    loginResponseBody!.Extensions.Should().ContainKey("Errors");
  }

  [Fact]
  public async Task Login_WhenCalledAndGivenInvalidEmail_ItShouldReturn400StatusCodeWithValidationProblemDetails()
  {
    var (userPassword, existingUser) = FakeDataFactory.TestUser.Generate();

    await Context.GetCollection<User>().InsertOneAsync(existingUser);

    var loginResponse = await Client.PostAsJsonAsync(LoginEndpoint, new
    {
      email = "invalid_email",
      password = userPassword,
    });

    loginResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var loginResponseBody = await loginResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
    loginResponseBody.Should().NotBeNull();
    loginResponseBody!.Errors.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task Login_WhenCalledAndGivenIncorrectEmail_ItShouldReturn401StatusCodeWithProblemDetails()
  {
    var (userPassword, existingUser) = FakeDataFactory.TestUser.Generate();

    await Context.GetCollection<User>().InsertOneAsync(existingUser);

    var loginResponse = await Client.PostAsJsonAsync(LoginEndpoint, new
    {
      email = "incorrect@test.com",
      password = userPassword,
    });

    loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

    var loginResponseBody = await loginResponse.Content.ReadFromJsonAsync<ProblemDetails>();
    loginResponseBody.Should().NotBeNull();
    loginResponseBody!.Title.Should().Be("Login failed");
    loginResponseBody!.Detail.Should().Be("Unable to login user. See errors for details.");
    loginResponseBody!.Extensions.Should().ContainKey("Errors");
  }

  [Fact]
  public async Task Login_WhenCalledAndGivenIncorrectPassword_ItShouldReturn401StatusCodeWithProblemDetails()
  {
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();

    await Context.GetCollection<User>().InsertOneAsync(existingUser);

    var loginResponse = await Client.PostAsJsonAsync(LoginEndpoint, new
    {
      email = existingUser.Email,
      password = "incorrect_password",
    });

    loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

    var loginResponseBody = await loginResponse.Content.ReadFromJsonAsync<ProblemDetails>();
    loginResponseBody.Should().NotBeNull();
    loginResponseBody!.Title.Should().Be("Login failed");
    loginResponseBody!.Detail.Should().Be("Unable to login user. See errors for details.");
    loginResponseBody!.Extensions.Should().ContainKey("Errors");
  }

  [Fact]
  public async Task Login_WhenCalledAndGivenEmailForNonExistingUser_ItShouldReturn401StatusCodeWithProblemDetails()
  {
    var (userPassword, user) = FakeDataFactory.TestUser.Generate();

    var loginResponse = await Client.PostAsJsonAsync(LoginEndpoint, new
    {
      email = user.Email,
      password = userPassword,
    });

    loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

    var loginResponseBody = await loginResponse.Content.ReadFromJsonAsync<ProblemDetails>();
    loginResponseBody.Should().NotBeNull();
    loginResponseBody!.Title.Should().Be("Login failed");
    loginResponseBody!.Detail.Should().Be("Unable to login user. See errors for details.");
    loginResponseBody!.Extensions.Should().ContainKey("Errors");
  }
}