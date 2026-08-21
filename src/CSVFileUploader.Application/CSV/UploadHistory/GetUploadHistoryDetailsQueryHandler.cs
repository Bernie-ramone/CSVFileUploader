using CSVFileUploader.Application.Common.Interfaces;
using CSVFileUploader.Application.DTOs.UploadHistory;

namespace CSVFileUploader.Application.CSV.UploadHistory
{
    public sealed class GetUploadHistoryDetailsQueryHandler
    {
        private readonly IUploadHistoryRepository _repository;

        public GetUploadHistoryDetailsQueryHandler(IUploadHistoryRepository repository)
        {
            _repository = repository;
        }

        public Task<UploadHistoryDetailDto?> HandleAsync(GetUploadHistoryDetailsQuery query, CancellationToken cancellationToken = default)
        {
            return _repository.GetDetailsAsync(query.UploadId, cancellationToken);
        }
    }
}
