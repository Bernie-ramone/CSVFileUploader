using CSVFileUploader.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSVFileUploader.Infrastructure.Persistence.Configurations
{
    public sealed class CsvUploadRowConfiguration
        : IEntityTypeConfiguration<CsvUploadRow>
    {
        public void Configure(
            EntityTypeBuilder<CsvUploadRow> builder)
        {
            builder.ToTable("CsvUploadRows");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RowNumber)
                .IsRequired();

            builder.Property(x => x.RecordId)
                .HasMaxLength(50);

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(2000);

            builder.HasIndex(
                x => new
                {
                    x.CsvUploadId,
                    x.RowNumber
                })
                .IsUnique()
                .HasDatabaseName(
                    "UX_CsvUploadRows_Upload_RowNumber");
        }
    }
}
