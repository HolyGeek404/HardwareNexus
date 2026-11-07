using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Website.Gateway.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddGatewayConfiguration();
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.AddWebsiteClientCors();
builder.AddAuthConfig();

var app = builder.Build();
app.UseCors("AllowAngularDev");

if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseGatewayPipeline();

app.MapGet("/gateway/user/login", (HttpContext context) =>
{
    context.Response.Cookies.Delete("__Host-bff", new CookieOptions
    {
        Path = "/", Secure = true, HttpOnly = true, SameSite = SameSiteMode.Lax
    });
    return Results.Challenge(new AuthenticationProperties
    {
        RedirectUri = "http://localhost:8100/user/dashboard"
    }, [OpenIdConnectDefaults.AuthenticationScheme]);
});

app.MapGet("/gateway/user/me", (ClaimsPrincipal user) => Results.Ok(new
{
    name = user.FindFirstValue("preferred_username"),
    email = user.FindFirstValue("email")
})).RequireAuthorization();

app.MapPost("/gateway/user/signout", () =>
        Results.SignOut(new AuthenticationProperties { RedirectUri = "/" },
        [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]))
    .RequireAuthorization();

app.MapGet("/gateway/user/signout", () =>
        Results.SignOut(new AuthenticationProperties { RedirectUri = "http://localhost:8100/" },
        [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]));

app.MapReverseProxy();

app.Run();
