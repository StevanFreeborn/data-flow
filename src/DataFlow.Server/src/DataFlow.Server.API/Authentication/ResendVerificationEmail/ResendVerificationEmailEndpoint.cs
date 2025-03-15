namespace DataFlow.Server.API.Authentication.ResendVerificationEmail;

internal static class ResendVerificationEmailEndpoint
{
  private const string Route = "/resend-verification-email";

  public static void MapResendVerificationEndpoint(this WebApplication app)
  {
    app
      .MapPost(Route, HandleAsync)
      .RequireAuthorization();
  }

  private static async Task<IResult> HandleAsync([AsParameters] ResendVerificationEmailRequest req)
  {
    var validationResult = await req.Validator.ValidateAsync(req.Dto);

    if (validationResult.IsValid is false)
    {
      return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var userResult = await req.UserService.GetUserByEmailAsync(req.Dto.Email);

    var problemTitle = "Resend verification email failed";
    var problemDetail = "Unable to resend verification email. See errors for details.";

    if (userResult.IsFailed)
    {
      return Results.Problem(
        title: problemTitle,
        detail: problemDetail,
        statusCode: (int)HttpStatusCode.NotFound,
        extensions: new Dictionary<string, object?> { { "Errors", userResult.Errors } }
      );
    }

    if (userResult.Value.IsVerified)
    {
      return Results.Problem(
        title: problemTitle,
        detail: problemDetail,
        statusCode: (int)HttpStatusCode.Conflict,
        extensions: new Dictionary<string, object?> { { "Errors", new[] { new UserAlreadyVerifiedError(userResult.Value.Email) } } }
      );
    }

    await req.TokenService.RevokeUserVerificationTokensAsync(userResult.Value.Id);

    var tokenResult = await req.TokenService.GenerateVerificationToken(userResult.Value.Id);

    if (tokenResult.IsFailed)
    {
      return Results.Problem(
        title: problemTitle,
        detail: problemDetail,
        statusCode: (int)HttpStatusCode.InternalServerError,
        extensions: new Dictionary<string, object?> { { "Errors", tokenResult.Errors } }
      );
    }

    var emailMessage = EmailBuilder.BuildVerificationEmail(
      userResult.Value.Email,
      tokenResult.Value.Token,
      req.CorsOptions.Value.AllowedOrigins[0]
    );

    var emailResult = await req.EmailService.SendEmailAsync(emailMessage);

    if (emailResult.IsFailed)
    {
      return Results.Problem(
        title: "Resend verification email failed",
        detail: "Unable to resend verification email. See errors for details.",
        statusCode: (int)HttpStatusCode.InternalServerError,
        extensions: new Dictionary<string, object?> { { "Errors", emailResult.Errors } }
      );
    }

    return Results.NoContent();
  }
}