using CSVFileUploader.Application.CSV.UploadHistory;
using CSVFileUploader.Application.DTOs.UploadHistory;
using CSVFileUploader.Domain.Enums;
using System.Text;

namespace CSVFileUploader.Application.Tests.CSV.UploadHistory
{
    public class UploadIssuesCsvExporterTests
    {
        [Fact]
        public void Export_ShouldIncludeOnlyNonImportedRows()
        {
            var upload =
                new UploadHistoryDetailDto(
                    Guid.NewGuid(),
                    "test.csv",
                    DateTimeOffset.UtcNow,
                    3,
                    1,
                    1,
                    1,
                    CsvUploadStatus.CompletedWithErrors,
                    [
                        new UploadHistoryRowDto(
                        2,
                        "REC-0001",
                        CsvUploadRowStatus.Imported,
                        null),

                    new UploadHistoryRowDto(
                        3,
                        "REC-0002",
                        CsvUploadRowStatus.Duplicate,
                        "Record already exists in the database."),

                    new UploadHistoryRowDto(
                        4,
                        "REC-0003",
                        CsvUploadRowStatus.Invalid,
                        "AssetId is required.")
                    ]);

            var exporter =
                new UploadIssuesCsvExporter();

            var bytes =
                exporter.Export(upload);

            var csv =
                Encoding.UTF8.GetString(bytes);

            Assert.Contains(
                "RowNumber,RecordId,Status,Message",
                csv);

            Assert.DoesNotContain(
                "REC-0001",
                csv);

            Assert.Contains(
                "REC-0002",
                csv);

            Assert.Contains(
                "REC-0003",
                csv);
        }

        [Fact]
        public void Export_ShouldEscapeCsvValues()
        {
            var upload =
                new UploadHistoryDetailDto(
                    Guid.NewGuid(),
                    "test.csv",
                    DateTimeOffset.UtcNow,
                    1,
                    0,
                    0,
                    1,
                    CsvUploadStatus.CompletedWithErrors,
                    [
                        new UploadHistoryRowDto(
                        2,
                        "REC-0001",
                        CsvUploadRowStatus.Invalid,
                        "Invalid value, expected \"TON\".")
                    ]);

            var exporter =
                new UploadIssuesCsvExporter();

            var csv =
                Encoding.UTF8.GetString(
                    exporter.Export(upload));

            Assert.Contains(
                "\"Invalid value, expected \"\"TON\"\".\"",
                csv);
        }
    }
}
