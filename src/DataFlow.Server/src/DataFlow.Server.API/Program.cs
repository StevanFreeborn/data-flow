var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.ConfigureOptions<MongoDbOptionsSetup>();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IRepository<User>, MongoUserRepository>();
builder.Services.AddScoped<IRepository<BaseToken>, MongoTokenRepository>();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.ConfigureOptions<SmtpOptionsSetup>();
builder.Services.AddScoped<IEmailClient, SmtpEmailClient>();
builder.Services.AddScoped<IEmailService, DotNetEmailService>();

builder.Services.ConfigureOptions<EncryptionOptionsSetup>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();

builder.Services.ConfigureOptions<JwtOptionsSetup>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IValidator<RegisterDto>, RegisterDtoValidator>();
builder.Services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();
builder.Services.AddScoped<IValidator<VerifyAccountDto>, VerifyAccountDtoValidator>();

var jwtOptions = new JwtOptions();
builder.Configuration.GetSection(nameof(JwtOptions)).Bind(jwtOptions);

builder.Services.ConfigureOptions<JwtOptionsSetup>();

builder.Services
  .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
  {
    ValidIssuer = jwtOptions.Issuer,
    ValidAudience = jwtOptions.Audience,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ClockSkew = TimeSpan.FromSeconds(0),
  });

builder.Services.AddAuthorization();
builder.AddCORS();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseCORS();

app.MapRegisterEndpoint();
app.MapLoginEndpoint();
app.MapVerifyAccountEndpoint();
app.MapLogoutEndpoint();
app.MapGet("/", static () => "Hello World!");

app.UseAuthentication();
app.UseAuthorization();

app.Run();

public partial class Program { }