using Microsoft.Extensions.Configuration;

namespace Aspire.Host.Options;

public sealed class PostgresSqlOptions
{
    public const string SectionName = "PostgresSql";

    [ConfigurationKeyName("database")]
    public required string DatabaseName { get; set; }

    [ConfigurationKeyName("admin")]
    public required string Login { get; set; }

    [ConfigurationKeyName("password")]
    public required string Password { get; set; }
}
