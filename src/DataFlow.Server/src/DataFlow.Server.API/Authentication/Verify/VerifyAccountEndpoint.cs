namespace DataFlow.Server.API.Authentication.Verify;

internal static class VerifyAccountEndpoint
{
  private const string Route = "/verify-account";

  public static void MapVerifyAccountEndpoint(this WebApplication app)
  {
    app.MapPost(Route, HandleAsync);
  }

  private static async Task<IResult> HandleAsync([AsParameters] VerifyAccountRequest req)
  {
    var validationResult = await req.Validator.ValidateAsync(req.Dto);

    if (validationResult.IsValid is false)
    {
      return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var verifyTokenResult = await req.TokenService.VerifyVerificationTokenAsync(req.Dto.Token);

    var problemTitle = "Verification failed";
    var problemDetail = "Unable to verify account. See errors for details.";

    if (
      verifyTokenResult.IsFailed &&
      verifyTokenResult.Errors.Exists(static e => e is TokenDoesNotExistError)
    )
    {
      return Results.Problem(
        title: problemTitle,
        detail: problemDetail,
        statusCode: (int)HttpStatusCode.NotFound,
        extensions: new Dictionary<string, object?> { { "Errors", verifyTokenResult.Errors } }
      );
    }

    if (
      verifyTokenResult.IsFailed &&
      verifyTokenResult.Errors.Exists(static e => e is ExpiredTokenError or InvalidTokenError)
    )
    {
      await req.TokenService.RevokeVerificationTokenAsync(req.Dto.Token);

      return Results.Problem(
        title: problemTitle,
        detail: problemDetail,
        statusCode: (int)HttpStatusCode.BadRequest,
        extensions: new Dictionary<string, object?> { { "Errors", verifyTokenResult.Errors } }
      );
    }

    var verifyUserResult = await req.UserService.VerifyUserAsync(verifyTokenResult.Value.UserId);

    if (verifyUserResult.IsFailed && verifyUserResult.Errors.Exists(static e => e is UserDoesNotExistError))
    {
      return Results.Problem(
        title: problemTitle,
        detail: problemDetail,
        statusCode: (int)HttpStatusCode.NotFound,
        extensions: new Dictionary<string, object?> { { "Errors", verifyUserResult.Errors } }
      );
    }

    if (verifyUserResult.IsFailed && verifyUserResult.Errors.Exists(static e => e is UserAlreadyVerifiedError))
    {
      return Results.Problem(
        title: problemTitle,
        detail: problemDetail,
        statusCode: (int)HttpStatusCode.Conflict,
        extensions: new Dictionary<string, object?> { { "Errors", verifyUserResult.Errors } }
      );
    }

    await req.TokenService.RevokeVerificationTokenAsync(req.Dto.Token);
    await req.TokenService.RemoveAllInvalidVerificationTokensAsync(verifyTokenResult.Value.UserId);

    return Results.NoContent();
  }
}