var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Website_Client>("Website");

builder.Build().Run();