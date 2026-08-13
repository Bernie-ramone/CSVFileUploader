using CSVFileUploader.Application.DTOs;

namespace CSVFileUploader.Application.Common.Models
{
    public sealed record CsvReadResult(IReadOnlyCollection<string> Headers, IReadOnlyCollection<CsvRowDto> Rows);
}
