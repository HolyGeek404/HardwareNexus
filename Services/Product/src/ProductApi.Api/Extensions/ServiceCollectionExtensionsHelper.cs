using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;

namespace ProductApi.Api.Extensions;

public static class ServiceCollectionExtensionsHelper
{
    public static async Task<string> GetMongoDbConnStr(WebApplicationBuilder  builder)
    {
        var maxAttempts = 5;
        var secretId = GetRequiredConfiguration(builder, "OPENBAO_PRODUCT_SECRET_ID");
        var openBaoAddr = GetRequiredConfiguration(builder, "OPENBAO_ADDR");
        var roleId = GetRequiredConfiguration(builder, "OPENBAO_PRODUCT_ROLE_ID");
        
        IAuthMethodInfo authMethod = new AppRoleAuthMethodInfo(roleId, secretId);
        var vaultClient = new VaultClient(new VaultClientSettings(openBaoAddr, authMethod));

        do
        {
            try
            {
                var secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync("hardwarenexus/api/product", mountPoint: "secret");
                var mongoConnectionString = secret.Data.Data["mongodb-connstr"].ToString() ??
                                            throw new InvalidOperationException("mongodb-connstr not found in OpenBao secret");
                return mongoConnectionString;
            }
            catch
            {
                if (maxAttempts == 0)
                {
                    throw;
                }
                maxAttempts--;
                Console.WriteLine($"Failed to get OpenBao secret. Attempts left: {maxAttempts}");
                await Task.Delay(1000);
            }
        } while (true);
    }

    private static string GetRequiredConfiguration(WebApplicationBuilder builder, string key)
        => builder.Configuration[key]
           ?? throw new InvalidOperationException($"Required configuration value '{key}' is not set.");
}
