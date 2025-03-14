namespace DataFlow.Server.API.Authentication.Register;

internal record RegisterDto(string Email, string Password)
{
  internal RegisterDto() : this(string.Empty, string.Empty) { }

  internal User ToUser()
  {
    return new User
    {
      Email = Email,
      Password = Password
    };
  }
}

internal class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
  public RegisterDtoValidator()
  {
    RuleFor(static dto => dto.Email)
      .NotEmpty()
      .EmailAddress()
      .WithMessage("Email must be a valid email address.");

    // At least one uppercase letter - (?=.*[A-Z])
    // At least one lowercase letter - (?=.*[a-z])
    // At least one number - (?=.*\d)
    // At least one special character - (?=.*\W)
    // Must be at least 8 characters long - {8,}
    RuleFor(static dto => dto.Password)
      .NotEmpty()
      .Matches(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*\W)[A-Za-z\d\W]{8,}$")
      .WithMessage("Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number and one special character.");
  }
}

internal record RegisterRequest(
  [FromBody] RegisterDto Dto,
  [FromServices] IOptions<CorsOptions> CorsOptions,
  [FromServices] IValidator<RegisterDto> Validator,
  [FromServices] IUserService UserService,
  [FromServices] IEmailService EmailService,
  [FromServices] ILogger<RegisterRequest> Logger,
  [FromServices] ITokenService TokenService
);