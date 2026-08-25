namespace CSVFileUploader.Application.CSV.UploadCsv
{
    public sealed record UploadCsvCommand(
    Stream FileStream,
    string FileName,
    string? ContentType,
    long FileSize,
    string? FileHash = null);
}