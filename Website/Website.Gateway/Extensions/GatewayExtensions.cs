using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Website.Gateway.Extensions;

public static class GatewayExtensions
{
    public static void AddGatewayConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddEnvironmentVariables();
    }
    
    public static void UseGatewayPipeline(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
    }

    public static void AddWebsiteClientCors(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAngularDev", policy =>
            {
                policy.WithOrigins("http://localhost:8100")
                    .AllowAnyHeader()
                    .WithMethods("GET")
                    .AllowCredentials();
            });
        });
    }

    public static void AddAuthConfig(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(o =>
            {
                o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(o =>
            {
                o.Cookie.Name = "hn-bff";
                o.Cookie.Path = "/gateway";
                o.Cookie.HttpOnly = true;
                o.Cookie.SameSite = SameSiteMode.None;
                o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                o.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/gateway"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            })
            .AddOpenIdConnect(o =>
            {
                o.Authority = builder.Configuration["Keycloak:Authority"];
                o.ClientId = builder.Configuration["Keycloak:ClientId"] ?? "gateway";
                o.ClientSecret = builder.Configuration["Keycloak:ClientSecret"];
                o.ResponseType = "code";
                o.UsePkce = true;
                o.SaveTokens = true;                 
                o.GetClaimsFromUserInfoEndpoint = true;
                o.MapInboundClaims = false;
                o.RequireHttpsMetadata = false;      
                o.Scope.Clear();
                o.Scope.Add("openid");
                o.Scope.Add("profile");
                o.Scope.Add("email");
                o.Scope.Add("offline_access");      
                o.TokenValidationParameters.NameClaimType = "preferred_username";
            });
    }
}