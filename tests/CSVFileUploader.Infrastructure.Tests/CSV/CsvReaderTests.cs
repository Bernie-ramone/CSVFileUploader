using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Infrastructure.CSV;
using System.Text;

namespace CSVFileUploader.Infrastructure.Tests.CSV
{

    public class CsvReaderTests
    {
        [Fact]
        public async Task ReadAsync_WithValidCsv_ShouldReadRows()
        {
            const string csv =
                """
            RecordId,AssetId,SourceSite,DestinationSite,EventDate,Volume,Unit,Notes
            REC-0001,AST-1001,MINE-NORTH,PLANT-A,2026-08-01,125.50,TON,Morning shift
            REC-0002,AST-1002,MINE-NORTH,PLANT-A,2026-08-01,118.25,TON,Morning shift
            """;

            await using var stream =
                new MemoryStream(
                    Encoding.UTF8.GetBytes(csv));

            var reader =
                CreateReader();

            var result =
                await reader.ReadAsync(stream);

            Assert.Equal(
                8,
                result.Headers.Count);

            Assert.Equal(
                2,
                result.Rows.Count);

            var firstRow =
                result.Rows.First();

            Assert.Equal(
                2,
                firstRow.RowNumber);

            Assert.Equal(
                "REC-0001",
                firstRow.RecordId);

            Assert.Equal(
                "AST-1001",
                firstRow.AssetId);

            Assert.Equal(
                "MINE-NORTH",
                firstRow.SourceSite);

            Assert.Equal(
                "PLANT-A",
                firstRow.DestinationSite);

            Assert.Equal(
                "2026-08-01",
                firstRow.EventDate);

            Assert.Equal(
                "125.50",
                firstRow.Volume);

            Assert.Equal(
                "TON",
                firstRow.Unit);

            Assert.Equal(
                "Morning shift",
                firstRow.Notes);
        }

        [Fact]
        public async Task ReadAsync_WithOptionalColumnsEmpty_ShouldReturnNull()
        {
            const string csv =
                """
            RecordId,AssetId,SourceSite,DestinationSite,EventDate,Volume,Unit,Notes
            REC-0001,AST-1001,MINE-NORTH,PLANT-A,2026-08-01,125.50,,
            """;

            await using var stream =
                new MemoryStream(
                    Encoding.UTF8.GetBytes(csv));

            var reader =
                CreateReader();

            var result =
                await reader.ReadAsync(stream);

            var row =
                Assert.Single(
                    result.Rows);

            Assert.Null(row.Unit);
            Assert.Null(row.Notes);
        }

        [Fact]
        public async Task ReadAsync_WithInvalidDate_ShouldStillReadRawValue()
        {
            const string csv =
                """
            RecordId,AssetId,SourceSite,DestinationSite,EventDate,Volume,Unit,Notes
            REC-0001,AST-1001,MINE-NORTH,PLANT-A,08/01/2026,125.50,TON,Invalid date
            """;

            await using var stream =
                new MemoryStream(
                    Encoding.UTF8.GetBytes(csv));

            var reader =
                CreateReader();

            var result =
                await reader.ReadAsync(stream);

            var row =
                Assert.Single(
                    result.Rows);

            Assert.Equal(
                "08/01/2026",
                row.EventDate);
        }

        [Fact]
        public async Task ReadAsync_WithInvalidVolume_ShouldStillReadRawValue()
        {
            const string csv =
                """
            RecordId,AssetId,SourceSite,DestinationSite,EventDate,Volume,Unit,Notes
            REC-0001,AST-1001,MINE-NORTH,PLANT-A,2026-08-01,INVALID,TON,Invalid volume
            """;

            await using var stream =
                new MemoryStream(
                    Encoding.UTF8.GetBytes(csv));

            var reader =
                CreateReader();

            var result =
                await reader.ReadAsync(stream);

            var row =
                Assert.Single(
                    result.Rows);

            Assert.Equal(
                "INVALID",
                row.Volume);
        }

        [Fact]
        public async Task ReadAsync_WhenRowCountExceedsLimit_ShouldThrow()
        {
            var builder =
                new StringBuilder();

            builder.AppendLine(
                "RecordId,AssetId,SourceSite,DestinationSite,EventDate,Volume,Unit,Notes");

            for (var index = 1;
                 index <= 3;
                 index++)
            {
                builder.AppendLine(
                    $"REC-{index:0000},AST-{index:0000}," +
                    "MINE-NORTH,PLANT-A,2026-08-01,100.00,TON,Test");
            }

            await using var stream =
                new MemoryStream(
                    Encoding.UTF8.GetBytes(
                        builder.ToString()));

            var options =
                new CsvUploadOptions
                {
                    MaximumRowCount = 2
                };

            var reader =
                new CsvReader(options);

            var exception =
                await Assert.ThrowsAsync<
                    InvalidOperationException>(
                    () => reader.ReadAsync(stream));

            Assert.Contains(
                "more than 2 data rows",
                exception.Message);
        }

        [Fact]
        public async Task ReadAsync_WhenRowCountIsExactlyAtLimit_ShouldSucceed()
        {
            const string csv =
                """
            RecordId,AssetId,SourceSite,DestinationSite,EventDate,Volume,Unit,Notes
            REC-0001,AST-0001,MINE-NORTH,PLANT-A,2026-08-01,100.00,TON,Test
            REC-0002,AST-0002,MINE-NORTH,PLANT-A,2026-08-01,100.00,TON,Test
            """;

            await using var stream =
                new MemoryStream(
                    Encoding.UTF8.GetBytes(csv));

            var options =
                new CsvUploadOptions
                {
                    MaximumRowCount = 2
                };

            var reader =
                new CsvReader(options);

            var result =
                await reader.ReadAsync(stream);

            Assert.Equal(
                2,
                result.Rows.Count);
        }

        private static CsvReader CreateReader()
        {
            return new CsvReader(
                new CsvUploadOptions());
        }
    }
}