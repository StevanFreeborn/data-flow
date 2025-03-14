namespace DataFlow.Server.API.Authentication;

internal static class HttpResponseExtensions
{
  internal static void SetRefreshTokenCookie(this HttpResponse response, string token, DateTimeOffset expiresAt)
  {
    response.Cookies.Append(
      "onxRefreshToken",
      token,
      new CookieOptions
      {
        HttpOnly = true,
        Expires = expiresAt,
        SameSite = SameSiteMode.None,
        Secure = true
      }
    );
  }
}

internal static class HttpRequestExtensions
{
  internal static string? GetRefreshTokenCookie(this HttpRequest request)
  {
    return request.Cookies["onxRefreshToken"];
  }
}

internal static class HttpContextExtensions
{
  internal static string? GetUserId(this HttpContext context)
  {
    return context.User.FindFirstValue(ClaimTypes.NameIdentifier);
  }
}