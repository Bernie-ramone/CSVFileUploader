using CSVFileUploader.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSVFileUploader.Infrastructure.Persistence.Configurations
{
    public sealed class CsvUploadConfiguration
      : IEntityTypeConfiguration<CsvUpload>
    {
        public void Configure(
            EntityTypeBuilder<CsvUpload> builder)
        {
            builder.ToTable("CsvUploads");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FileName)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.FileHash)
                .HasMaxLength(64);

            builder.HasIndex(x => x.FileHash)
                .HasDatabaseName(
                    "IX_CsvUploads_FileHash");

            builder.Property(x => x.UploadedAtUtc)
                .HasColumnType("datetimeoffset")
                .IsRequired();

            builder.Property(x => x.TotalRows)
                .IsRequired();

            builder.Property(x => x.InsertedRows)
                .IsRequired();

            builder.Property(x => x.DuplicateRows)
                .IsRequired();

            builder.Property(x => x.ErrorRows)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.HasMany(x => x.Rows)
                .WithOne()
                .HasForeignKey(x => x.CsvUploadId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
