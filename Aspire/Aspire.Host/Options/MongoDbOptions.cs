using Microsoft.Extensions.Configuration;

namespace Aspire.Host.Options;

public sealed class MongoDbOptions
{
    public const string SectionName = "Mongo";

    [ConfigurationKeyName("database")]
    public required string DatabaseName { get; set; }

    [ConfigurationKeyName("admin")]
    public required string Login { get; set; }

    [ConfigurationKeyName("secret")]
    public required string Password { get; set; }
}
