using Microsoft.Extensions.Configuration;

namespace Aspire.Host.Options;

public sealed class KeyCloakOptions
{
    public const string SectionName = "KeyCloak";

    [ConfigurationKeyName("admin")]
    public required string Login { get; set; }

    [ConfigurationKeyName("password")]
    public required string Password { get; set; }
}
