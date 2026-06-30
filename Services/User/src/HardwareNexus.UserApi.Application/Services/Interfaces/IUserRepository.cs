using HardwareNexus.UserApi.Domain.Entities;
using HardwareNexus.UserApi.Domain.ValueObjects;

namespace HardwareNexus.UserApi.Application.Services.Interfaces;

public interface IUserRepository
{
    Task SignUpAsync(User user);
    Task<User?> GetUserByEmailAsync(Email email);
    Task<bool> ActivateUserAsync(Email email, ActivationToken providedKey);
    Task RemoveUserAsync(Email user);
}