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
        
        public void AddApiSection(InfrastructureResources  infrastructure)
        {
            var apiSection = builder.AddGroup("Api");

            // builder.AddProject<Projects.UserApi_Presentation>("User-Api")
            //     .WaitFor(openbaoSeed)
            //     .WithParentRelationship(apiSection.Resource);
            //
            builder.AddProject<Projects.ProductApi_Api>("ProductApi-Api")
                .WaitFor(infrastructure.OpenBaoSeed)
                .WaitFor(infrastructure.MongoSeed)
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

        public InfrastructureResources AddInfrastructureSection()
        {
            var infraSection = builder.AddGroup("Infrastructure");

            var openbao = builder.AddOpenBaoSection(infraSection);
            var mongo = builder.AddMongoDbSection(infraSection);

            return new InfrastructureResources(mongo, openbao);
        }
        
        private IResourceBuilder<ExecutableResource> AddMongoDbSection(IResourceBuilder<GroupResource> infraSection)
        {
            var mongoSection = builder.AddGroup("MongoDB")
                .WithParentRelationship(infraSection.Resource);

            var mongoContainer = builder.AddContainer("mongodb-container", "mongo")
                .WithContainerName("mongodb-dev")
                .WithBindMount(
                    "./Scripts/MongoDB/seed-mongodb.sh",
                    "/scripts/seed-mongodb.sh")
                .WithBindMount(
                    "./Scripts/MongoDB/products.json",
                    "/scripts/products.json")
                .WithParentRelationship(mongoSection.Resource)
                .WithEndpoint(
                    port: 27017,
                    targetPort: 27017,
                    name: "mongodb");

            var mongoSeed = builder.AddExecutable(
                    "mongodb-seed",
                    "/bin/sh",
                    ".",
                    "Scripts/MongoDB/start-seed-mongodb.sh")
                .WaitFor(mongoContainer)
                .WithParentRelationship(mongoSection.Resource);

            return mongoSeed;
        }
        private IResourceBuilder<ExecutableResource> AddOpenBaoSection(IResourceBuilder<GroupResource> infraSection)
        {
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
public record InfrastructureResources(
    IResourceBuilder<ExecutableResource> MongoSeed,
    IResourceBuilder<ExecutableResource> OpenBaoSeed);
