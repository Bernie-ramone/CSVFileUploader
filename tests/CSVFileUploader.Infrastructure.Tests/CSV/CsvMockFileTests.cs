using CSVFileUploader.Infrastructure.CSV;

namespace CSVFileUploader.Infrastructure.Tests.CSV
{
    public class CsvMockFileTests
    {
        [Fact]
        public async Task ReadAsync_WithMockCsv_ShouldReadTwentyRows()
        {
            var filePath = Path.Combine(
                AppContext.BaseDirectory,
                "TestData",
                "mock_csv_upload_test.csv");

            await using var stream =
                File.OpenRead(filePath);

            var reader = new CsvReader();

            var result = await reader.ReadAsync(stream);

            Assert.Equal(8, result.Headers.Count);
            Assert.Equal(20, result.Rows.Count);
        }
    }
}
