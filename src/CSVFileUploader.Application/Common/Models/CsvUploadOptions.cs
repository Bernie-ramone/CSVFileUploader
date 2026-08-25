namespace CSVFileUploader.Application.Common.Models
{
    public sealed class CsvUploadOptions
    {
        public const string SectionName = "CsvUpload";

        public long MaximumFileSizeInBytes { get; set; } =
            10 * 1024 * 1024;

        public int MaximumRowCount { get; set; } =
            100_000;
    }
}
