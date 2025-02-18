var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.ConfigureOptions<MongoDbOptionsSetup>();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IRepository<User>, MongoUserRepository>();
builder.Services.AddScoped<IRepository<BaseToken>, MongoTokenRepository>();

builder.Services.AddScoped<IEmailClient, SmtpEmailClient>();
builder.Services.AddScoped<IEmailService, DotNetEmailService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.AddCORS();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCORS();

app.MapGet("/", static () => "Hello World!");

app.Run();