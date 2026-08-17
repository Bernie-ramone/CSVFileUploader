namespace CSVFileUploader.Application.DTOs
{
    public sealed record CsvRowDto(
    int RowNumber,
    string RecordId,
    string AssetId,
    string SourceSite,
    string DestinationSite,
    string EventDate,
    string Volume,
    string? Unit,
    string? Notes);
}