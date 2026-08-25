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

    public class UploadCsvTransactionIntegrationTests
    {
        [Fact]
        public async Task UploadCsv_WhenPersistenceFails_ShouldRollbackBusinessDataAndPersistFailedAudit()
        {
            await using var database =
                new TestDatabase();

            await database.InitializeAsync();

            await using var context =
                database.CreateContext();

            var realRepository =
                new ImportedRecordRepository(
                    context);

            var failingRepository =
                new FailingImportedRecordRepository(
                    realRepository);

            var uploadRepository =
                new UploadRepository(
                    context);

            var unitOfWork =
                new UnitOfWork(
                    context);

            var handler =
                new UploadCsvCommandHandler(
                    new CsvReader(
                        new CsvUploadOptions()),
                    new CsvStructureValidator(),
                    failingRepository,
                    uploadRepository,
                    unitOfWork,
                    new CsvRowValidator(),
                    new UploadCsvCommandValidator(
                        new CsvUploadOptions()),
                    NullLogger<UploadCsvCommandHandler>.Instance);

            var csvPath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "TestData",
                    "mock_csv_upload_test.csv");

            await using var stream =
                File.OpenRead(csvPath);

            var command =
                new UploadCsvCommand(
                    stream,
                    "mock_csv_upload_test.csv",
                    "text/csv",
                    stream.Length);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.HandleAsync(command));

            var importedRecordCount =
                await context.ImportedRecords.CountAsync();

            Assert.Equal(
                0,
                importedRecordCount);

            var uploadCount =
                await context.CsvUploads.CountAsync();

            Assert.Equal(
                1,
                uploadCount);

            var upload =
                await context.CsvUploads
                    .Include(x => x.Rows)
                    .SingleAsync();

            Assert.Equal(
                CsvUploadStatus.Failed,
                upload.Status);

            Assert.Equal(
                "mock_csv_upload_test.csv",
                upload.FileName);

            Assert.Equal(
                20,
                upload.TotalRows);

            Assert.Equal(
                0,
                upload.InsertedRows);

            Assert.Equal(
                0,
                upload.DuplicateRows);

            Assert.Equal(
                0,
                upload.ErrorRows);

            var uploadRowsCount =
                await context.CsvUploadRows.CountAsync();

            Assert.Equal(
                0,
                uploadRowsCount);
        }
    }
}