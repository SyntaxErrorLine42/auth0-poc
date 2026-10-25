namespace _1lab.Endpoints;

using FastEndpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

sealed class LoginEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await HttpContext.ChallengeAsync(
            OpenIdConnectDefaults.AuthenticationScheme,
            new AuthenticationProperties
            {
                RedirectUri = "/"
            });

        ResponseStarted = true;
    }
}
