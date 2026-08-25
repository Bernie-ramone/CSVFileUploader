namespace CSVFileUploader.Application.Common.Models
{
    public sealed record InvalidCsvRow(
    int RowNumber,
    string? RecordId,
    string ErrorMessage);
}
