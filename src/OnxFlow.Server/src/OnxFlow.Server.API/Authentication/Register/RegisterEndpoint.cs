namespace OnxFlow.Server.API.Authentication.Register;

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
      var emailMessage = BuildVerificationEmail(
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

  private static EmailMessage BuildVerificationEmail(string email, string token, string origin)
  {
    // TODO: Use a templating engine for email content
    return new()
    {
      To = email,
      Subject = "Welcome to OnxFlow! Verify your account to get started.",
      HtmlContent = $"""
        <h1>Welcome to OnxFlow!</h1>
        <p>We're excited to welcome you to OnxFlow! Before you begin we need to verify your account. Follow these steps to complete the verification process:</p>
        <p>Click the link below to verify your account:</p>
        <a href='{origin}/open/verify-account?t={token}'>Verify Account</a>
        <p>If you didn't create an account with OnxFlow, please ignore this email.</p>
      """
    };
  }
}