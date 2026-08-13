
using CSVFileUploader.Application.Common.Models;

namespace CSVFileUploader.Application.CSV.UploadCsv
{
    public sealed record UploadCsvResult(
    int TotalRows,
    int InsertedRows,
    int DuplicateRows,
    IReadOnlyCollection<CsvUploadError> Errors);
}
