using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserApi.Application.Services;
using UserApi.Application.Services.Interfaces;
using UserApi.Domain.Entities;
using UserApi.Domain.ValueObjects;
using UserApi.Infrastructure.DataAccess.Context;

namespace UserApi.Infrastructure.DataAccess;

public class UserRepository(HardwareNexusContext context, ILogger<UserRepository> logger) : IUserRepository
{
    public async Task SignUpAsync(User user)
    {
        try
        {
            Logs.LogAddingUserUserEmailToDatabase(logger, user.Email.Value);
            await context.User.AddAsync(user);
            await context.SaveChangesAsync();
            Logs.LogAddedUserUserEmailToDatabase(logger, user.Email.Value);
        }
        catch (Exception ex)
        {
            Logs.LogCouldNotAddUserUserEmailToDatabaseBecauseExMessage(logger, ex, user.Email.Value, ex.Message);
            throw;
        }
    }

    public async Task<User?> GetUserByEmailAsync(Email email)
    {
        return await context.User.AsNoTracking().FirstOrDefaultAsync(u => u.Email.Value == email.Value);
    }

    public async Task<bool> ActivateUserAsync(Email email, ActivationToken providedKey)
    {
        var user = await context.User.FirstOrDefaultAsync(u => u.Email.Value == email.Value);

        if (user == null)
            return false;

        if (user.ActivationKey != null && user.ActivationKey.Value != providedKey.Value)
            return false;

        user.Activate();
        await context.SaveChangesAsync();
        return true;
    }

    public Task RemoveUserAsync(Email email)
    {
        var user = context.User.FirstOrDefault(u => u.Email.Value == email.Value);
        if (user == null)
            throw new ArgumentException("User already removed");

        context.User.Remove(user);
        return context.SaveChangesAsync();
    }
}