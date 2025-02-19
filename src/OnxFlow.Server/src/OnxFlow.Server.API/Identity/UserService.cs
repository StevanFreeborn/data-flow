namespace OnxFlow.Server.API.Identity;

internal class UserService(
  IRepository<User> userRepository,
  IEncryptionService encryptionService,
  ILogger<UserService> logger,
  ITokenService tokenService
) : IUserService
{
  private readonly IRepository<User> _userRepository = userRepository;
  private readonly IEncryptionService _encryptionService = encryptionService;
  private readonly ILogger<UserService> _logger = logger;
  private readonly ITokenService _tokenService = tokenService;

  public Task<Result<User>> GetUserByEmailAsync(string userEmail)
  {
    throw new NotImplementedException();
  }

  public Task<Result<User>> GetUserByIdAsync(string userId)
  {
    throw new NotImplementedException();
  }

  public async Task<Result<(string AccessToken, RefreshToken RefreshToken)>> LoginUserAsync(string email, string password)
  {
    var existingUser = await _userRepository.GetAsync(FilterSpecification<User>.From(u => u.Email == email));

    if (existingUser is null)
    {
      return Result.Fail(new InvalidLoginError());
    }

    var passwordValid = BCrypt.Net.BCrypt.Verify(password, existingUser.Password);

    if (passwordValid is false)
    {
      return Result.Fail(new InvalidLoginError());
    }

    if (existingUser.IsVerified is false)
    {
      return Result.Fail(new UserNotVerifiedError(existingUser.Id));
    }

    var accessToken = _tokenService.GenerateAccessToken(existingUser);
    var refreshTokenResult = await _tokenService.GenerateRefreshToken(existingUser.Id);

    if (refreshTokenResult.IsFailed)
    {
      _logger.LogError("Failed to generate refresh token: {Errors}", refreshTokenResult.Errors);
      return Result.Fail(new LoginFailedError());
    }

    return Result.Ok((accessToken, refreshTokenResult.Value));
  }

  public async Task<Result<string>> RegisterUserAsync(User user)
  {
    var existingUser = await _userRepository.GetAsync(FilterSpecification<User>.From(u => u.Email == user.Email));

    if (existingUser is not null)
    {
      return Result.Fail(new UserAlreadyExistError(user.Email));
    }

    var username = await GenerateUniqueUsernameAsync(user.Email);
    var passwordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);

    user.Username = username;
    user.Password = passwordHash;

    var encryptionKey = _encryptionService.GenerateKey();
    var encryptedKey = await _encryptionService.EncryptAsync(encryptionKey);
    user.EncryptionKey = encryptedKey;

    var createdUser = await _userRepository.CreateAsync(user);

    return Result.Ok(createdUser.Id);
  }

  public async Task<Result> VerifyUserAsync(string userId)
  {
    var filter = FilterSpecification<User>.From(u => u.Id == userId);
    var existingUser = await _userRepository.GetAsync(filter);

    if (existingUser is null)
    {
      return Result.Fail(new UserDoesNotExistError(userId));
    }

    if (existingUser.IsVerified)
    {
      return Result.Fail(new UserAlreadyVerifiedError(userId));
    }

    existingUser.IsVerified = true;

    await _userRepository.UpdateAsync(filter, existingUser);

    return Result.Ok();
  }

  private async Task<string> GenerateUniqueUsernameAsync(string email)
  {
    var randomNumber = RandomNumberGenerator.GetInt32(0, 1000);
    var username = $"{email.Split('@')[0]}{randomNumber}";
    var existingUser = await _userRepository.GetAsync(FilterSpecification<User>.From(u => u.Username == username));

    if (existingUser is not null)
    {
      _logger.LogInformation("Username {Username} already exists. Generating another.", username);
      return await GenerateUniqueUsernameAsync(email);
    }

    return username;
  }
}