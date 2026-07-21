using System.Text;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Website.Gateway.Extensions;

public static class GatewayExtensions
{
    public static void AddGatewayConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddEnvironmentVariables();
    }

    public static void AddGatewayServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();

        var keyValue = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(keyValue))
            throw new InvalidOperationException("JWT signing key is not configured.");

        services
            .AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue)),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (!string.IsNullOrWhiteSpace(context.Token)) return Task.CompletedTask;
                        var token = context.Request.Cookies["access_token"];
                        if (!string.IsNullOrWhiteSpace(token)) context.Token = token;

                        return Task.CompletedTask;
                    }
                };
            });
        services.AddAuthorization();
    }

    public static void UseGatewayPipeline(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
    }
}