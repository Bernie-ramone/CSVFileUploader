using CSVFileUploader.Domain.Entities;
using CSVFileUploader.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CSVFileUploader.Infrastructure.Persistence
{

    public sealed class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ImportedRecord> ImportedRecords =>
            Set<ImportedRecord>();

        public DbSet<CsvUpload> CsvUploads =>
            Set<CsvUpload>();

        public DbSet<CsvUploadRow> CsvUploadRows =>
            Set<CsvUploadRow>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(
                modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ApplicationDbContext).Assembly);
        }
    }
}