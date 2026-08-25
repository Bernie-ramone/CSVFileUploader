using System.Diagnostics;
using System.Text;
using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Infrastructure.CSV;

namespace CSVFileUploader.Infrastructure.Tests.CSV
{

    public class CsvReaderLargeFileTests
    {
        [Fact]
        public async Task ReadAsync_WithFiftyThousandRows_ShouldReadWithinExpectedBounds()
        {
            const int rowCount = 50_000;

            var csv =
                BuildCsv(rowCount);

            await using var stream =
                new MemoryStream(
                    Encoding.UTF8.GetBytes(csv));

            var reader =
                new CsvReader(
                    new CsvUploadOptions
                    {
                        MaximumRowCount = rowCount
                    });

            var stopwatch =
                Stopwatch.StartNew();

            var result =
                await reader.ReadAsync(stream);

            stopwatch.Stop();

            Assert.Equal(
                rowCount,
                result.Rows.Count);

            Assert.Equal(
                8,
                result.Headers.Count);

            Assert.Equal(
                2,
                result.Rows.First().RowNumber);

            Assert.Equal(
                rowCount + 1,
                result.Rows.Last().RowNumber);

            // This is deliberately a generous upper bound.
            // The purpose is to catch an accidental severe regression,
            // not to create a flaky micro-performance test.
            Assert.True(
                stopwatch.Elapsed < TimeSpan.FromSeconds(10),
                $"Reading {rowCount:N0} rows took " +
                $"{stopwatch.Elapsed.TotalSeconds:F2} seconds.");
        }

        private static string BuildCsv(
            int rowCount)
        {
            var builder =
                new StringBuilder(
                    capacity: rowCount * 120);

            builder.AppendLine(
                "RecordId,AssetId,SourceSite,DestinationSite,EventDate,Volume,Unit,Notes");

            for (var index = 1;
                 index <= rowCount;
                 index++)
            {
                builder.Append("REC-")
                    .Append(index.ToString("D6"))
                    .Append(",AST-")
                    .Append(index.ToString("D6"))
                    .Append(",MINE-NORTH,PLANT-A,")
                    .Append("2026-08-01,")
                    .Append("100.00,TON,")
                    .Append("Large file performance test")
                    .AppendLine();
            }

            return builder.ToString();
        }
    }
}