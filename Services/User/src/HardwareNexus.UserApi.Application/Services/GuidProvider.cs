using HardwareNexus.UserApi.Application.Services.Interfaces;

namespace HardwareNexus.UserApi.Application.Services;

public class GuidProvider : IGuidProvider
{
    public Guid Get()
    {
        var guid = Guid.NewGuid();
        return guid;
    }
}