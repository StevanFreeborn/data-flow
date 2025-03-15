namespace DataFlow.Server.API.Authentication;

internal class TokenService(
  TimeProvider timeProvider,
  IRepository<BaseToken> tokenRepository,
  IOptions<JwtOptions> jwtOptions,
  ILogger<TokenService> logger
) : ITokenService
{
  private readonly TimeProvider _timeProvider = timeProvider;
  private readonly IRepository<BaseToken> _tokenRepository = tokenRepository;
  private readonly JwtOptions _jwtOptions = jwtOptions.Value;
  private readonly ILogger<TokenService> _logger = logger;

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
      return Result.Fail(new GenerateRefreshTokenError().CausedBy(ex));
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
      return Result.Fail(new GenerateVerificationTokenError().CausedBy(ex));
    }
  }

  public Task<Result<(string AccessToken, RefreshToken RefreshToken)>> RefreshAccessTokenAsync(string userId, string refreshToken)
  {
  }

  public async Task RemoveAllInvalidRefreshTokensAsync(string userId)
  {
    var filter = FilterSpecification<BaseToken>.From(
      t => t.UserId == userId &&
        t.TokenType == TokenTypes.Refresh &&
        (t.Revoked || t.ExpiresAt < _timeProvider.GetUtcNow().DateTime)
    );

    await _tokenRepository.DeleteManyAsync(filter);
  }

  public async Task RemoveAllInvalidVerificationTokensAsync(string userId)
  {
    var filter = FilterSpecification<BaseToken>.From(
      t => t.UserId == userId &&
        t.TokenType == TokenTypes.Verification &&
        (t.Revoked || t.ExpiresAt < _timeProvider.GetUtcNow().DateTime)
    );

    await _tokenRepository.DeleteManyAsync(filter);
  }

  public async Task RevokeRefreshTokenAsync(string userId, string refreshToken)
  {
    var filter = FilterSpecification<BaseToken>.From(
      t => t.Token == refreshToken &&
        t.TokenType == TokenTypes.Refresh &&
        t.UserId == userId
    );
    var token = await _tokenRepository.GetAsync(filter);

    if (token is null)
    {
      _logger.LogWarning(
        "Refresh token {RefreshToken} does not exist",
        refreshToken
      );
      return;
    }

    var updatedToken = new RefreshToken(token)
    {
      Revoked = true,
    };

    await _tokenRepository.UpdateAsync(filter, updatedToken);
  }

  public async Task RevokeUserVerificationTokensAsync(string userId)
  {
    var filter = FilterSpecification<BaseToken>.From(t => t.UserId == userId && t.TokenType == TokenTypes.Verification);
    var existingTokens = await _tokenRepository.GetAsync(filter);
  }

  public async Task RevokeVerificationTokenAsync(string token)
  {
    var filter = FilterSpecification<BaseToken>.From(t => t.Token == token);
    var existingToken = await _tokenRepository.GetAsync(filter);

    if (existingToken is null)
    {
      return;
    }

    var updatedToken = new VerificationToken(existingToken)
    {
      Revoked = true
    };

    await _tokenRepository.UpdateAsync(filter, updatedToken);
  }

  public async Task<Result<BaseToken>> VerifyVerificationTokenAsync(string token)
  {
    var filter = FilterSpecification<BaseToken>.From(t => t.Token == token && t.TokenType == TokenTypes.Verification);
    var verificationToken = await _tokenRepository.GetAsync(filter);

    if (verificationToken is null)
    {
      return Result.Fail(new TokenDoesNotExistError(token));
    }

    if (verificationToken.Revoked)
    {
      return Result.Fail(new InvalidTokenError(token));
    }

    if (verificationToken.ExpiresAt < _timeProvider.GetUtcNow().UtcDateTime)
    {
      return Result.Fail(new ExpiredTokenError(token));
    }

    return Result.Ok(verificationToken);
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