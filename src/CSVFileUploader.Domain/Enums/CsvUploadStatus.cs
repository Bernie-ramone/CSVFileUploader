namespace CSVFileUploader.Domain.Enums
{
    public enum CsvUploadStatus
    {
        Processing = 1,
        Completed = 2,
        CompletedWithErrors = 3,
        Failed = 4
    }
}
