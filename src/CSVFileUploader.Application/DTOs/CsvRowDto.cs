namespace CSVFileUploader.Application.DTOs
{
    public sealed record CsvRowDto(
    string RecordId,
    string AssetId,
    string SourceSite,
    string DestinationSite,
    DateOnly EventDate,
    decimal Volume,
    string? Unit,
    string? Notes);
}
