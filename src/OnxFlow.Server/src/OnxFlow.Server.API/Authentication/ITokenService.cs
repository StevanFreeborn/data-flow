namespace OnxFlow.Server.API.Authentication;

internal interface ITokenService
{
  string GenerateAccessToken(User existingUser);
  Task<Result<RefreshToken>> GenerateRefreshToken(string userId);
  Task<Result<VerificationToken>> GenerateVerificationToken(string userId);
  Task<Result<(string AccessToken, RefreshToken RefreshToken)>> RefreshAccessTokenAsync(string userId, string refreshToken);
  Task RemoveAllInvalidRefreshTokensAsync(string userId);
  Task RevokeRefreshTokenAsync(string userId, string refreshToken);
  Task RevokeUserVerificationTokensAsync(string userId);
  Task<Result<BaseToken>> VerifyVerificationTokenAsync(string token);
  Task RevokeVerificationTokenAsync(string token);
  Task RemoveAllInvalidVerificationTokensAsync(string userId);
}