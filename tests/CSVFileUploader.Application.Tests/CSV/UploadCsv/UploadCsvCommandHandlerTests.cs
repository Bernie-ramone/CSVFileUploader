using CSVFileUploader.Application.Common.Interfaces;
using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Application.CSV.UploadCsv;
using CSVFileUploader.Application.DTOs;
using CSVFileUploader.Domain.Entities;
using CSVFileUploader.Domain.ValueObjects;
using CSVFileUploader.Application.CSV.Validators;

namespace CSVFileUploader.Application.Tests.CSV.UploadCsv
{
    public class UploadCsvCommandHandlerTests
    {
        [Fact]
        public async Task HandleAsync_WithValidRows_ShouldInsertRecords()
        {
            var csvReader = new FakeCsvReader(
                new CsvReadResult(
                    [
                        "RecordId",
                    "AssetId",
                    "SourceSite",
                    "DestinationSite",
                    "EventDate",
                    "Volume",
                    "Unit",
                    "Notes"
                    ],
                    [
                        new CsvRowDto(
    RowNumber: 2,
    RecordId: "REC-0001",
    AssetId: "AST-1001",
    SourceSite: "MINE-NORTH",
    DestinationSite: "PLANT-A",
    EventDate: "2026-08-01",
    Volume: "125.50",
    Unit: "TON",
    Notes: "Morning shift")
                    ]));

            var structureValidator = new FakeStructureValidator(
                CsvStructureValidationResult.Success());

            var repository = new FakeImportedRecordRepository();
            var rowValidator = new CsvRowValidator();
            var commandValidator = new UploadCsvCommandValidator();

            var handler = new UploadCsvCommandHandler(
                csvReader,
                structureValidator,
                repository,
                rowValidator,
                   commandValidator);

            await using var stream = new MemoryStream();

            var command = new UploadCsvCommand(
                stream,
                "test.csv",
                "text/csv",
                100);

            var result = await handler.HandleAsync(command);

            Assert.Equal(1, result.TotalRows);
            Assert.Equal(1, result.InsertedRows);
            Assert.Equal(0, result.DuplicateRows);
            Assert.Empty(result.Errors);
            Assert.Single(repository.InsertedRecords);
        }

        [Fact]
        public async Task HandleAsync_WithInvalidStructure_ShouldReturnErrors()
        {
            var csvReader = new FakeCsvReader(
                new CsvReadResult(
                    ["RecordId"],
                    []));

            var structureValidator = new FakeStructureValidator(
                CsvStructureValidationResult.Failure(
                    "Missing required columns."));

            var repository = new FakeImportedRecordRepository();
            var rowValidator = new CsvRowValidator();
            var commandValidator = new UploadCsvCommandValidator();
            var handler = new UploadCsvCommandHandler(
                csvReader,
                structureValidator,
                repository,
                rowValidator,
                commandValidator);

            await using var stream = new MemoryStream();

            var command = new UploadCsvCommand(
                stream,
                "test.csv",
                "text/csv",
                100);

            var result = await handler.HandleAsync(command);

            Assert.Equal(0, result.InsertedRows);
            Assert.Single(result.Errors);
            Assert.Empty(repository.InsertedRecords);
        }

        [Fact]
        public async Task HandleAsync_WithInvalidRow_ShouldReturnErrorAndNotInsertRecord()
        {
            var csvReader = new FakeCsvReader(
                new CsvReadResult(
                    [
                        "RecordId",
                "AssetId",
                "SourceSite",
                "DestinationSite",
                "EventDate",
                "Volume",
                "Unit",
                "Notes"
                    ],
                    [
                        new CsvRowDto(
                    RowNumber: 2,
                    RecordId: "REC-0001",
                    AssetId: "INVALID",
                    SourceSite: "MINE-NORTH",
                    DestinationSite: "PLANT-A",
                    EventDate: "2026-08-01",
                    Volume: "125.50",
                    Unit: "TON",
                    Notes: "Invalid asset")
                    ]));

            var structureValidator = new FakeStructureValidator(
                CsvStructureValidationResult.Success());

            var repository = new FakeImportedRecordRepository();

            var rowValidator = new CsvRowValidator();
            var commandValidator = new UploadCsvCommandValidator();

            var handler = new UploadCsvCommandHandler(
                csvReader,
                structureValidator,
                repository,
                rowValidator,
                commandValidator);

            await using var stream = new MemoryStream();

            var command = new UploadCsvCommand(
                stream,
                "test.csv",
                "text/csv",
                100);

            var result = await handler.HandleAsync(command);
            var error = Assert.Single(result.Errors);

            Assert.Equal(1, result.TotalRows);
            Assert.Equal(0, result.InsertedRows);
            Assert.Equal(0, result.DuplicateRows);

            Assert.Single(result.Errors);

            Assert.Equal(2, error.RowNumber);

            Assert.Empty(repository.InsertedRecords);
        }

        private sealed class FakeCsvReader : ICsvReader
        {
            private readonly CsvReadResult _result;

            public FakeCsvReader(CsvReadResult result)
            {
                _result = result;
            }

            public Task<CsvReadResult> ReadAsync(
                Stream stream,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_result);
            }
        }

        private sealed class FakeStructureValidator : ICsvStructureValidator
        {
            private readonly CsvStructureValidationResult _result;

            public FakeStructureValidator(
                CsvStructureValidationResult result)
            {
                _result = result;
            }

            public CsvStructureValidationResult Validate(
                IReadOnlyCollection<string> headers)
            {
                return _result;
            }
        }

        private sealed class FakeImportedRecordRepository
            : IImportedRecordRepository
        {
            public List<ImportedRecord> InsertedRecords { get; } = [];

            public Task<bool> ExistsByBusinessKeyAsync(
                ImportedRecordKey businessKey,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(false);
            }

            public Task AddRangeAsync(
                IReadOnlyCollection<ImportedRecord> records,
                CancellationToken cancellationToken = default)
            {
                InsertedRecords.AddRange(records);

                return Task.CompletedTask;
            }
        }
    }
}

