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

            var handler =
                CreateHandler(
                    recordRepository,
                    uploadRepository,
                    unitOfWork);

            var filePath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "TestData",
                    "mock_csv_upload_test.csv");

            const string fileHash =
                "TEST-HASH-001";

            await using var stream =
                File.OpenRead(filePath);

            var result =
                await handler.HandleAsync(
                    new UploadCsvCommand(
                        stream,
                        "mock_csv_upload_test.csv",
                        "text/csv",
                        stream.Length,
                        fileHash));

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
                "TEST-HASH-001",
                upload.FileHash);

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
        }

        [Fact]
        public async Task UploadCsv_WithSameSuccessfulFileHash_ShouldRejectSecondUpload()
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

            var handler =
                CreateHandler(
                    recordRepository,
                    uploadRepository,
                    unitOfWork);

            const string fileHash =
                "TEST-HASH-IDEMPOTENT";

            var filePath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "TestData",
                    "mock_csv_upload_test.csv");

            await using var firstStream =
                File.OpenRead(filePath);

            var firstResult =
                await handler.HandleAsync(
                    new UploadCsvCommand(
                        firstStream,
                        "mock_csv_upload_test.csv",
                        "text/csv",
                        firstStream.Length,
                        fileHash));

            Assert.Empty(
                firstResult.Errors);

            await using var secondStream =
                File.OpenRead(filePath);

            var secondResult =
                await handler.HandleAsync(
                    new UploadCsvCommand(
                        secondStream,
                        "mock_csv_upload_test.csv",
                        "text/csv",
                        secondStream.Length,
                        fileHash));

            Assert.Equal(
                0,
                secondResult.TotalRows);

            Assert.Equal(
                0,
                secondResult.InsertedRows);

            Assert.Equal(
                0,
                secondResult.DuplicateRows);

            var error =
                Assert.Single(
                    secondResult.Errors);

            Assert.Equal(
                0,
                error.RowNumber);

            Assert.Equal(
                "This exact file has already been processed.",
                error.Message);

            Assert.Equal(
                1,
                await context.CsvUploads.CountAsync());

            Assert.Equal(
                18,
                await context.ImportedRecords.CountAsync());
        }

        [Fact]
        public async Task UploadCsv_WithFailedPreviousUpload_ShouldAllowRetry()
        {
            await using var database =
                new TestDatabase();

            await database.InitializeAsync();

            await using var context =
                database.CreateContext();

            const string fileHash =
                "TEST-HASH-FAILED";

            var failedUpload =
                CSVFileUploader.Domain.Entities.CsvUpload.Start(
                    "failed.csv",
                    DateTimeOffset.UtcNow,
                    fileHash);

            failedUpload.MarkAsFailed();

            context.CsvUploads.Add(
                failedUpload);

            await context.SaveChangesAsync();

            var recordRepository =
                new ImportedRecordRepository(
                    context);

            var uploadRepository =
                new UploadRepository(
                    context);

            var unitOfWork =
                new UnitOfWork(
                    context);

            var handler =
                CreateHandler(
                    recordRepository,
                    uploadRepository,
                    unitOfWork);

            var filePath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "TestData",
                    "mock_csv_upload_test.csv");

            await using var stream =
                File.OpenRead(filePath);

            var result =
                await handler.HandleAsync(
                    new UploadCsvCommand(
                        stream,
                        "mock_csv_upload_test.csv",
                        "text/csv",
                        stream.Length,
                        fileHash));

            Assert.Empty(
                result.Errors);

            Assert.Equal(
                2,
                await context.CsvUploads.CountAsync());

            Assert.Equal(
                18,
                await context.ImportedRecords.CountAsync());
        }

        private static UploadCsvCommandHandler CreateHandler(
            ImportedRecordRepository recordRepository,
            UploadRepository uploadRepository,
            UnitOfWork unitOfWork)
        {
            return new UploadCsvCommandHandler(
                new CsvReader(
                    new CsvUploadOptions()),
                new CsvStructureValidator(),
                recordRepository,
                uploadRepository,
                unitOfWork,
                new CsvRowValidator(),
                new UploadCsvCommandValidator(
                    new CsvUploadOptions()),
                NullLogger<UploadCsvCommandHandler>.Instance);
        }
    }
}