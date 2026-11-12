using Microsoft.OpenApi;

namespace OrderApi.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
    {
       services.AddSwaggerGen(x =>
        {
            x.SwaggerDoc("v1", new OpenApiInfo { Title = "HardwareNexus Order Api Swagger", Version = "v1" });
        });

        return services;
    }
}