using Aspire.Host;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddApiSection();
builder.AddWebsiteSection();
builder.Build().Run();