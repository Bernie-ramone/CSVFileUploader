using CSVFileUploader.Application.CSV.UploadCsv;
using CSVFileUploader.Application.CSV.Validators;
using CSVFileUploader.Infrastructure.CSV;
using CSVFileUploader.Infrastructure.Persistence.Repositories;
using CSVFileUploader.Integration.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CSVFileUploader.Integration.Tests.Upload
{
    public class UploadCsvIntegrationTests
    {
        [Fact]
        public async Task UploadCsv_WithRealCsvAndDatabase_ShouldProcessFile()
        {
            await using var database = new Infrastructure.TestDatabase();

            await database.InitializeAsync();

            await using var context =
                database.CreateContext();

            var repository =
                new ImportedRecordRepository(context);

            var csvReader =
                new CsvReader();

            var structureValidator =
                new CsvStructureValidator();

            var rowValidator =
                new CsvRowValidator();

            var commandValidator =
                new UploadCsvCommandValidator();

            var handler =
                new UploadCsvCommandHandler(
                    csvReader,
                    structureValidator,
                    repository,
                    rowValidator,
                    commandValidator);

            var filePath = Path.Combine(
                AppContext.BaseDirectory,
                "TestData",
                "mock_csv_upload_test.csv");

            await using var stream =
                File.OpenRead(filePath);

            var command =
                new UploadCsvCommand(
                    FileStream: stream,
                    FileName: "mock_csv_upload_test.csv",
                    ContentType: "text/csv",
                    FileSize: stream.Length);

            var result =
                await handler.HandleAsync(command);

            Console.WriteLine($"TotalRows: {result.TotalRows}");
            Console.WriteLine($"InsertedRows: {result.InsertedRows}");
            Console.WriteLine($"DuplicateRows: {result.DuplicateRows}");
            Console.WriteLine($"ErrorCount: {result.Errors.Count}");

            foreach (var error in result.Errors)
            {
                Console.WriteLine(
                    $"Row {error.RowNumber}: {error.Message}");
            }


            Assert.Equal(20, result.TotalRows);
            Assert.Equal(18, result.InsertedRows);
            Assert.Equal(2, result.DuplicateRows);
            Assert.Empty(result.Errors);

            //var persistedRecords =
            //    await context.ImportedRecords.ToListAsync();

            var persistedRecords =
    await context.ImportedRecords
        .OrderBy(x => x.RecordId)
        .ToListAsync();

            foreach (var record in persistedRecords)
            {
                Console.WriteLine(
                    $"{record.RecordId} | " +
                    $"{record.AssetId} | " +
                    $"{record.SourceSite} | " +
                    $"{record.DestinationSite} | " +
                    $"{record.EventDate:yyyy-MM-dd} | " +
                    $"{record.Volume}");
            }

            Assert.Equal(
                18,
                persistedRecords.Count);
        }
    }
}
