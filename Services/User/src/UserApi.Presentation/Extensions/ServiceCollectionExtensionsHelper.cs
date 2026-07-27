namespace UserApi.Presentation.Extensions;

public static class ServiceCollectionExtensionsHelper
{
    public static string GetPostgresSqlConnStr(IConfiguration configuration)
    {
        var userName = configuration["PostgresSql:admin"];
        var password = configuration["PostgresSql:password"];
        var database = configuration["PostgresSql:database"];

        if (string.IsNullOrEmpty(userName)  || string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException("Couldn't create PostgresSql connection string");
        }

        return $"Host=localhost;Port=5432;Database={database};Username={userName};Password={password};";
    }
}