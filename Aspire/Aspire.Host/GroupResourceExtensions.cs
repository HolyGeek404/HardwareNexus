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
        
        public void AddApiSection()
        {
            var apiSection = builder.AddGroup("Api");

            builder.AddProject<Projects.UserApi_Presentation>("User-Api")
                .WithParentRelationship(apiSection.Resource);
            builder.AddProject<Projects.ProductApi_Api>("ProductApi-Api")
                .WithParentRelationship(apiSection.Resource);
            builder.AddProject<Projects.CartApi_Presentation>("Cart-Api")
                .WithParentRelationship(apiSection.Resource);
        }
    }
}
public sealed class GroupResource(string name) : Resource(name)
{
}