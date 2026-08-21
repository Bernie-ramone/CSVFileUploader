using CSVFileUploader.Application.DTOs.UploadHistory;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSVFileUploader.Application.Common.Interfaces
{
    public interface IUploadHistoryRepository
    {
        Task<IReadOnlyCollection<UploadHistoryItemDto>> GetHistoryAsync(CancellationToken cancellationToken = default);

        Task<UploadHistoryDetailDto?> GetDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
