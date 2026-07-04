using System.Text.Json;
using CartApi.Application.Services;

namespace CartApi.Infrastructure.Services;

public class SerializeService : ISerializeService
{
    public string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj);
    }

    public T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json);
    }
}