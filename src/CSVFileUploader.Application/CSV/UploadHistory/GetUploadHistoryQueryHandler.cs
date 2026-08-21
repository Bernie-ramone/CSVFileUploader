using CSVFileUploader.Application.Common.Interfaces;
using CSVFileUploader.Application.DTOs.UploadHistory;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSVFileUploader.Application.CSV.UploadHistory
{
    public sealed class GetUploadHistoryQueryHandler
    {
        private readonly IUploadHistoryRepository _repository;

        public GetUploadHistoryQueryHandler(IUploadHistoryRepository repository)
        {
            _repository = repository;
        }

        public Task<IReadOnlyCollection<UploadHistoryItemDto>> HandleAsync(GetUploadHistoryQuery query, CancellationToken cancellationToken = default)
        {
            return _repository.GetHistoryAsync(cancellationToken);
        }
    }
}
