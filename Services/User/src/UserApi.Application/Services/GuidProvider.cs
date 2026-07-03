using UserApi.Application.Services.Interfaces;

namespace UserApi.Application.Services;

public class GuidProvider : IGuidProvider
{
    public Guid Get()
    {
        var guid = Guid.NewGuid();
        return guid;
    }
}