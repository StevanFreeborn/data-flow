var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

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