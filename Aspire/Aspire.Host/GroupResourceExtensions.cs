using Aspire.Host.Options;
using Keycloak.Hosting;
using Projects;

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

            var gateway = builder.AddProject<Website_Gateway>("website-gateway")
                .WithParentRelationship(websiteSection.Resource);
            
            builder.AddJavaScriptApp("website-client", "../../Website/Website.Client")
                .WithRunScript("start")
                .WithExternalHttpEndpoints()
                .WaitFor(gateway)
                .WithParentRelationship(websiteSection.Resource);
        }

        public void AddApiSection(InfrastructureResources infrastructure, InfrastructureOptions options)
        {
            var apiSection = builder.AddGroup("Api");

            builder.AddUserApi(infrastructure, apiSection,options);
            builder.AddProductApi(infrastructure, apiSection, options);
        }

        private void AddUserApi(InfrastructureResources infrastructure,
            IResourceBuilder<GroupResource> apiSection,
            InfrastructureOptions options)
        {
            builder.AddProject<UserApi_Presentation>("User-Api")
                .WithEnvironment("OPENBAO_ADDR", options.OpenBao.Address)
                .WithEnvironment("OPENBAO_USER_ROLE_ID", options.OpenBao.User.RoleId)
                .WithEnvironment("OPENBAO_USER_SECRET_ID", options.OpenBao.User.SecretId)
                .WaitFor(infrastructure.OpenBaoSeed)
                .WaitFor(infrastructure.MongoSeed)
                .WithParentRelationship(apiSection.Resource);
        }
        private void AddProductApi(
            InfrastructureResources infrastructure,
            IResourceBuilder<GroupResource> apiSection,
            InfrastructureOptions options)
        {
            builder.AddProject<ProductApi_Api>("Product-Api")
                .WithEnvironment("OPENBAO_ADDR", options.OpenBao.Address)
                .WithEnvironment("OPENBAO_PRODUCT_ROLE_ID", options.OpenBao.Product.RoleId)
                .WithEnvironment("OPENBAO_PRODUCT_SECRET_ID", options.OpenBao.Product.SecretId)
                .WaitFor(infrastructure.OpenBaoSeed)
                .WaitFor(infrastructure.MongoSeed)
                .WithParentRelationship(apiSection.Resource);
        }

        public InfrastructureResources AddInfrastructureSection(InfrastructureOptions options)
        {
            var infraSection = builder.AddGroup("Infrastructure");

            var openbao = builder.AddOpenBao(infraSection, options.OpenBao);
            var mongo = builder.AddMongoDb(infraSection, options.Mongo);
            var postgres = builder.AddPostgresSql(infraSection, options.PostgresSql);
            var keycloak = builder.AddKeyCloak(infraSection, postgres, options.KeyCloak);
            
            return new InfrastructureResources(mongo, openbao, postgres,keycloak);
        }

        private IResourceBuilder<ExecutableResource> AddMongoDb(
            IResourceBuilder<GroupResource> infraSection,
            MongoDbOptions options)
        {
            var mongoSection = builder.AddGroup("MongoDB")
                .WithParentRelationship(infraSection.Resource);

            var mongoContainer = builder.AddContainer("mongodb-container", "mongo:7")
                .WithContainerName("mongodb-dev")
                .WithBindMount(
                    "./Scripts/MongoDB/seed-mongodb.sh",
                    "/scripts/seed-mongodb.sh")
                .WithBindMount(
                    "./Scripts/MongoDB/products.json",
                    "/scripts/products.json")
                .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", options.Login)
                .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", options.Password)
                .WithParentRelationship(mongoSection.Resource)
                .WithEndpoint(
                    27017,
                    27017,
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
        private IResourceBuilder<ExecutableResource> AddOpenBao(
            IResourceBuilder<GroupResource> infraSection,
            OpenBaoOptions options)
        {
            var openbaoSection = builder.AddGroup("OpenBao")
                .WithParentRelationship(infraSection.Resource);

            var openbaoContainer = builder.AddContainer("openbao-container", "openbao/openbao")
                .WithBindMount(
                    "./Scripts/OpenBao/seed-openbao.sh",
                    "/scripts/seed-openbao.sh")
                .WithContainerName("openbao-dev")
                .WithParentRelationship(openbaoSection.Resource)
                .WithEnvironment("BAO_DEV_ROOT_TOKEN_ID", options.DevRootToken)
                .WithEndpoint(
                    8200,
                    8200,
                    name: "http");

            var openbaoSeed = builder.AddExecutable(
                    "openbao-seed",
                    "/bin/sh",
                    ".",
                    "Scripts/OpenBao/start-seed-openbao.sh")
                .WithEnvironment("OPENBAO_USER_ROLE_ID", options.User.RoleId)
                .WithEnvironment("OPENBAO_USER_SECRET_ID", options.User.SecretId)
                .WithEnvironment("OPENBAO_PRODUCT_ROLE_ID", options.Product.RoleId)
                .WithEnvironment("OPENBAO_PRODUCT_SECRET_ID", options.Product.SecretId)
                .WithEnvironment("OPENBAO_CART_ROLE_ID", options.Cart.RoleId)
                .WithEnvironment("OPENBAO_CART_SECRET_ID", options.Cart.SecretId)
                .WaitFor(openbaoContainer)
                .WithParentRelationship(openbaoSection.Resource);

            return openbaoSeed;
        }
        private IResourceBuilder<PostgresServerResource> AddPostgresSql(
            IResourceBuilder<GroupResource> infraSection,
            PostgresSqlOptions options)
        {
            var postgresSection = builder.AddGroup("Postgres")
                .WithParentRelationship(infraSection.Resource);
            
            var userName = builder.AddParameter("userName", options.Login);
            var password = builder.AddParameter("password", options.Password, secret:true);
            var postgres = builder.AddPostgres("postgres-sql-container", userName, password)
                .WithDataVolume()
                .WithHostPort(5432)
                .WithParentRelationship(postgresSection.Resource);
            postgres.AddDatabase(options.DatabaseName);
            return postgres;
        }
        private IResourceBuilder<KeycloakResource> AddKeyCloak(
            IResourceBuilder<GroupResource> infraSection,
            IResourceBuilder<PostgresServerResource> postgres,
            KeyCloakOptions options)
        {
            var keycloakSection = builder.AddGroup("Keycloak")
                .WithParentRelationship(infraSection.Resource);

            var keycloak = builder.AddKeycloak("keycloak-container", postgres)
                .WithEnvironment("KEYCLOAK_ADMIN", options.Login)
                .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", options.Password)
                .WithEndpoint(port: 8080, targetPort: 8080, name: "http")
                .WithParentRelationship(keycloakSection.Resource);

            return keycloak;
        }
    }
}

public sealed class GroupResource(string name) : Resource(name);

public record InfrastructureResources(
    IResourceBuilder<ExecutableResource> MongoSeed,
    IResourceBuilder<ExecutableResource> OpenBaoSeed,
    IResourceBuilder<PostgresServerResource> Postgres,
    IResourceBuilder<KeycloakResource> Keycloak);
