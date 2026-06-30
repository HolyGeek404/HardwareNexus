using HardwareNexus.UserApi.Domain.Entities;
using HardwareNexus.UserApi.Application.Models;

namespace HardwareNexus.UserApi.Application.Services.Interfaces;

public interface IUserSessionService
{
    UserSession CreateSession(User user);
    UserSession? GetUserSession(string? session = null);
    bool Validate();
    void ClearUserCachedData(string sessionId);
}