using System.Diagnostics.Metrics;

namespace CSVFileUploader.Application.CSV.UploadCsv
{

    public static class UploadMetrics
    {
        public const string MeterName =
            "CSVFileUploader.Uploads";

        private static readonly Meter Meter =
            new(MeterName);

        public static readonly Counter<long> UploadsStarted =
            Meter.CreateCounter<long>(
                "csv_uploads_started",
                unit: "{upload}",
                description: "Number of CSV uploads that started.");

        public static readonly Counter<long> UploadsCompleted =
            Meter.CreateCounter<long>(
                "csv_uploads_completed",
                unit: "{upload}",
                description: "Number of CSV uploads that completed successfully.");

        public static readonly Counter<long> UploadsDuplicated =
            Meter.CreateCounter<long>(
                "csv_uploads_duplicated",
                unit: "{upload}",
                description: "Number of CSV uploads rejected because the file was already processed.");

        public static readonly Counter<long> UploadsFailed =
            Meter.CreateCounter<long>(
                "csv_uploads_failed",
                unit: "{upload}",
                description: "Number of CSV uploads that failed.");

        public static readonly Counter<long> UploadsCancelled =
            Meter.CreateCounter<long>(
                "csv_uploads_cancelled",
                unit: "{upload}",
                description: "Number of CSV uploads that were cancelled.");

        public static readonly Counter<long> RowsInserted =
            Meter.CreateCounter<long>(
                "csv_rows_inserted",
                unit: "{row}",
                description: "Number of CSV rows inserted.");

        public static readonly Counter<long> RowsDuplicated =
            Meter.CreateCounter<long>(
                "csv_rows_duplicated",
                unit: "{row}",
                description: "Number of CSV rows classified as duplicates.");

        public static readonly Counter<long> RowsRejected =
            Meter.CreateCounter<long>(
                "csv_rows_rejected",
                unit: "{row}",
                description: "Number of CSV rows rejected by validation.");
    }
}