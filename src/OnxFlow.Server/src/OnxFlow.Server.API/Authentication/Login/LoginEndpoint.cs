namespace OnxFlow.Server.API.Authentication.Login;

internal static class LoginEndpoint
{
  private const string Route = "/login";

  public static void MapLoginEndpoint(this WebApplication app)
  {
    app.MapPost(Route, HandleAsync);
  }

  private static async Task<IResult> HandleAsync([AsParameters] LoginRequest req)
  {
    var validationResult = await req.Validator.ValidateAsync(req.Dto);

    if (validationResult.IsValid is false)
    {
      return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var loginResult = await req.UserService.LoginUserAsync(req.Dto.Email, req.Dto.Password);

    var problemTitle = "Login failed";
    var problemDetail = "Unable to login user. See errors for details.";

    if (loginResult.IsFailed && loginResult.Errors.Exists(static e => e is InvalidLoginError))
    {
      return Results.Problem(
        title: problemTitle,
        detail: problemDetail,
        statusCode: (int)HttpStatusCode.Unauthorized,
        extensions: new Dictionary<string, object?> { { "Errors", loginResult.Errors } }
      );
    }

    if (loginResult.IsFailed && loginResult.Errors.Exists(static e => e is UserNotVerifiedError))
    {
      return Results.Problem(
        title: problemTitle,
        detail: problemDetail,
        statusCode: (int)HttpStatusCode.Forbidden,
        extensions: new Dictionary<string, object?> { { "Errors", loginResult.Errors } }
      );
    }

    req.Context.Response.SetRefreshTokenCookie(
      loginResult.Value.RefreshToken.Token,
      loginResult.Value.RefreshToken.ExpiresAt
    );

    return Results.Ok(new LoginResponse(loginResult.Value.AccessToken));
  }
}