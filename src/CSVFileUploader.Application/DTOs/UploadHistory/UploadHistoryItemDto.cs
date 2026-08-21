using CSVFileUploader.Domain.Enums;

namespace CSVFileUploader.Application.DTOs.UploadHistory
{
    public sealed record UploadHistoryItemDto(
    Guid Id,
    string FileName,
    DateTimeOffset UploadedAtUtc,
    int TotalRows,
    int InsertedRows,
    int DuplicateRows,
    int ErrorRows,
    CsvUploadStatus Status);
}
