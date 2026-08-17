using CSVFileUploader.Application.Common.Interfaces;
using CSVFileUploader.Domain.Entities;
using CSVFileUploader.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace CSVFileUploader.Infrastructure.Persistence.Repositories
{
    public sealed class ImportedRecordRepository
    : IImportedRecordRepository
    {
        private readonly ApplicationDbContext _context;

        public ImportedRecordRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<bool> ExistsByBusinessKeyAsync(
            ImportedRecordKey businessKey,
            CancellationToken cancellationToken = default)
        {
            return _context.ImportedRecords
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.AssetId == businessKey.AssetId &&
                        x.SourceSite == businessKey.SourceSite &&
                        x.DestinationSite == businessKey.DestinationSite &&
                        x.EventDate == businessKey.EventDate &&
                        x.Volume == businessKey.Volume,
                    cancellationToken);
        }

        public async Task AddRangeAsync(
            IReadOnlyCollection<ImportedRecord> records,
            CancellationToken cancellationToken = default)
        {
            await _context.ImportedRecords.AddRangeAsync(
                records,
                cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
