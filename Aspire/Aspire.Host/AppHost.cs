using Aspire.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    var secretsPath = Path.Combine(builder.AppHostDirectory, "Properties","dev.secrets.json");
    builder.Configuration.AddJsonFile(secretsPath, optional: true, reloadOnChange: true);
}
builder.AddApiSection(builder.AddInfrastructureSection());
builder.AddWebsiteSection();

builder.Build().Run();