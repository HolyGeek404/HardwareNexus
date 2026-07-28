using Microsoft.EntityFrameworkCore;
using UserApi.Application.Services.Interfaces;
using UserApi.Infrastructure.DataAccess.Context;
using UserApi.Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServices();
builder.Services.AddMediatrConfig();
builder.Services.AddDataBaseConfig(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfig();
builder.Services.AddMemoryCache();
builder.Logging.AddLoggingConfig();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HardwareNexusContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("DatabaseSeeder");

    logger.LogInformation("Applying pending database migrations...");
    db.Database.Migrate();
    logger.LogInformation("Migrations applied successfully.");

    if (app.Environment.IsDevelopment())
    {
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
        await DatabaseSeeder.SeedAsync(db, passwordService, logger);
    }
}
app.UseSwagger(c => { c.RouteTemplate = "swagger/{documentName}/swagger.json"; });

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "HardwareNexus User Api v1");
    c.OAuthClientId(builder.Configuration["Swagger:SwaggerClientId"]);
    c.OAuthUsePkce();
    c.OAuthScopeSeparator(" ");
});
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();