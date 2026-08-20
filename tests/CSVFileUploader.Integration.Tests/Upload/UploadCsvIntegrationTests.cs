using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Application.CSV.UploadCsv;
using CSVFileUploader.Application.CSV.Validators;
using CSVFileUploader.Domain.Enums;
using CSVFileUploader.Infrastructure.CSV;
using CSVFileUploader.Infrastructure.Persistence;
using CSVFileUploader.Infrastructure.Persistence.Repositories;
using CSVFileUploader.Integration.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CSVFileUploader.Integration.Tests.Upload
{
    public class UploadCsvIntegrationTests
    {
        [Fact]
        public async Task UploadCsv_WithRealCsvAndDatabase_ShouldProcessFile()
        {
            await using var database =
                new TestDatabase();

            await database.InitializeAsync();

            await using var context =
                database.CreateContext();

            var recordRepository =
                new ImportedRecordRepository(
                    context);

            var uploadRepository =
                new UploadRepository(
                    context);

            var unitOfWork =
                new UnitOfWork(
                    context);

            var csvReader =
                new CsvReader();

            var structureValidator =
                new CsvStructureValidator();

            var rowValidator =
                new CsvRowValidator();

            var commandValidator =
                new UploadCsvCommandValidator(
                    new CsvUploadOptions());

            var logger =
                NullLogger<UploadCsvCommandHandler>.Instance;

            var handler =
                new UploadCsvCommandHandler(
                    csvReader,
                    structureValidator,
                    recordRepository,
                    uploadRepository,
                    unitOfWork,
                    rowValidator,
                    commandValidator,
                    logger);

            var filePath = Path.Combine(
                AppContext.BaseDirectory,
                "TestData",
                "mock_csv_upload_test.csv");

            await using var stream =
                File.OpenRead(filePath);

            var command =
                new UploadCsvCommand(
                    stream,
                    "mock_csv_upload_test.csv",
                    "text/csv",
                    stream.Length);

            var result =
                await handler.HandleAsync(command);

            Assert.Equal(
                20,
                result.TotalRows);

            Assert.Equal(
                18,
                result.InsertedRows);

            Assert.Equal(
                2,
                result.DuplicateRows);

            Assert.Empty(
                result.Errors);

            var persistedRecords =
                await context.ImportedRecords
                    .ToListAsync();

            Assert.Equal(
                18,
                persistedRecords.Count);

            var upload =
                await context.CsvUploads
                    .Include(x => x.Rows)
                    .SingleAsync();

            Assert.Equal(
                CsvUploadStatus.Completed,
                upload.Status);

            Assert.Equal(
                20,
                upload.TotalRows);

            Assert.Equal(
                18,
                upload.InsertedRows);

            Assert.Equal(
                2,
                upload.DuplicateRows);

            Assert.Equal(
                0,
                upload.ErrorRows);

            Assert.Equal(
                20,
                upload.Rows.Count);

            Assert.Equal(
                18,
                upload.Rows.Count(
                    row => row.Status ==
                        CsvUploadRowStatus.Imported));

            Assert.Equal(
                2,
                upload.Rows.Count(
                    row => row.Status ==
                        CsvUploadRowStatus.Duplicate));

            Assert.Equal(
                0,
                upload.Rows.Count(
                    row => row.Status ==
                        CsvUploadRowStatus.Invalid));
        }
    }
}
