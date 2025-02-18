namespace OnxFlow.Server.API.Authentication;

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