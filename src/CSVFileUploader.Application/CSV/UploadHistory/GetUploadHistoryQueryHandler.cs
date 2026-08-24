using CSVFileUploader.Application.Common.Interfaces;
using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Application.DTOs.UploadHistory;

namespace CSVFileUploader.Application.CSV.UploadHistory
{
    public sealed class GetUploadHistoryQueryHandler
    {
        private readonly IUploadHistoryRepository _repository;

        public GetUploadHistoryQueryHandler(
            IUploadHistoryRepository repository)
        {
            _repository = repository;
        }

        public Task<PagedResult<UploadHistoryItemDto>>
            HandleAsync(
                GetUploadHistoryQuery query,
                CancellationToken cancellationToken = default)
        {
            return _repository.GetHistoryAsync(
                query.PageNumber,
                query.PageSize,
                cancellationToken);
        }
    }
}
