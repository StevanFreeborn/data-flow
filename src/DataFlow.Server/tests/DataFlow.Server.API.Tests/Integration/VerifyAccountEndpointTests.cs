namespace DataFlow.Server.API.Tests.Integration;

public class VerifyAccountEndpointTests(AppFactory factory) : IntegrationTest(factory)
{
  private const string VerifyAccountEndpoint = "/verify-account";

  [Fact]
  public async Task VerifyAccount_WhenCalledAndNoTokenIsGiven_ItShouldReturn400StatusCodeWithValidationProblemDetails()
  {
    var verifyResponse = await Client.PostAsJsonAsync(VerifyAccountEndpoint, new
    {
      token = "",
    });

    verifyResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var verifyResponseBody = await verifyResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
    verifyResponseBody.Should().NotBeNull();
    verifyResponseBody!.Errors.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task VerifyAccount_WhenCalledAndTokenDoesNotExist_ItShouldReturn404StatusCodeWithProblemDetails()
  {
    var verifyResponse = await Client.PostAsJsonAsync(VerifyAccountEndpoint, new
    {
      token = "non_existing_token",
    });

    verifyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

    var verifyResponseBody = await verifyResponse.Content.ReadFromJsonAsync<ProblemDetails>();
    verifyResponseBody.Should().NotBeNull();
    verifyResponseBody!.Title.Should().Be("Verification failed");
    verifyResponseBody!.Detail.Should().Be("Unable to verify account. See errors for details.");
    verifyResponseBody!.Extensions.Should().ContainKey("Errors");
  }

  [Fact]
  public async Task VerifyAccount_WhenCalledAndTokenIsExpired_ItShouldReturn400StatusCodeWithProblemDetails()
  {
    var verificationToken = new VerificationToken(FakeDataFactory.VerificationToken.Generate())
    {
      ExpiresAt = DateTime.UtcNow.AddMinutes(-30),
    };

    await Context.GetCollection<BaseToken>().InsertOneAsync(verificationToken);

    var verifyResponse = await Client.PostAsJsonAsync(VerifyAccountEndpoint, new
    {
      token = verificationToken.Token,
    });

    verifyResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var verifyResponseBody = await verifyResponse.Content.ReadFromJsonAsync<ProblemDetails>();
    verifyResponseBody.Should().NotBeNull();
    verifyResponseBody!.Title.Should().Be("Verification failed");
    verifyResponseBody!.Detail.Should().Be("Unable to verify account. See errors for details.");
    verifyResponseBody!.Extensions.Should().ContainKey("Errors");

    var revokedToken = await Context.GetCollection<BaseToken>()
      .Find(t => t.Id == verificationToken.Id)
      .FirstOrDefaultAsync();

    revokedToken.Should().NotBeNull();
    revokedToken!.Revoked.Should().BeTrue();
  }

  [Fact]
  public async Task VerifyAccount_WhenCalledAndTokenIsRevoked_ItShouldReturn400StatusCodeWithProblemDetails()
  {
    var verificationToken = new VerificationToken(FakeDataFactory.VerificationToken.Generate())
    {
      Revoked = true,
    };

    await Context.GetCollection<BaseToken>().InsertOneAsync(verificationToken);

    var verifyResponse = await Client.PostAsJsonAsync(VerifyAccountEndpoint, new
    {
      token = verificationToken.Token,
    });

    verifyResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var verifyResponseBody = await verifyResponse.Content.ReadFromJsonAsync<ProblemDetails>();
    verifyResponseBody.Should().NotBeNull();
    verifyResponseBody!.Title.Should().Be("Verification failed");
    verifyResponseBody!.Detail.Should().Be("Unable to verify account. See errors for details.");
    verifyResponseBody!.Extensions.Should().ContainKey("Errors");
  }

  [Fact]
  public async Task VerifyAccount_WhenCalledAndTokenBelongsToNonExistentUser_ItShouldReturn404StatusCodeWithProblemDetails()
  {
    var verificationToken = FakeDataFactory.VerificationToken.Generate();
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();

    await Context.GetCollection<BaseToken>().InsertOneAsync(verificationToken);

    var verifyResponse = await Client.PostAsJsonAsync(VerifyAccountEndpoint, new
    {
      token = verificationToken.Token,
    });

    verifyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

    var verifyResponseBody = await verifyResponse.Content.ReadFromJsonAsync<ProblemDetails>();
    verifyResponseBody.Should().NotBeNull();
    verifyResponseBody!.Title.Should().Be("Verification failed");
    verifyResponseBody!.Detail.Should().Be("Unable to verify account. See errors for details.");
    verifyResponseBody!.Extensions.Should().ContainKey("Errors");

    var revokedToken = await Context.GetCollection<BaseToken>()
      .Find(t => t.Id == verificationToken.Id)
      .FirstOrDefaultAsync();

    revokedToken.Should().NotBeNull();
  }

  [Fact]
  public async Task VerifyAccount_WhenCalledAndTokenBelongsToAlreadyVerifiedUser_ItShouldReturn409StatusCodeWithProblemDetails()
  {
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();
    existingUser.IsVerified = true;

    var verificationToken = new VerificationToken(FakeDataFactory.VerificationToken.Generate())
    {
      UserId = existingUser.Id,
    };

    await Context.GetCollection<BaseToken>().InsertOneAsync(verificationToken);
    await Context.GetCollection<User>().InsertOneAsync(existingUser);

    var verifyResponse = await Client.PostAsJsonAsync(VerifyAccountEndpoint, new
    {
      token = verificationToken.Token,
    });

    verifyResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

    var verifyResponseBody = await verifyResponse.Content.ReadFromJsonAsync<ProblemDetails>();
    verifyResponseBody.Should().NotBeNull();
    verifyResponseBody!.Title.Should().Be("Verification failed");
    verifyResponseBody!.Detail.Should().Be("Unable to verify account. See errors for details.");
    verifyResponseBody!.Extensions.Should().ContainKey("Errors");

    var revokedToken = await Context.GetCollection<BaseToken>()
      .Find(t => t.Id == verificationToken.Id)
      .FirstOrDefaultAsync();

    revokedToken.Should().NotBeNull();
  }

  [Fact]
  public async Task VerifyAccount_WhenCalledAndTokenValid_ItShouldReturn204StatusCodeAndVerifyAccount()
  {
    var (_, existingUser) = FakeDataFactory.TestUser.Generate();
    var verificationToken = new VerificationToken(FakeDataFactory.VerificationToken.Generate())
    {
      UserId = existingUser.Id,
    };

    await Context.GetCollection<BaseToken>().InsertOneAsync(verificationToken);
    await Context.GetCollection<User>().InsertOneAsync(existingUser);

    var verifyResponse = await Client.PostAsJsonAsync(VerifyAccountEndpoint, new
    {
      token = verificationToken.Token,
    });

    verifyResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

    var verifyResponseBody = await verifyResponse.Content.ReadAsStringAsync();

    verifyResponseBody.Should().BeEmpty();

    var verifiedUser = await Context.GetCollection<User>()
      .Find(u => u.Id == existingUser.Id)
      .FirstOrDefaultAsync();

    verifiedUser.Should().NotBeNull();
    verifiedUser!.IsVerified.Should().BeTrue();

    var revokedToken = await Context.GetCollection<BaseToken>()
      .Find(t => t.Id == verificationToken.Id)
      .FirstOrDefaultAsync();

    revokedToken.Should().BeNull();
  }
}