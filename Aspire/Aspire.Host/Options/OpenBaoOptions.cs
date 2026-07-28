using Microsoft.Extensions.Configuration;

namespace Aspire.Host.Options;

public sealed class OpenBaoOptions
{
    public const string SectionName = "OpenBao";

    public required string Address { get; set; }

    // Preserve the existing development-secret key while exposing a correctly named property in code.
    [ConfigurationKeyName("DevRooToken")]
    public required string DevRootToken { get; set; }

    public required AppRoleOptions User { get; set; }
    public required AppRoleOptions Product { get; set; }
    public required AppRoleOptions Cart { get; set; }
}

public sealed class AppRoleOptions
{
    public required string RoleId { get; set; }
    public required string SecretId { get; set; }
}
