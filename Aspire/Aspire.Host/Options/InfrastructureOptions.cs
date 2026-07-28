using Microsoft.Extensions.Configuration;

namespace Aspire.Host.Options;

public sealed class InfrastructureOptions
{
    public required MongoDbOptions Mongo { get; init; }
    public required OpenBaoOptions OpenBao { get; init; }
    public required PostgresSqlOptions PostgresSql { get; init; }
    public required KeyCloakOptions KeyCloak { get; init; }

    public static InfrastructureOptions Load(IConfiguration configuration)
    {
        var options = new InfrastructureOptions
        {
            Mongo = Bind<MongoDbOptions>(configuration, MongoDbOptions.SectionName),
            OpenBao = Bind<OpenBaoOptions>(configuration, OpenBaoOptions.SectionName),
            PostgresSql = Bind<PostgresSqlOptions>(configuration, PostgresSqlOptions.SectionName),
            KeyCloak = Bind<KeyCloakOptions>(configuration, KeyCloakOptions.SectionName)
        };

        Validate(options);
        return options;
    }

    private static T Bind<T>(IConfiguration configuration, string sectionName) where T : class
        => configuration.GetRequiredSection(sectionName).Get<T>()
            ?? throw new InvalidOperationException($"Configuration section '{sectionName}' could not be bound.");

    private static void Validate(InfrastructureOptions options)
    {
        var missing = new List<string>();

        Require(options.Mongo.DatabaseName, "Mongo:database", missing);
        Require(options.Mongo.Login, "Mongo:admin", missing);
        Require(options.Mongo.Password, "Mongo:secret", missing);
        Require(options.OpenBao.Address, "OpenBao:Address", missing);
        Require(options.OpenBao.DevRootToken, "OpenBao:DevRooToken", missing);
        Require(options.PostgresSql.DatabaseName, "PostgresSql:database", missing);
        Require(options.PostgresSql.Login, "PostgresSql:admin", missing);
        Require(options.PostgresSql.Password, "PostgresSql:password", missing);
        Require(options.KeyCloak.Login, "KeyCloak:admin", missing);
        Require(options.KeyCloak.Password, "KeyCloak:password", missing);

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Missing required infrastructure configuration values: {string.Join(", ", missing)}.");
        }
    }

    private static void Require(string? value, string configurationKey, ICollection<string> missing)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            missing.Add(configurationKey);
        }
    }
}
