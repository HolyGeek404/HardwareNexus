using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderApi.Domain.Entities;
using OrderApi.Domain.ValueObjects;

namespace OrderApi.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public const string LinesNavigation = "OrderLines";

    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasConversion(id => id.Value, value => OrderId.Create(value))
            .ValueGeneratedNever();

        builder.Property(o => o.CustomerId)
            .HasConversion(id => id.Value, value => CustomerId.Create(value))
            .IsRequired();

        builder.Property(o => o.OrderDate).IsRequired();
        
        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.ComplexProperty(o => o.Total, money =>
        {
            money.Property(m => m.Amount).HasColumnName("TotalAmount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("Currency").HasConversion<string>().HasMaxLength(3);
        });

        builder.HasMany<OrderLine>(LinesNavigation)
            .WithOne()
            .HasForeignKey(OrderLineConfiguration.OrderIdColumn)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(LinesNavigation).AutoInclude();

        builder.Ignore(o => o.Lines);
        builder.Ignore(o => o.DomainEvents);
    }
}
