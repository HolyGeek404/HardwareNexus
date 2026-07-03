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
            var website = builder.AddGroup("Website");

            var gateway = builder.AddProject<Projects.Website_Gateway>("website-gateway")
                .WithParentRelationship(website.Resource);

            builder.AddProject<Projects.Website_Client>("website-client")
                .WaitFor(gateway)
                .WithParentRelationship(website.Resource);
        }
        
        public void AddApiSection()
        {
            var website = builder.AddGroup("Api");

            builder.AddProject<Projects.UserApi_Presentation>("User-Api")
                .WithParentRelationship(website.Resource);
        }
    }
}
public sealed class GroupResource(string name) : Resource(name)
{
}