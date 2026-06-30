var builder = DistributedApplication.CreateBuilder(args);

var gateway = builder.AddProject<Projects.Website_Gateway>("Website-Gateway");
builder.AddProject<Projects.Website_Client>("Website-Client")
    .WaitFor(gateway)
    .WithParentRelationship(gateway);

builder.AddProject<Projects.HardwareNexus_UserApi_Presentation>("User-Api");

builder.Build().Run();