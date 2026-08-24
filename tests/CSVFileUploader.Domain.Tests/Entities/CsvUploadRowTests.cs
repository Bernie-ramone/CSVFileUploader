using CSVFileUploader.Domain.Entities;
using CSVFileUploader.Domain.Enums;

namespace CSVFileUploader.Domain.Tests.Entities
{
    public class CsvUploadRowTests
    {
        [Fact]
        public void Imported_ShouldCreateImportedRow()
        {
            var row = CsvUploadRow.Imported(
                rowNumber: 2,
                recordId: "REC-0001");

            Assert.Equal(2, row.RowNumber);
            Assert.Equal(
                "REC-0001",
                row.RecordId);

            Assert.Equal(
                CsvUploadRowStatus.Imported,
                row.Status);

            Assert.Null(row.ErrorMessage);
        }

        [Fact]
        public void Duplicate_ShouldCreateDuplicateRow()
        {
            var row = CsvUploadRow.Duplicate(
                rowNumber: 5,
                recordId: "REC-0005",
                reason: "Record already exists in the database.");

            Assert.Equal(
                        "Record already exists in the database.",
                        row.ErrorMessage);
        }

        [Fact]
        public void Invalid_ShouldStoreErrorMessage()
        {
            var row = CsvUploadRow.Invalid(
                rowNumber: 8,
                recordId: "REC-0008",
                errorMessage: "Invalid AssetId.");

            Assert.Equal(
                CsvUploadRowStatus.Invalid,
                row.Status);

            Assert.Equal(
                "Invalid AssetId.",
                row.ErrorMessage);
        }
    }
}
