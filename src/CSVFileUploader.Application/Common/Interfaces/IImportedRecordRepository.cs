using CSVFileUploader.Domain.Entities;
using CSVFileUploader.Domain.ValueObjects;

namespace CSVFileUploader.Application.Common.Interfaces
{
    public interface IImportedRecordRepository
    {
        Task<bool> ExistsByBusinessKeyAsync(
            ImportedRecordKey businessKey,
            CancellationToken cancellationToken = default);

        Task AddRangeAsync(
            IReadOnlyCollection<ImportedRecord> records,
            CancellationToken cancellationToken = default);
    }
}
