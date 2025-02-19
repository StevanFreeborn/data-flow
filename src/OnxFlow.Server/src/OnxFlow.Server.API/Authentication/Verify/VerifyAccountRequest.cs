namespace OnxFlow.Server.API.Authentication.Verify;

internal record VerifyAccountDto(string Token)
{
  internal VerifyAccountDto() : this(string.Empty) { }
}

internal class VerifyAccountDtoValidator : AbstractValidator<VerifyAccountDto>
{
  public VerifyAccountDtoValidator()
  {
    RuleFor(static dto => dto.Token).NotEmpty();
  }
}

internal record VerifyAccountRequest(
  [FromBody] VerifyAccountDto Dto,
  [FromServices] IValidator<VerifyAccountDto> Validator,
  [FromServices] IUserService UserService,
  [FromServices] ITokenService TokenService
);