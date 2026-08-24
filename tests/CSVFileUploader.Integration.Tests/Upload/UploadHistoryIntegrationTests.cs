using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Application.CSV.UploadCsv;
using CSVFileUploader.Application.CSV.UploadHistory;
using CSVFileUploader.Application.CSV.Validators;
using CSVFileUploader.Infrastructure.CSV;
using CSVFileUploader.Infrastructure.Persistence;
using CSVFileUploader.Infrastructure.Persistence.Repositories;
using CSVFileUploader.Integration.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CSVFileUploader.Integration.Tests.Upload
{
    public class UploadHistoryIntegrationTests
    {
        [Fact]
        public async Task GetUploadHistory_ShouldReturnCompletedUpload()
        {
            await using var database =
                new TestDatabase();

            await database.InitializeAsync();

            await using var context =
                database.CreateContext();

            var recordRepository =
                new ImportedRecordRepository(context);

            var uploadRepository =
                new UploadRepository(context);

            var unitOfWork =
                new UnitOfWork(context);

            var historyRepository =
                new UploadHistoryRepository(context);

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

            var result =
                await handler.HandleAsync(
                    new UploadCsvCommand(
                        stream,
                        "mock_csv_upload_test.csv",
                        "text/csv",
                        stream.Length));

            Assert.Equal(20, result.TotalRows);

            var queryHandler =
                new GetUploadHistoryQueryHandler(
                    historyRepository);

            
            var history = await queryHandler.HandleAsync(
                new GetUploadHistoryQuery(
                PageNumber: 1,
                PageSize: 20));

            var upload = Assert.Single(history.Items);

            Assert.Equal(
                "mock_csv_upload_test.csv",
                upload.FileName);

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
                1,
                history.PageNumber);

            Assert.Equal(
                20,
                history.PageSize);

            Assert.Equal(
                1,
                history.TotalCount);

            Assert.Equal(
                1,
                history.TotalPages);
        }

        [Fact]
        public async Task GetUploadDetails_ShouldReturnRows()
        {
            await using var database =
                new TestDatabase();

            await database.InitializeAsync();

            await using var context =
                database.CreateContext();

            var recordRepository =
                new ImportedRecordRepository(context);

            var uploadRepository =
                new UploadRepository(context);

            var unitOfWork =
                new UnitOfWork(context);

            var historyRepository =
                new UploadHistoryRepository(context);

            var handler =
                new UploadCsvCommandHandler(
                    new CsvReader(),
                    new CsvStructureValidator(),
                    recordRepository,
                    uploadRepository,
                    unitOfWork,
                    new CsvRowValidator(),
                    new UploadCsvCommandValidator(
                        new CsvUploadOptions()),
                    NullLogger<UploadCsvCommandHandler>.Instance);

            var filePath = Path.Combine(
                AppContext.BaseDirectory,
                "TestData",
                "mock_csv_upload_test.csv");

            await using var stream =
                File.OpenRead(filePath);

            await handler.HandleAsync(
                new UploadCsvCommand(
                    stream,
                    "mock_csv_upload_test.csv",
                    "text/csv",
                    stream.Length));

            var upload =
                await context.CsvUploads
                    .SingleAsync();

            var queryHandler =
                new GetUploadHistoryDetailsQueryHandler(
                    historyRepository);

            var details =
                await queryHandler.HandleAsync(
                    new GetUploadHistoryDetailsQuery(
                        upload.Id));

            Assert.NotNull(details);

            Assert.Equal(
                20,
                details!.Rows.Count);

            Assert.Equal(
                18,
                details.Rows.Count(
                    row =>
                        row.Status ==
                        Domain.Enums.CsvUploadRowStatus.Imported));

            Assert.Equal(
                2,
                details.Rows.Count(
                    row =>
                        row.Status ==
                        Domain.Enums.CsvUploadRowStatus.Duplicate));
        }

        [Fact]
        public async Task GetUploadHistory_ShouldReturnRequestedPage()
        {
            await using var database =
                new Infrastructure.TestDatabase();

            await database.InitializeAsync();

            await using var context =
                database.CreateContext();

            for (var index = 1; index <= 25; index++)
            {
                var upload =
                    CSVFileUploader.Domain.Entities.CsvUpload.Start(
                        $"file-{index}.csv",
                        DateTimeOffset.UtcNow.AddMinutes(index));

                upload.Complete(
                    totalRows: 1,
                    insertedRows: 1,
                    duplicateRows: 0,
                    errorRows: 0);

                context.CsvUploads.Add(upload);
            }

            await context.SaveChangesAsync();

            var repository =
                new UploadHistoryRepository(
                    context);

            var result =
                await repository.GetHistoryAsync(
                    pageNumber: 2,
                    pageSize: 10);

            Assert.Equal(
                10,
                result.Items.Count);

            Assert.Equal(
                2,
                result.PageNumber);

            Assert.Equal(
                10,
                result.PageSize);

            Assert.Equal(
                25,
                result.TotalCount);

            Assert.Equal(
                3,
                result.TotalPages);
        }
    }
}