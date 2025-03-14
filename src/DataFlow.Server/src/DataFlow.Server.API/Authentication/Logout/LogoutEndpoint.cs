namespace DataFlow.Server.API.Authentication.Logout;

internal static class LogoutEndpoint
{
  private const string Route = "/logout";

  public static void MapLogoutEndpoint(this WebApplication app)
  {
    app
      .MapPost(Route, HandleAsync)
      .RequireAuthorization();
  }

  private static async Task<IResult> HandleAsync([AsParameters] LogoutRequest req)
  {
    var userId = req.Context.GetUserId();

    if (userId is null)
    {
      return Results.Problem(
        title: "Unable to logout user",
        detail: "No user is logged in",
        statusCode: 401
      );
    }

    var refreshToken = req.Context.Request.GetRefreshTokenCookie();

    if (string.IsNullOrWhiteSpace(refreshToken) is false)
    {
      await req.TokenService.RevokeRefreshTokenAsync(userId, refreshToken);
      await req.TokenService.RemoveAllInvalidRefreshTokensAsync(userId);
    }

    req.Context.Response.SetRefreshTokenCookie(string.Empty, DateTime.UtcNow.AddDays(-1));
    return Results.Ok();
  }
}