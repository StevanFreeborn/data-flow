namespace DataFlow.Server.API.Authentication.Logout;

internal record LogoutRequest(
  HttpContext Context,
  [FromServices] ITokenService TokenService
);
