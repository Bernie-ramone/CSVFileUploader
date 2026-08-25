using CSVFileUploader.Domain.Enums;

namespace CSVFileUploader.Domain.Entities
{
    public sealed class CsvUpload
    {
        private readonly List<CsvUploadRow> _rows = [];

        public Guid Id { get; private set; }

        public string FileName { get; private set; }

        public string? FileHash { get; private set; }

        public DateTimeOffset UploadedAtUtc { get; private set; }

        public int TotalRows { get; private set; }

        public int InsertedRows { get; private set; }

        public int DuplicateRows { get; private set; }

        public int ErrorRows { get; private set; }

        public CsvUploadStatus Status { get; private set; }

        public IReadOnlyCollection<CsvUploadRow> Rows =>
            _rows.AsReadOnly();

        private CsvUpload()
        {
            FileName = string.Empty;
        }

        private CsvUpload(
            string fileName,
            string? fileHash,
            DateTimeOffset uploadedAtUtc)
        {
            Id = Guid.NewGuid();
            FileName = fileName;
            FileHash = string.IsNullOrWhiteSpace(fileHash)
                ? null
                : fileHash.Trim().ToUpperInvariant();

            UploadedAtUtc = uploadedAtUtc;
            Status = CsvUploadStatus.Processing;
        }

        public static CsvUpload Start(
            string fileName,
            DateTimeOffset uploadedAtUtc,
            string? fileHash = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(
                fileName);

            return new CsvUpload(
                fileName.Trim(),
                fileHash,
                uploadedAtUtc);
        }

        public void AddRow(
            CsvUploadRow row)
        {
            ArgumentNullException.ThrowIfNull(row);

            _rows.Add(row);
        }

        public void Complete(
            int totalRows,
            int insertedRows,
            int duplicateRows,
            int errorRows)
        {
            ValidateCounts(
                totalRows,
                insertedRows,
                duplicateRows,
                errorRows);

            TotalRows = totalRows;
            InsertedRows = insertedRows;
            DuplicateRows = duplicateRows;
            ErrorRows = errorRows;

            Status = errorRows > 0
                ? CsvUploadStatus.CompletedWithErrors
                : CsvUploadStatus.Completed;
        }

        public void MarkAsFailed()
        {
            Status = CsvUploadStatus.Failed;

            InsertedRows = 0;
            DuplicateRows = 0;
            ErrorRows = 0;

            _rows.Clear();
        }

        private static void ValidateCounts(
            int totalRows,
            int insertedRows,
            int duplicateRows,
            int errorRows)
        {
            if (totalRows < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(totalRows));
            }

            if (insertedRows < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(insertedRows));
            }

            if (duplicateRows < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(duplicateRows));
            }

            if (errorRows < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(errorRows));
            }

            if (insertedRows +
                duplicateRows +
                errorRows != totalRows)
            {
                throw new InvalidOperationException(
                    "Upload row counts do not match " +
                    "the total row count.");
            }
        }
    }
}
