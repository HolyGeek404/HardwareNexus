using Microsoft.EntityFrameworkCore;
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