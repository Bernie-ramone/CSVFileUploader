using CSVFileUploader.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CSVFileUploader.Infrastructure.Persistence
{
    public sealed class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ImportedRecord> ImportedRecords => Set<ImportedRecord>();

        public DbSet<CsvUpload> CsvUploads => Set<CsvUpload>();

        public DbSet<CsvUploadRow> CsvUploadRows => Set<CsvUploadRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ApplicationDbContext).Assembly);
        }
    }
}
