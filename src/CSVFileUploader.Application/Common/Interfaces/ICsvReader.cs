using CSVFileUploader.Application.Common.Models;

namespace CSVFileUploader.Application.Common.Interfaces
{
    public interface ICsvReader
    {
        Task<CsvReadResult> ReadAsync(Stream stream, CancellationToken cancellationToken = default);
    }
}