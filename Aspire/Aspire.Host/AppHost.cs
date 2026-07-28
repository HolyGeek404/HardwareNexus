using Aspire.Host;
using Aspire.Host.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    var secretsPath = Path.Combine(builder.AppHostDirectory, "Properties","dev.secrets.json");
    builder.Configuration.AddJsonFile(secretsPath, optional: true, reloadOnChange: true);
}

var infrastructureOptions = InfrastructureOptions.Load(builder.Configuration);
builder.AddApiSection(builder.AddInfrastructureSection(infrastructureOptions), infrastructureOptions);
builder.AddWebsiteSection();

builder.Build().Run();
