namespace DataFlow.Server.API.Cors;

internal static class CorsExtensions
{
  private const string PolicyName = "CORSpolicy";

  public static WebApplicationBuilder AddCORS(this WebApplicationBuilder builder)
  {
    var corsOptions = new CorsOptions();
    builder.Configuration.GetSection(nameof(CorsOptions)).Bind(corsOptions);
    builder.Services.ConfigureOptions<CorsOptionsSetup>();

    builder.Services.AddCors(
      options => options.AddPolicy(
        PolicyName,
        policy => policy
          .AllowCredentials()
          .AllowAnyHeader()
          .AllowAnyMethod()
          .WithOrigins(corsOptions.AllowedOrigins)
      )
    );

    return builder;
  }

  public static WebApplication UseCORS(this WebApplication app)
  {
    app.UseCors(PolicyName);
    return app;
  }
}