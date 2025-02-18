namespace OnxFlow.Server.API.Authentication;

internal class InvalidLoginError : Error
{
  internal InvalidLoginError() : base("Email/Password combination is not valid")
  {
  }
}

internal class LoginFailedError : Error
{
  internal LoginFailedError() : base("Login failed")
  {
  }
}

internal class GenerateRefreshTokenError : Error
{
  internal GenerateRefreshTokenError() : base("Failed to generate refresh token")
  {
  }
}

internal class GenerateVerificationTokenError : Error
{
  internal GenerateVerificationTokenError() : base("Failed to generate verification token")
  {
  }
}

internal class TokenDoesNotExistError : Error
{
  internal TokenDoesNotExistError(string identifier) : base($"Token does not exist with identifier: {identifier}")
  {
  }
}

internal class ExpiredTokenError : Error
{
  internal ExpiredTokenError(string token) : base($"Token has expired with: {token}")
  {
  }
}

internal class InvalidTokenError : Error
{
  internal InvalidTokenError(string token) : base($"Token is invalid with: {token}")
  {
  }
}