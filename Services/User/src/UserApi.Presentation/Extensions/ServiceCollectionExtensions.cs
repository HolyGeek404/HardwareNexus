using Azure.Identity;
using FluentValidation;
using FluentValidation.AspNetCore;
using UserApi.Infrastructure;
using UserApi.Infrastructure.DataAccess;
using UserApi.Infrastructure.DataAccess.Context;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Reflection;
using System.Text;
using UserApi.Application.Features;
using UserApi.Application.Features.Validators.SignUp;
using UserApi.Application.Services;
using UserApi.Application.Services.Interfaces;

namespace UserApi.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddServices()
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IGuidProvider, GuidProvider>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IUserSessionService, UserSessionService>();
            services.AddScoped<IEmailNotificationFunctionClient, EmailNotificationFunctionClient>();
        }

        public void AddMediatrConfig()
        {
            services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(SignUpCommandValidator).Assembly));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddValidatorsFromAssemblyContaining<SignUpCommandValidator>();
            services.AddFluentValidationAutoValidation();
        }

        public void AddAzureConfig(IConfigurationManager configuration)
        {
            var azureAd = configuration.GetSection("AzureAd");
            configuration.AddAzureKeyVault(new Uri(azureAd["KvUrl"]!), new DefaultAzureCredential());
            
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "CookieJwt";
                    options.DefaultChallengeScheme = "CookieJwt";
                })
                .AddJwtBearer("CookieJwt", options =>
                {
                    var keyValue = configuration["Jwt:Key"];
                    if (string.IsNullOrWhiteSpace(keyValue))
                        throw new InvalidOperationException("JWT signing key is not configured.");

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue)),
                        ValidateIssuer = true,
                        ValidIssuer = configuration["Jwt:Issuer"] ?? "goodstuff-user-api",
                        ValidateAudience = true,
                        ValidAudience = configuration["Jwt:Audience"] ?? "goodstuff",
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var token = context.Request.Cookies["access_token"];
                            if (!string.IsNullOrWhiteSpace(token))
                                context.Token = token;

                            return Task.CompletedTask;
                        }
                    };
                })
                .AddMicrosoftIdentityWebApi(azureAd, jwtBearerScheme: "AzureAd");
        }

        public void AddDataBaseConfig(IConfigurationManager configuration)
        {
            var connectionString = Environment.GetEnvironmentVariable("USER_API_CONNECTION_STRING");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("USER_API_CONNECTION_STRING is not configured.");

            services.AddDbContext<GoodStuffContext>(options =>
                options.UseSqlServer(connectionString));
        }

        public void AddSwaggerConfig(IConfiguration configuration)
        {
            var tenantId = configuration.GetSection("AzureAd")["TenantId"];
            var swaggerScope = configuration.GetSection("Swagger")["Scope"];

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "GoodStuff User API",
                    Version = "v1",
                    Description = "User management API for GoodStuff. Provides signup, signin, account verification, and profile endpoints. Use the OAuth2 flow in this UI to authorize requests.",
                    TermsOfService = new Uri("https://goodstuff.example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "GoodStuff API Support",
                        Email = "support@goodstuff.example.com",
                        Url = new Uri("https://goodstuff.example.com/support")
                    }
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "OAuth2.0 Auth Code with PKCE",
                    Name = "oauth2",
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl =
                                new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize"),
                            TokenUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                { $"{swaggerScope}", "Swagger - Local testing" }
                            }
                        }
                    }
                });
            c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("oauth2", document, null),
                    [swaggerScope]
                }
            });
        });
    }
}
}
