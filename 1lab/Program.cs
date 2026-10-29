using _1lab.Components;
using _1lab.Persistence;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents();

builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddFastEndpoints();
// Gives us Authentication in SSR
builder.Services.AddCascadingAuthenticationState();
builder.Services
    // This first section is basically like a "pointer", a declaration on what needs to be done
    // Other 2 sections are the definitions
    .AddAuthentication(options =>
    {
        // Make app by default always look for present cookie
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        // If user isn't logged in, redirect him to OIDC client
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    // Configure cookies
    .AddCookie()
    // Configure the OIDC client
    .AddOpenIdConnect(options =>
    {
        options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}";
        options.ClientId = builder.Configuration["Auth0:ClientId"];
        options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
        // Auth0 returns a short-lived code via the browser, and the server swaps it for tokens using the client secret
        options.ResponseType = "code";
        options.CallbackPath = "/signin-oidc";
        // Without this, logout doesn't know what app he should log me out from
        options.SaveTokens = true;
    })
    // Configure the JWT token
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
    {
        o.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
        o.Audience = builder.Configuration["Auth0:Audience"];
    });

builder.Services.AddAuthorization();

// Render settings
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownIPNetworks.Clear();
    o.KnownProxies.Clear();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>();

app.UseFastEndpoints();

app.Run();
