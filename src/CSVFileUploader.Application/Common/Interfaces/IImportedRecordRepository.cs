using CSVFileUploader.Domain.Entities;
using CSVFileUploader.Domain.ValueObjects;

namespace CSVFileUploader.Application.Common.Interfaces
{
    public interface IImportedRecordRepository
    {
        Task<IReadOnlyCollection<ImportedRecordKey>>
            GetExistingBusinessKeysAsync(
                IReadOnlyCollection<ImportedRecordKey> businessKeys,
                CancellationToken cancellationToken = default);

        Task AddRangeAsync(
            IReadOnlyCollection<ImportedRecord> records,
            CancellationToken cancellationToken = default);
    }
}
