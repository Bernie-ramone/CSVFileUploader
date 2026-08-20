using CSVFileUploader.Domain.Entities;

namespace CSVFileUploader.Application.Common.Interfaces
{
    public interface IUploadRepository
    {
        Task AddAsync(
            CsvUpload upload,
            CancellationToken cancellationToken = default);

        Task<CsvUpload?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
