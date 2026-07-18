using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;

namespace ProductApi.Api.Extensions;

public static class ServiceCollectionExtensionsHelper
{
    public static async Task<string> GetMongoDbConnStr(WebApplicationBuilder  builder)
    {
        var envFilePath = builder.Configuration["OPENBAO_ENV_FILE_PATH"]!;
        var lines = await File.ReadAllLinesAsync(envFilePath);

        var secretId = lines.First(l => l.StartsWith("OPENBAO_PRODUCT_SECRET_ID=")).Split('=', 2)[1];
        var openBaoAddr = builder.Configuration["OPENBAO_ADDR"] ?? throw new InvalidOperationException("OPENBAO_ADDR not set");
        var roleId = lines.First(l => l.StartsWith("OPENBAO_PRODUCT_ROLE_ID=")).Split('=', 2)[1];
        
        IAuthMethodInfo authMethod = new AppRoleAuthMethodInfo(roleId, secretId);
        var vaultClient = new VaultClient(new VaultClientSettings(openBaoAddr, authMethod));

        var secret =
            await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync("hardwarenexus/api/product",
                mountPoint: "secret");

        var mongoConnectionString = secret.Data.Data["mongodb-connstr"].ToString() ??
                                    throw new InvalidOperationException(
                                        "mongodb-connstr not found in OpenBao secret");
        
        return mongoConnectionString;
    }
}