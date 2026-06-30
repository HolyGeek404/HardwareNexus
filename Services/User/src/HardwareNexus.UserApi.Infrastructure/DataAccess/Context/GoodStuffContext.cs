using HardwareNexus.UserApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HardwareNexus.UserApi.Infrastructure.DataAccess.Context;

public class GoodStuffContext(DbContextOptions<GoodStuffContext> options) : DbContext(options)
{
    public DbSet<User> User { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}