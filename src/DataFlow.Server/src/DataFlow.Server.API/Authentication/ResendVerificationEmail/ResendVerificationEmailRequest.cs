internal record ResendVerificationEmailDto(string Email);

internal class ResendVerificationEmailDtoValidator : AbstractValidator<ResendVerificationEmailDto>
{
  public ResendVerificationEmailDtoValidator()
  {
    RuleFor(static dto => dto.Email)
      .NotEmpty()
      .EmailAddress()
      .WithMessage("Email must be a valid email address.");
  }
}

internal record ResendVerificationEmailRequest(
  [FromBody] ResendVerificationEmailDto Dto,
  [FromServices] IValidator<ResendVerificationEmailDto> Validator,
  [FromServices] IUserService UserService,
  [FromServices] IEmailService EmailService,
  [FromServices] ITokenService TokenService,
  [FromServices] IOptions<CorsOptions> CorsOptions
);