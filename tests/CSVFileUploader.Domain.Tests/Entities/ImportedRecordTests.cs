using CSVFileUploader.Domain.Entities;
using CSVFileUploader.Domain.Enums;

namespace CSVFileUploader.Domain.Tests.Entities
{
    public class ImportedRecordTests
    {
        [Fact]
        public void Create_WithValidValues_ShouldCreateRecord()
        {
            var record = ImportedRecord.Create(
                "REC-0001",
                "AST-1001",
                "MINE-NORTH",
                "PLANT-A",
                new DateOnly(2026, 8, 1),
                125.50m,
                "TON",
                "Morning shift");

            Assert.Equal("REC-0001", record.RecordId);
            Assert.Equal("AST-1001", record.AssetId);
            Assert.Equal("MINE-NORTH", record.SourceSite);
            Assert.Equal("PLANT-A", record.DestinationSite);
            Assert.Equal(new DateOnly(2026, 8, 1), record.EventDate);
            Assert.Equal(125.50m, record.Volume);
            Assert.Equal("TON", record.Unit);
            Assert.Equal("Morning shift", record.Notes);
            Assert.Equal(ImportRecordStatus.Valid, record.Status);
        }

        [Fact]
        public void Create_WithNegativeVolume_ShouldThrowException()
        {
            var action = () => ImportedRecord.Create(
                "REC-0001",
                "AST-1001",
                "MINE-NORTH",
                "PLANT-A",
                new DateOnly(2026, 8, 1),
                -1m,
                "TON",
                null);

            Assert.Throws<ArgumentOutOfRangeException>(action);
        }

        [Fact]
        public void MarkAsDuplicate_ShouldChangeStatus()
        {
            var record = ImportedRecord.Create(
                "REC-0001",
                "AST-1001",
                "MINE-NORTH",
                "PLANT-A",
                new DateOnly(2026, 8, 1),
                125.50m,
                "TON",
                null);

            record.MarkAsDuplicate();

            Assert.Equal(ImportRecordStatus.Duplicate, record.Status);
        }

        [Fact]
        public void BusinessKey_ShouldContainDuplicateDetectionFields()
        {
            var record = ImportedRecord.Create(
                "REC-0001",
                "AST-1001",
                "MINE-NORTH",
                "PLANT-A",
                new DateOnly(2026, 8, 1),
                125.50m,
                "TON",
                null);

            var key = record.BusinessKey;

            Assert.Equal("AST-1001", key.AssetId);
            Assert.Equal("MINE-NORTH", key.SourceSite);
            Assert.Equal("PLANT-A", key.DestinationSite);
            Assert.Equal(new DateOnly(2026, 8, 1), key.EventDate);
            Assert.Equal(125.50m, key.Volume);
        }
    }
}
