using Aspire.Host;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddApiSection(builder.AddInfrastructureSection());
builder.AddWebsiteSection();

builder.Build().Run();