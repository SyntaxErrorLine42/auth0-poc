namespace _1lab.Endpoints;

using Microsoft.AspNetCore.Authentication;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

sealed class LogoutEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/logout");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Logout user + clear cookie
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
            new AuthenticationProperties { RedirectUri = "/" });
        ResponseStarted = true;
    }
}
