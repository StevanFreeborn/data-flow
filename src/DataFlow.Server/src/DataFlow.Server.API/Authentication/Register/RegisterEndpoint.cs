namespace DataFlow.Server.API.Authentication.Register;

internal static class RegisterEndpoint
{
  private const string Route = "/register";

  public static void MapRegisterEndpoint(this WebApplication app)
  {
    app.MapPost(Route, HandleAsync);
  }

  private static async Task<IResult> HandleAsync([AsParameters] RegisterRequest req)
  {
    var validationResult = await req.Validator.ValidateAsync(req.Dto);

    if (validationResult.IsValid is false)
    {
      return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var registrationResult = await req.UserService.RegisterUserAsync(req.Dto.ToUser());

    if (registrationResult.IsFailed)
    {
      return Results.Problem(
        title: "Registration failed",
        detail: "Unable to register user. See errors for details.",
        statusCode: (int)HttpStatusCode.Conflict,
        extensions: new Dictionary<string, object?> { { "Errors", registrationResult.Errors } }
      );
    }

    var tokenResult = await req.TokenService.GenerateVerificationToken(registrationResult.Value);

    if (tokenResult.IsFailed)
    {
      req.Logger.LogError("Failed to generate verification token for user: {UserId}", registrationResult.Value);
    }

    if (tokenResult.IsSuccess)
    {
      var emailMessage = EmailBuilder.BuildVerificationEmail(
        req.Dto.Email,
        tokenResult.Value.Token,
        req.CorsOptions.Value.AllowedOrigins[0]
      );

      var emailResult = await req.EmailService.SendEmailAsync(emailMessage);

      if (emailResult.IsFailed)
      {
        req.Logger.LogError("Failed to send verification email for user: {UserId}", registrationResult.Value);
      }
    }

    return Results.Created(
      uri: $"/users/{registrationResult.Value}",
      value: new RegisterResponse(registrationResult.Value)
    );
  }
}