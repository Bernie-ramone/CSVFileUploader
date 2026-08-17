using CSVFileUploader.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSVFileUploader.Infrastructure.Persistence.Configurations
{
    public sealed class ImportedRecordConfiguration
    : IEntityTypeConfiguration<ImportedRecord>
    {
        public void Configure(
            EntityTypeBuilder<ImportedRecord> builder)
        {
            builder.ToTable("ImportedRecords");

            builder.HasKey(x => x.RecordId);

            builder.Property(x => x.RecordId)
                .HasMaxLength(50)
                .IsRequired()
                .ValueGeneratedNever();

            builder.Property(x => x.AssetId)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.SourceSite)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.DestinationSite)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.EventDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.Volume)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.Unit)
                .HasMaxLength(20);

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Ignore(x => x.BusinessKey);

            builder.HasIndex(
                x => new
                {
                    x.AssetId,
                    x.SourceSite,
                    x.DestinationSite,
                    x.EventDate,
                    x.Volume
                })
                .IsUnique()
                .HasDatabaseName("UX_ImportedRecords_BusinessKey");
        }
    }
}
