using HardwareNexus.UserApi.Infrastructure.DataAccess.Context;
using HardwareNexus.UserApi.Presentation.Extensions;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddServices();
        builder.Services.AddMediatrConfig();
        builder.Services.AddAzureConfig(builder.Configuration);
        builder.Services.AddDataBaseConfig(builder.Configuration);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerConfig(builder.Configuration);
        builder.Services.AddMemoryCache();
        builder.Logging.AddLoggingConfig();
        builder.Services.AddHttpClient();
        
        var app = builder.Build();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GoodStuffContext>();
            db.Database.Migrate();
        }

        app.UseSwagger(c =>
        {
            c.RouteTemplate = "swagger/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("v1/swagger.json", "GoodStuff User Api v1");
            c.OAuthClientId(builder.Configuration["Swagger:SwaggerClientId"]);
            c.OAuthUsePkce();
            c.OAuthScopeSeparator(" ");
        });
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();

