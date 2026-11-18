using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderApi.Infrastructure.Outbox;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Type).HasMaxLength(200);
        builder.Property(m => m.Content);
        builder.Property(m => m.Error).HasMaxLength(2000);

        builder.HasIndex(m => m.OccurredOnUtc)
            .HasFilter("[ProcessedOnUtc] IS NULL")
            .HasDatabaseName("IX_OutboxMessages_Unprocessed");
    }
}
