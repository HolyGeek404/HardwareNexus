using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserApi.Application.Services.Interfaces;
using UserApi.Domain.Entities;
using UserApi.Domain.ValueObjects;

namespace UserApi.Infrastructure.DataAccess.Context;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        HardwareNexusContext context,
        IPasswordService passwordService,
        ILogger logger)
    {
        logger.LogInformation("Checking whether admin user seed is required...");

        var adminExists = await context.User
            .AnyAsync(u => u.Email.Value == "admin@hardwarenexus.com");

        if (adminExists)
        {
            logger.LogInformation("Admin user already exists — skipping seed.");
            return;
        }

        logger.LogInformation("Admin user not found — creating seed admin account.");

        try
        {
            var adminUser = User.Create(
                Name.Create("Admin"),
                Name.Create("User"),
                Email.Create("admin@hardwarenexus.com"),
                Password.Create(passwordService.HashPassword("Wiktor1@34")));

            adminUser.Activate();

            context.User.Add(adminUser);
            await context.SaveChangesAsync();

            logger.LogInformation("Admin user seeded successfully with email {Email}.", "admin@hardwarenexus.com");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to seed admin user.");
            throw;
        }
    }
}