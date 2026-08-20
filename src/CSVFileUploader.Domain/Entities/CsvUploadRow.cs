using CSVFileUploader.Domain.Enums;

namespace CSVFileUploader.Domain.Entities
{
    public sealed class CsvUploadRow
    {
        public Guid Id { get; private set; }

        public Guid CsvUploadId { get; private set; }

        public int RowNumber { get; private set; }

        public string? RecordId { get; private set; }

        public CsvUploadRowStatus Status { get; private set; }

        public string? ErrorMessage { get; private set; }

        private CsvUploadRow()
        {
        }

        private CsvUploadRow(
            int rowNumber,
            string? recordId,
            CsvUploadRowStatus status,
            string? errorMessage)
        {
            Id = Guid.NewGuid();
            RowNumber = rowNumber;
            RecordId = string.IsNullOrWhiteSpace(recordId)
                ? null
                : recordId.Trim();
            Status = status;
            ErrorMessage = string.IsNullOrWhiteSpace(
                errorMessage)
                ? null
                : errorMessage.Trim();
        }

        public static CsvUploadRow Imported(
            int rowNumber,
            string? recordId)
        {
            return new CsvUploadRow(
                rowNumber,
                recordId,
                CsvUploadRowStatus.Imported,
                null);
        }

        public static CsvUploadRow Duplicate(
            int rowNumber,
            string? recordId)
        {
            return new CsvUploadRow(
                rowNumber,
                recordId,
                CsvUploadRowStatus.Duplicate,
                null);
        }

        public static CsvUploadRow Invalid(
            int rowNumber,
            string? recordId,
            string errorMessage)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(
                errorMessage);

            return new CsvUploadRow(
                rowNumber,
                recordId,
                CsvUploadRowStatus.Invalid,
                errorMessage);
        }
    }
}
