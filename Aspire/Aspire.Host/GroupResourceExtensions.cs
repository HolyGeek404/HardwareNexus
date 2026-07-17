namespace Aspire.Host;

public static class GroupResourceExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        private IResourceBuilder<GroupResource> AddGroup(string name)
        {
            var resource = new GroupResource(name);

            var initialSnapshot = new CustomResourceSnapshot
            {
                ResourceType = "Group",
                Properties = [],
                State = new ResourceStateSnapshot(KnownResourceStates.Running, KnownResourceStateStyles.Success),
                StartTimeStamp = DateTime.UtcNow
            };

            return builder.AddResource(resource)
                .WithInitialState(initialSnapshot)
                .ExcludeFromManifest(); 
        }

        public void AddWebsiteSection()
        {
            var websiteSection = builder.AddGroup("Website");

            var gateway = builder.AddProject<Projects.Website_Gateway>("website-gateway")
                .WithParentRelationship(websiteSection.Resource);

            builder.AddProject<Projects.Website_Client>("website-client")
                .WaitFor(gateway)
                .WithParentRelationship(websiteSection.Resource);
        }
        
        public void AddApiSection(IResourceBuilder<ExecutableResource> openbaoSeed)
        {
            var apiSection = builder.AddGroup("Api");

            // builder.AddProject<Projects.UserApi_Presentation>("User-Api")
            //     .WaitFor(openbaoSeed)
            //     .WithParentRelationship(apiSection.Resource);
            //
            builder.AddProject<Projects.ProductApi_Api>("ProductApi-Api")
                .WaitFor(openbaoSeed)
                .WithParentRelationship(apiSection.Resource);
            //
            // builder.AddProject<Projects.CartApi_Presentation>("Cart-Api")
            //     .WaitFor(openbaoSeed)
            //     .WithParentRelationship(apiSection.Resource);
            //
            // builder.AddProject<Projects.OrderApi_Presentation>("Order-Api")
            //     .WaitFor(openbaoSeed)
            //     .WithParentRelationship(apiSection.Resource);
        }

        public IResourceBuilder<ExecutableResource> AddInfrastructureSection()
        {
            var infraSection = builder.AddGroup("Infrastructure");
            var openbaoSection = builder.AddGroup("OpenBao")
                .WithParentRelationship(infraSection.Resource);
            
            var openbaoContainer = builder.AddContainer("openbao-container", "openbao/openbao")
                .WithBindMount(
                    "./Scripts/OpenBao/seed-openbao.sh",
                    "/scripts/seed-openbao.sh")
                .WithContainerName("openbao-dev")
                .WithParentRelationship(openbaoSection.Resource)
                .WithEnvironment("BAO_DEV_ROOT_TOKEN_ID", "dev-root-token")
                .WithEndpoint(
                    targetPort: 8200,
                    name: "http");

            var openbaoSeed = builder.AddExecutable(
                    "openbao-seed",
                    "/bin/sh",
                    ".",
                    "Scripts/OpenBao/start-seed-openbao.sh")
                .WaitFor(openbaoContainer)
                .WithParentRelationship(openbaoSection.Resource);

            return openbaoSeed;

        }
    }
}
public sealed class GroupResource(string name) : Resource(name)
{
}