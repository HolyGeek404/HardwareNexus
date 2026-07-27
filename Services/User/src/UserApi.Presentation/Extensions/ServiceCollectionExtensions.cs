using System.Reflection;
using System.Text;
using Azure.Identity;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using UserApi.Application.Features;
using UserApi.Application.Features.Validators.SignUp;
using UserApi.Application.Services;
using UserApi.Application.Services.Interfaces;
using UserApi.Infrastructure;
using UserApi.Infrastructure.DataAccess;
using UserApi.Infrastructure.DataAccess.Context;

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

        public void AddDataBaseConfig(IConfigurationManager configuration)
        {
            var connectionString = ServiceCollectionExtensionsHelper.GetPostgresSqlConnStr(configuration);

            services.AddDbContext<HardwareNexusContext>(options => options.UseNpgsql(connectionString));
        }

        public void AddSwaggerConfig()
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "HardwareNexus User API",
                    Version = "v1",
                    Description =
                        "User management API for HardwareNexus. Provides signup, signin, account verification, and profile endpoints. Use the OAuth2 flow in this UI to authorize requests.",
                    TermsOfService = new Uri("https://HardwareNexus.example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "HardwareNexus API Support",
                        Email = "support@HardwareNexus.example.com",
                        Url = new Uri("https://HardwareNexus.example.com/support")
                    }
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath, true);
            });
        }
    }
}