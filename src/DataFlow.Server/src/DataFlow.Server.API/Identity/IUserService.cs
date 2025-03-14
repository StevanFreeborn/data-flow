namespace DataFlow.Server.API.Identity;

internal interface IUserService
{
  Task<Result<(string AccessToken, RefreshToken RefreshToken)>> LoginUserAsync(string email, string password);
  Task<Result<string>> RegisterUserAsync(User user);
  Task<Result<User>> GetUserByEmailAsync(string userEmail);
  Task<Result> VerifyUserAsync(string userId);
  Task<Result<User>> GetUserByIdAsync(string userId);
}