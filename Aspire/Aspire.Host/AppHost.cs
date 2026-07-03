using Aspire.Host;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddWebsiteSection();
builder.AddApiSection();

builder.Build().Run();