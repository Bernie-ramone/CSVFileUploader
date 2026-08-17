namespace CSVFileUploader.Application.Common.Models
{
    public static class CsvFileDefinition
    {
        public const string RecordId = "RecordId";
        public const string AssetId = "AssetId";
        public const string SourceSite = "SourceSite";
        public const string DestinationSite = "DestinationSite";
        public const string EventDate = "EventDate";
        public const string Volume = "Volume";
        public const string Unit = "Unit";
        public const string Notes = "Notes";

        public static readonly IReadOnlyList<string> OrderedHeaders =
        [
            RecordId,
        AssetId,
        SourceSite,
        DestinationSite,
        EventDate,
        Volume,
        Unit,
        Notes
        ];

        public static readonly IReadOnlySet<string> RequiredHeaders =
            new HashSet<string>(
                [
                    RecordId,
                AssetId,
                SourceSite,
                DestinationSite,
                EventDate,
                Volume
                ],
                StringComparer.OrdinalIgnoreCase);

        public static readonly IReadOnlySet<string> OptionalHeaders =
            new HashSet<string>(
                [
                    Unit,
                Notes
                ],
                StringComparer.OrdinalIgnoreCase);
    }
}
