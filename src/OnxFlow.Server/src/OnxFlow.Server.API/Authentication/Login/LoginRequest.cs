namespace OnxFlow.Server.API.Authentication.Login;

internal record LoginDto(string Email, string Password)
{
  internal LoginDto() : this(string.Empty, string.Empty) { }
}

internal class LoginDtoValidator : AbstractValidator<LoginDto>
{
  public LoginDtoValidator()
  {
    RuleFor(static dto => dto.Email)
      .NotEmpty()
      .EmailAddress()
      .WithMessage("Email must be a valid email address.");

    RuleFor(static dto => dto.Password).NotEmpty();
  }
}

internal record LoginRequest(
  HttpContext Context,
  [FromBody] LoginDto Dto,
  [FromServices] IValidator<LoginDto> Validator,
  [FromServices] IUserService UserService
);