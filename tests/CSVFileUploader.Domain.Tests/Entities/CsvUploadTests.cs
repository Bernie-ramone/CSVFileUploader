using CSVFileUploader.Domain.Entities;
using CSVFileUploader.Domain.Enums;

namespace CSVFileUploader.Domain.Tests.Entities
{
    public class CsvUploadTests
    {
        [Fact]
        public void Start_ShouldCreateProcessingUpload()
        {
            var upload = CsvUpload.Start(
                "test.csv",
                new DateTimeOffset(
                    2026,
                    8,
                    20,
                    10,
                    0,
                    0,
                    TimeSpan.Zero));

            Assert.NotEqual(Guid.Empty, upload.Id);
            Assert.Equal("test.csv", upload.FileName);
            Assert.Equal(
                CsvUploadStatus.Processing,
                upload.Status);
        }

        [Fact]
        public void Complete_WithValidCounts_ShouldComplete()
        {
            var upload = CsvUpload.Start(
                "test.csv",
                DateTimeOffset.UtcNow);

            upload.Complete(
                totalRows: 20,
                insertedRows: 18,
                duplicateRows: 2,
                errorRows: 0);

            Assert.Equal(
                CsvUploadStatus.Completed,
                upload.Status);

            Assert.Equal(20, upload.TotalRows);
            Assert.Equal(18, upload.InsertedRows);
            Assert.Equal(2, upload.DuplicateRows);
            Assert.Equal(0, upload.ErrorRows);
        }

        [Fact]
        public void Complete_WithErrors_ShouldHaveCompletedWithErrorsStatus()
        {
            var upload = CsvUpload.Start(
                "test.csv",
                DateTimeOffset.UtcNow);

            upload.Complete(
                totalRows: 20,
                insertedRows: 17,
                duplicateRows: 2,
                errorRows: 1);

            Assert.Equal(
                CsvUploadStatus.CompletedWithErrors,
                upload.Status);
        }

        [Fact]
        public void Complete_WithInvalidCounts_ShouldThrow()
        {
            var upload = CsvUpload.Start(
                "test.csv",
                DateTimeOffset.UtcNow);

            Assert.Throws<InvalidOperationException>(
                () => upload.Complete(
                    totalRows: 20,
                    insertedRows: 18,
                    duplicateRows: 2,
                    errorRows: 1));
        }
    }
}
