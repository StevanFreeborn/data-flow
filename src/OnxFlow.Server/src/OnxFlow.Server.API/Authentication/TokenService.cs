namespace OnxFlow.Server.API.Authentication;

internal class TokenService(
  TimeProvider timeProvider,
  IRepository<BaseToken> tokenRepository,
  IOptions<JwtOptions> jwtOptions
) : ITokenService
{
  private readonly TimeProvider _timeProvider = timeProvider;
  private readonly IRepository<BaseToken> _tokenRepository = tokenRepository;
  private readonly JwtOptions _jwtOptions = jwtOptions.Value;

  public string GenerateAccessToken(User existingUser)
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(_jwtOptions.Secret);
    var issuedAt = _timeProvider.GetUtcNow();
    var expires = issuedAt.AddMinutes(_jwtOptions.ExpiryInMinutes);
    var claims = new List<Claim>
    {
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
      new(JwtRegisteredClaimNames.Sub, existingUser.Id),
      new(JwtRegisteredClaimNames.NameId, existingUser.Username),
      new(JwtRegisteredClaimNames.Email, existingUser.Email),
    };

    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(claims),
      Expires = expires.UtcDateTime,
      IssuedAt = issuedAt.UtcDateTime,
      Issuer = _jwtOptions.Issuer,
      Audience = _jwtOptions.Audience,
      SigningCredentials = new SigningCredentials(
        new SymmetricSecurityKey(key),
        SecurityAlgorithms.HmacSha256Signature
      )
    };

    var securityToken = tokenHandler.CreateToken(tokenDescriptor);
    var jwtToken = tokenHandler.WriteToken(securityToken);
    return jwtToken;
  }

  public async Task<Result<RefreshToken>> GenerateRefreshToken(string userId)
  {
    var expiresAt = _timeProvider
      .GetUtcNow()
      .AddHours(12)
      .UtcDateTime;

    var token = new RefreshToken
    {
      UserId = userId,
      Token = GenerateToken(),
      ExpiresAt = expiresAt
    };

    try
    {
      var createdToken = await _tokenRepository.CreateAsync(token);
      return Result.Ok(token);
    }
    catch (Exception ex)
    {
      return Result.Fail(
        new GenerateRefreshTokenError().CausedBy(ex)
      );
    }
  }

  public async Task<Result<VerificationToken>> GenerateVerificationToken(string userId)
  {
    var expiresAt = _timeProvider
      .GetUtcNow()
      .AddMinutes(15)
      .UtcDateTime;

    var token = new VerificationToken
    {
      UserId = userId,
      Token = GenerateToken(),
      ExpiresAt = expiresAt
    };

    try
    {
      var createdToken = await _tokenRepository.CreateAsync(token);
      return Result.Ok(token);
    }
    catch (Exception ex)
    {
      return Result.Fail(
        new GenerateVerificationTokenError().CausedBy(ex)
      );
    }
  }

  public Task<Result<(string AccessToken, RefreshToken RefreshToken)>> RefreshAccessTokenAsync(string userId, string refreshToken)
  {
    throw new NotImplementedException();
  }

  public Task RemoveAllInvalidRefreshTokensAsync(string userId)
  {
    throw new NotImplementedException();
  }

  public Task RemoveAllInvalidVerificationTokensAsync(string userId)
  {
    throw new NotImplementedException();
  }

  public Task RevokeRefreshTokenAsync(string userId, string refreshToken)
  {
    throw new NotImplementedException();
  }

  public Task RevokeUserVerificationTokensAsync(string userId)
  {
    throw new NotImplementedException();
  }

  public Task RevokeVerificationTokenAsync(string token)
  {
    throw new NotImplementedException();
  }

  public Task<Result<BaseToken>> VerifyVerificationTokenAsync(string token)
  {
    throw new NotImplementedException();
  }

  private static string GenerateToken()
  {
    var randomBytes = new byte[32];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomBytes);
    var token = Convert.ToBase64String(randomBytes);
    return RemoveNonAlphaNumericCharacters(token);
  }

  private static string RemoveNonAlphaNumericCharacters(string input)
  {
    var pattern = @"[^A-Za-z0-9]";
    var replacement = string.Empty;
    var output = Regex.Replace(input, pattern, replacement);
    return output;
  }
}