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

            var reader = new CsvReader();

            var result = await reader.ReadAsync(stream);

            Assert.Equal(8, result.Headers.Count);
            Assert.Equal(2, result.Rows.Count);

            var firstRow = result.Rows.First();

            Assert.Equal(2, firstRow.RowNumber);
            Assert.Equal("REC-0001", firstRow.RecordId);
            Assert.Equal("AST-1001", firstRow.AssetId);
            Assert.Equal("MINE-NORTH", firstRow.SourceSite);
            Assert.Equal("PLANT-A", firstRow.DestinationSite);
            Assert.Equal("2026-08-01", firstRow.EventDate);
            Assert.Equal("125.50", firstRow.Volume);
            Assert.Equal("TON", firstRow.Unit);
            Assert.Equal("Morning shift", firstRow.Notes);
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

            var reader = new CsvReader();

            var result = await reader.ReadAsync(stream);

            var row = Assert.Single(result.Rows);

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

            var reader = new CsvReader();

            var result = await reader.ReadAsync(stream);

            var row = Assert.Single(result.Rows);

            Assert.Equal("08/01/2026", row.EventDate);
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

            var reader = new CsvReader();

            var result = await reader.ReadAsync(stream);

            var row = Assert.Single(result.Rows);

            Assert.Equal("INVALID", row.Volume);
        }
    }
}
