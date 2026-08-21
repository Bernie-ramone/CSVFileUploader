using CSVFileUploader.Domain.Enums;

namespace CSVFileUploader.Application.DTOs.UploadHistory
{
    public sealed record UploadHistoryRowDto(
    int RowNumber,
    string? RecordId,
    CsvUploadRowStatus Status,
    string? ErrorMessage);
}
