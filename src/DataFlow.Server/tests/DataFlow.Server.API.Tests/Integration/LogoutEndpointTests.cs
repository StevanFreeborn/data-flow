namespace DataFlow.Server.API.Tests.Integration;

public class LogoutEndpointTests(AppFactory factory) : IntegrationTest(factory)
{
  private const string LogoutEndpoint = "/logout";

  [Fact]
  public async Task Logout_WhenCalledByUnauthorizedUser_ItShouldReturn401StatusCodeWithProblemDetails()
  {
    var logoutResponse = await Client.PostAsync(new Uri(LogoutEndpoint, UriKind.Relative), null);

    logoutResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

    var logoutResponseBody = await logoutResponse.Content.ReadFromJsonAsync<ProblemDetails>();

    logoutResponseBody.Should().NotBeNull();
    logoutResponseBody!.Title.Should().Be("Unauthorized");
  }

  [Fact]
  public async Task Logout_WhenCalledByAuthorizedUser_ItShouldReturn200StatusCodeAndRevokeRefreshToken()
  {
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();
    var token = FakeDataFactory.RefreshToken.Generate();
    var userRefreshToken = new RefreshToken(token) { UserId = existingUser.Id };
    var userJwtToken = TestJwtTokenBuilder
      .Create()
      .WithClaim(new(JwtRegisteredClaimNames.Sub, existingUser.Id))
      .Build();

    await Context.GetCollection<User>().InsertOneAsync(existingUser);
    await Context.GetCollection<BaseToken>().InsertOneAsync(userRefreshToken);

    Client.DefaultRequestHeaders.Authorization = new("Bearer", userJwtToken);
    Client.DefaultRequestHeaders.Add("Cookie", $"onxRefreshToken={userRefreshToken.Token}");

    var logoutResponse = await Client.PostAsync(new Uri(LogoutEndpoint, UriKind.Relative), null);

    logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

    logoutResponse.Headers
      .Should()
      .Contain(
        h =>
          h.Key == "Set-Cookie" &&
          h.Value.Any(v => v.Contains("onxRefreshToken", StringComparison.OrdinalIgnoreCase))
      );

    var revokedToken = await Context.GetCollection<BaseToken>()
      .Find(t => t.Id == userRefreshToken.Id)
      .FirstOrDefaultAsync();

    revokedToken.Should().BeNull();
  }
}