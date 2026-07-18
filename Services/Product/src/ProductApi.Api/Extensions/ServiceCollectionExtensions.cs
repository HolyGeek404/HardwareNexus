using Microsoft.OpenApi;
using System.Reflection;
using MongoDB.Driver;
using ProductApi.Api.Interfaces;
using ProductApi.Api.Repositories;
using ProductApi.Api.Services;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;

namespace ProductApi.Api.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddCosmosRepoConfig(WebApplicationBuilder builder)
        {
            services.AddScoped(typeof(IReadRepository<>), typeof(CosmosRepository<>));
            services.AddScoped(typeof(IWriteRepository<>), typeof(CosmosRepository<>));

            services.AddScoped<ICpuRepository, CpuRepository>();
            services.AddScoped<IGpuRepository, GpuRepository>();
            services.AddScoped<ICoolerRepository, CoolerRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        public async Task AddMongoDbConfig(WebApplicationBuilder builder)
        {
            var envFilePath = builder.Configuration["OPENBAO_ENV_FILE_PATH"]!;
            var lines = await File.ReadAllLinesAsync(envFilePath);
            
            var secretId = lines.First(l => l.StartsWith("OPENBAO_PRODUCT_SECRET_ID=")).Split('=', 2)[1];
            var openBaoAddr = builder.Configuration["OPENBAO_ADDR"] ?? throw new InvalidOperationException("OPENBAO_ADDR not set");
            var roleId = builder.Configuration["OPENBAO_ROLE_ID"] ?? throw new InvalidOperationException("OPENBAO_ROLE_ID not set");

            IAuthMethodInfo authMethod = new AppRoleAuthMethodInfo(roleId, secretId);
            var vaultClient = new VaultClient(new VaultClientSettings(openBaoAddr, authMethod));

            var secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: "hardwarenexus/api/product", mountPoint: "secret");

            var mongoConnectionString = secret.Data.Data["mongodb-connstr"].ToString() ?? throw new InvalidOperationException("mongodb-connstr not found in OpenBao secret");

            builder.Services.AddSingleton(sp => new MongoClient(mongoConnectionString));
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
