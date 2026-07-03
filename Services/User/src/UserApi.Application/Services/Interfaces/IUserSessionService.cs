using UserApi.Domain.Entities;
using UserApi.Application.Models;

namespace UserApi.Application.Services.Interfaces;

public interface IUserSessionService
{
    UserSession CreateSession(User user);
    UserSession? GetUserSession(string? session = null);
    bool Validate();
    void ClearUserCachedData(string sessionId);
}