using System.Reflection;
using Microsoft.OpenApi;
using MongoDB.Driver;
using ProductApi.Api.Interfaces;
using ProductApi.Api.Repositories;
using ProductApi.Api.Repositories.Base;
using ProductApi.Api.Services;

namespace ProductApi.Api.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public static async Task AddMongoDbConfig(WebApplicationBuilder builder)
        {
            var mongoConnectionString = await ServiceCollectionExtensionsHelper.GetMongoDbConnStr(builder);
            builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
            builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase("hardwarenexus-products"));

            builder.Services.AddScoped(typeof(IReadRepository<>), typeof(MongoRepository<>));
            builder.Services.AddScoped(typeof(IWriteRepository<>), typeof(MongoRepository<>));
        }

        public void AddRepositories()
        {
            services.AddScoped<ICpuRepository, CpuRepository>();
            services.AddScoped<IGpuRepository, GpuRepository>();
            services.AddScoped<ICoolerRepository, CoolerRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        public void AddServices()
        {
            services.AddScoped<IProductService, ProductServiceUnit>();
        }

        public void AddSwaggerConfig(IConfiguration configuration)
        {
            var tenantId = configuration.GetSection("AzureAd")["TenantId"];
            var swaggerScope = configuration.GetSection("Swagger")["Scope"];
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "GoodStuff Product API",
                    Version = "v1",
                    Description =
                        "Catalog and product data for GoodStuff commerce apps. " +
                        "All endpoints require a valid bearer token (OAuth2 auth code with PKCE). " +
                        "Use the configured scope to authenticate and explore product queries."
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath, true);
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
                            TokenUrl =
                                new Uri(
                                    $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"), //token end point
                            Scopes = new Dictionary<string, string> { { $"{swaggerScope}", "Swagger - Local testing" } }
                        }
                    }
                });
                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("oauth2", document), [swaggerScope!]
                    }
                });
            });
        }
    }
}