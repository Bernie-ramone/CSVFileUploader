using CSVFileUploader.Domain.Enums;
using CSVFileUploader.Domain.ValueObjects;

namespace CSVFileUploader.Domain.Entities
{
    public sealed class ImportedRecord
    {
        public string RecordId { get; private set; }

        public string AssetId { get; private set; }

        public string SourceSite { get; private set; }

        public string DestinationSite { get; private set; }

        public DateOnly EventDate { get; private set; }

        public decimal Volume { get; private set; }

        public string? Unit { get; private set; }

        public string? Notes { get; private set; }

        public ImportRecordStatus Status { get; private set; }

        public ImportedRecordKey BusinessKey =>
            new(
                AssetId,
                SourceSite,
                DestinationSite,
                EventDate,
                Volume);

        private ImportedRecord()
        {
            RecordId = string.Empty;
            AssetId = string.Empty;
            SourceSite = string.Empty;
            DestinationSite = string.Empty;
        }

        private ImportedRecord(
            string recordId,
            string assetId,
            string sourceSite,
            string destinationSite,
            DateOnly eventDate,
            decimal volume,
            string? unit,
            string? notes)
        {
            RecordId = recordId;
            AssetId = assetId;
            SourceSite = sourceSite;
            DestinationSite = destinationSite;
            EventDate = eventDate;
            Volume = volume;
            Unit = unit;
            Notes = notes;
            Status = ImportRecordStatus.Valid;
        }

        public static ImportedRecord Create(
            string recordId,
            string assetId,
            string sourceSite,
            string destinationSite,
            DateOnly eventDate,
            decimal volume,
            string? unit,
            string? notes)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(recordId);
            ArgumentException.ThrowIfNullOrWhiteSpace(assetId);
            ArgumentException.ThrowIfNullOrWhiteSpace(sourceSite);
            ArgumentException.ThrowIfNullOrWhiteSpace(destinationSite);

            if (volume < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(volume),
                    volume,
                    "Volume cannot be negative.");
            }

            return new ImportedRecord(
                recordId.Trim(),
                assetId.Trim(),
                sourceSite.Trim(),
                destinationSite.Trim(),
                eventDate,
                volume,
                string.IsNullOrWhiteSpace(unit) ? null : unit.Trim(),
                string.IsNullOrWhiteSpace(notes) ? null : notes.Trim());
        }

        public void MarkAsDuplicate()
        {
            Status = ImportRecordStatus.Duplicate;
        }
    }
}