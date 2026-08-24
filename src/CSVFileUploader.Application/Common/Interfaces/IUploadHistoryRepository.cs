using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Application.DTOs.UploadHistory;

namespace CSVFileUploader.Application.Common.Interfaces
{
    public interface IUploadHistoryRepository
    {
        Task<PagedResult<UploadHistoryItemDto>> GetHistoryAsync(
                int pageNumber,
                int pageSize,
                CancellationToken cancellationToken = default);

        Task<UploadHistoryDetailDto?> GetDetailsAsync(
                Guid id,
                CancellationToken cancellationToken = default);
    }
}
