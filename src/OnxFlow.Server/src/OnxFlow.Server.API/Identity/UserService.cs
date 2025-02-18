namespace OnxFlow.Server.API.Identity;

internal class UserService(
  IRepository<User> userRepository,
  IEncryptionService encryptionService,
  ILogger<UserService> logger
) : IUserService
{
  private readonly IRepository<User> _userRepository = userRepository;
  private readonly IEncryptionService _encryptionService = encryptionService;
  private readonly ILogger<UserService> _logger = logger;

  public Task<Result<User>> GetUserByEmailAsync(string userEmail)
  {
    throw new NotImplementedException();
  }

  public Task<Result<User>> GetUserByIdAsync(string userId)
  {
    throw new NotImplementedException();
  }

  public Task<Result<(string AccessToken, RefreshToken RefreshToken)>> LoginUserAsync(string email, string password)
  {
    throw new NotImplementedException();
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

  public Task<Result> VerifyUserAsync(string userId)
  {
    throw new NotImplementedException();
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