using Microsoft.Extensions.Configuration;

namespace Aspire.Host.Options;

public sealed class OpenBaoOptions
{
    public const string SectionName = "OpenBao";

    public required string Address { get; set; }

    // Preserve the existing development-secret key while exposing a correctly named property in code.
    [ConfigurationKeyName("DevRooToken")]
    public required string DevRootToken { get; set; }
}
