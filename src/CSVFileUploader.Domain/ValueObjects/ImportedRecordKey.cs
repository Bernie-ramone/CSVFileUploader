namespace CSVFileUploader.Domain.ValueObjects
{
    public sealed record ImportedRecordKey(
     string AssetId,
     string SourceSite,
     string DestinationSite,
     DateOnly EventDate,
     decimal Volume);
}
