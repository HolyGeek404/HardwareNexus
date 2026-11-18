using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderApi.Domain.Entities;
using OrderApi.Domain.ValueObjects;

namespace OrderApi.Infrastructure.Persistence.Configurations;

internal sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public const string OrderIdColumn = "OrderId";

    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("OrderLines");

        builder.Property<OrderId>(OrderIdColumn)
            .HasConversion(id => id.Value, value => OrderId.Create(value));

        builder.Property(l => l.Id)
            .HasColumnName("ProductId")
            .HasConversion(id => id.Value, value => ProductId.Create(value))
            .ValueGeneratedNever();

        builder.HasKey(OrderIdColumn, nameof(OrderLine.Id));

        builder.ComplexProperty(l => l.Quantity, quantity =>
            quantity.Property(q => q.Amount).HasColumnName("Quantity"));

        builder.ComplexProperty(l => l.Price, money =>
        {
            money.Property(m => m.Amount).HasColumnName("UnitPrice").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("Currency").HasConversion<string>().HasMaxLength(3);
        });

        builder.Ignore(l => l.ProductId);
        builder.Ignore(l => l.TotalPrice);
    }
}
