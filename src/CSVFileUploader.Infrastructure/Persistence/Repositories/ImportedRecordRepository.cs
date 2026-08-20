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

        public async Task<IReadOnlyCollection<ImportedRecordKey>>
            GetExistingBusinessKeysAsync(
                IReadOnlyCollection<ImportedRecordKey> businessKeys,
                CancellationToken cancellationToken = default)
        {
            if (businessKeys.Count == 0)
            {
                return [];
            }

            var assetIds = businessKeys
                .Select(key => key.AssetId)
                .Distinct()
                .ToArray();

            var eventDates = businessKeys
                .Select(key => key.EventDate)
                .Distinct()
                .ToArray();

            var candidates = await _context.ImportedRecords
                .AsNoTracking()
                .Where(record =>
                    assetIds.Contains(record.AssetId) &&
                    eventDates.Contains(record.EventDate))
                .Select(record => new ImportedRecordKey(
                    record.AssetId,
                    record.SourceSite,
                    record.DestinationSite,
                    record.EventDate,
                    record.Volume))
                .ToListAsync(cancellationToken);

            var requestedKeys =
                businessKeys.ToHashSet();

            return candidates
                .Where(requestedKeys.Contains)
                .Distinct()
                .ToArray();
        }

        public async Task AddRangeAsync(
    IReadOnlyCollection<ImportedRecord> records,
    CancellationToken cancellationToken = default)
        {
            if (records.Count == 0)
            {
                return;
            }

            await _context.ImportedRecords.AddRangeAsync(
                records,
                cancellationToken);
        }
    }
}