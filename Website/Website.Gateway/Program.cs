using Website.Gateway.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddGatewayConfiguration();
builder.Services.AddGatewayServices(builder.Configuration);
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
var app = builder.Build();


if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseGatewayPipeline();
app.MapReverseProxy();
app.Run();