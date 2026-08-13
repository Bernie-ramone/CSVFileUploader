namespace CSVFileUploader.Application.Common.Models
{
    public sealed record CsvUploadError(
    int RowNumber,
    string Message);
}
