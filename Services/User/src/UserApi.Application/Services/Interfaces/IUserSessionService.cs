using UserApi.Application.Models;
using UserApi.Domain.Entities;

namespace UserApi.Application.Services.Interfaces;

public interface IUserSessionService
{
    UserSession CreateSession(User user);
    UserSession? GetUserSession(string? session = null);
    bool Validate();
    void ClearUserCachedData(string sessionId);
}