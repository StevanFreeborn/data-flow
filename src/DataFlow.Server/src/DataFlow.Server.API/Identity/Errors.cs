namespace DataFlow.Server.API.Identity;

internal class UserAlreadyExistError : Error
{
  internal UserAlreadyExistError(string identifier) : base($"User already exists with identifier: {identifier}")
  {
  }
}

internal class UserDoesNotExistError : Error
{
  internal UserDoesNotExistError(string identifier) : base($"User does not exist with identifier: {identifier}")
  {
  }
}

internal class UserNotVerifiedError : Error
{
  internal UserNotVerifiedError(string identifier) : base($"User is not verified with identifier: {identifier}")
  {
  }
}

internal class UserAlreadyVerifiedError : Error
{
  internal UserAlreadyVerifiedError(string identifier) : base($"User is already verified with identifier: {identifier}")
  {
  }
}