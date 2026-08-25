using CSVFileUploader.Application.Common.Interfaces;
using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Application.CSV.UploadCsv;
using CSVFileUploader.Application.CSV.Validators;
using CSVFileUploader.Application.DTOs;
using CSVFileUploader.Domain.Entities;
using CSVFileUploader.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;

namespace CSVFileUploader.Application.Tests.CSV.UploadCsv
{

    public class UploadCsvCommandHandlerTests
    {
        [Fact]
        public async Task HandleAsync_WithValidRows_ShouldInsertRecords()
        {
            var csvReader =
                new FakeCsvReader(
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
                            2,
                            "REC-0001",
                            "AST-1001",
                            "MINE-NORTH",
                            "PLANT-A",
                            "2026-08-01",
                            "125.50",
                            "TON",
                            "Morning shift")
                        ]));

            var structureValidator =
                new FakeStructureValidator(
                    CsvStructureValidationResult.Success());

            var recordRepository =
                new FakeImportedRecordRepository();

            var uploadRepository =
                new FakeUploadRepository();

            var unitOfWork =
                new FakeUnitOfWork();

            var handler =
                CreateHandler(
                    csvReader,
                    structureValidator,
                    recordRepository,
                    uploadRepository,
                    unitOfWork);

            await using var stream =
                new MemoryStream();

            var result =
                await handler.HandleAsync(
                    new UploadCsvCommand(
                        stream,
                        "test.csv",
                        "text/csv",
                        100));

            Assert.Equal(1, result.TotalRows);
            Assert.Equal(1, result.InsertedRows);
            Assert.Equal(0, result.DuplicateRows);
            Assert.Empty(result.Errors);

            Assert.Single(
                recordRepository.InsertedRecords);

            var upload =
                Assert.Single(
                    uploadRepository.Uploads);

            Assert.Equal(1, upload.TotalRows);
            Assert.Equal(1, upload.InsertedRows);
            Assert.Equal(0, upload.DuplicateRows);
            Assert.Equal(0, upload.ErrorRows);
            Assert.Single(upload.Rows);

            Assert.Equal(
                1,
                unitOfWork.TransactionCalls);
        }

        [Fact]
        public async Task HandleAsync_WithInvalidStructure_ShouldReturnErrors()
        {
            var csvReader =
                new FakeCsvReader(
                    new CsvReadResult(
                        ["RecordId"],
                        []));

            var structureValidator =
                new FakeStructureValidator(
                    CsvStructureValidationResult.Failure(
                        "Missing required columns."));

            var recordRepository =
                new FakeImportedRecordRepository();

            var uploadRepository =
                new FakeUploadRepository();

            var unitOfWork =
                new FakeUnitOfWork();

            var handler =
                CreateHandler(
                    csvReader,
                    structureValidator,
                    recordRepository,
                    uploadRepository,
                    unitOfWork);

            await using var stream =
                new MemoryStream();

            var result =
                await handler.HandleAsync(
                    new UploadCsvCommand(
                        stream,
                        "test.csv",
                        "text/csv",
                        100));

            Assert.Equal(
                0,
                result.InsertedRows);

            Assert.Single(
                result.Errors);

            Assert.Empty(
                recordRepository.InsertedRecords);

            var upload =
                Assert.Single(
                    uploadRepository.Uploads);

            Assert.Equal(
                Domain.Enums.CsvUploadStatus.Failed,
                upload.Status);

            Assert.Equal(
                1,
                unitOfWork.SaveChangesCalls);
        }

        [Fact]
        public async Task HandleAsync_WithInvalidRow_ShouldReturnErrorAndNotInsertRecord()
        {
            var csvReader =
                new FakeCsvReader(
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
                            2,
                            "REC-0001",
                            "INVALID",
                            "MINE-NORTH",
                            "PLANT-A",
                            "2026-08-01",
                            "125.50",
                            "TON",
                            "Invalid asset")
                        ]));

            var structureValidator =
                new FakeStructureValidator(
                    CsvStructureValidationResult.Success());

            var recordRepository =
                new FakeImportedRecordRepository();

            var uploadRepository =
                new FakeUploadRepository();

            var unitOfWork =
                new FakeUnitOfWork();

            var handler =
                CreateHandler(
                    csvReader,
                    structureValidator,
                    recordRepository,
                    uploadRepository,
                    unitOfWork);

            await using var stream =
                new MemoryStream();

            var result =
                await handler.HandleAsync(
                    new UploadCsvCommand(
                        stream,
                        "test.csv",
                        "text/csv",
                        100));

            Assert.Equal(
                1,
                result.TotalRows);

            Assert.Equal(
                0,
                result.InsertedRows);

            Assert.Equal(
                0,
                result.DuplicateRows);

            var error =
                Assert.Single(
                    result.Errors);

            Assert.Equal(
                2,
                error.RowNumber);

            Assert.Empty(
                recordRepository.InsertedRecords);

            var upload =
                Assert.Single(
                    uploadRepository.Uploads);

            Assert.Equal(
                1,
                upload.ErrorRows);

            Assert.Single(
                upload.Rows);

            Assert.Equal(
                Domain.Enums.CsvUploadRowStatus.Invalid,
                upload.Rows.First().Status);
        }

        [Fact]
        public async Task HandleAsync_WithDuplicateRowsInFile_ShouldKeepFirstOccurrence()
        {
            var csvReader =
                new FakeCsvReader(
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
                            2,
                            "REC-0012",
                            "AST-1004",
                            "MINE-EAST",
                            "PLANT-C",
                            "2026-08-05",
                            "145.50",
                            "TON",
                            "First"),

                        new CsvRowDto(
                            3,
                            "REC-0013",
                            "AST-1004",
                            "MINE-EAST",
                            "PLANT-C",
                            "2026-08-05",
                            "145.50",
                            "TON",
                            "Duplicate")
                        ]));

            var recordRepository =
                new FakeImportedRecordRepository();

            var uploadRepository =
                new FakeUploadRepository();

            var unitOfWork =
                new FakeUnitOfWork();

            var handler =
                CreateHandler(
                    csvReader,
                    new FakeStructureValidator(
                        CsvStructureValidationResult.Success()),
                    recordRepository,
                    uploadRepository,
                    unitOfWork);

            await using var stream =
                new MemoryStream();

            var result =
                await handler.HandleAsync(
                    new UploadCsvCommand(
                        stream,
                        "test.csv",
                        "text/csv",
                        100));

            Assert.Equal(
                2,
                result.TotalRows);

            Assert.Equal(
                1,
                result.InsertedRows);

            Assert.Equal(
                1,
                result.DuplicateRows);

            Assert.Empty(
                result.Errors);

            Assert.Single(
                recordRepository.InsertedRecords);

            var upload =
                Assert.Single(
                    uploadRepository.Uploads);

            Assert.Equal(
                1,
                upload.InsertedRows);

            Assert.Equal(
                1,
                upload.DuplicateRows);

            Assert.Equal(
                2,
                upload.Rows.Count);

            Assert.Equal(
                Domain.Enums.CsvUploadRowStatus.Imported,
                upload.Rows
                    .OrderBy(x => x.RowNumber)
                    .First()
                    .Status);

            Assert.Equal(
                Domain.Enums.CsvUploadRowStatus.Duplicate,
                upload.Rows
                    .OrderBy(x => x.RowNumber)
                    .Last()
                    .Status);

            Assert.Equal(
                1,
                recordRepository.GetExistingBusinessKeysCalls);
        }

        [Fact]
        public async Task HandleAsync_WithExistingDatabaseRecord_ShouldMarkAsDuplicate()
        {
            var existingKey =
                new ImportedRecordKey(
                    "AST-1004",
                    "MINE-EAST",
                    "PLANT-C",
                    new DateOnly(2026, 8, 5),
                    145.50m);

            var recordRepository =
                new FakeImportedRecordRepository();

            recordRepository.ExistingKeys.Add(
                existingKey);

            var uploadRepository =
                new FakeUploadRepository();

            var unitOfWork =
                new FakeUnitOfWork();

            var handler =
                CreateHandler(
                    new FakeCsvReader(
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
                                2,
                                "REC-0021",
                                "AST-1004",
                                "MINE-EAST",
                                "PLANT-C",
                                "2026-08-05",
                                "145.50",
                                "TON",
                                "Already exists")
                            ])),
                    new FakeStructureValidator(
                        CsvStructureValidationResult.Success()),
                    recordRepository,
                    uploadRepository,
                    unitOfWork);

            await using var stream =
                new MemoryStream();

            var result =
                await handler.HandleAsync(
                    new UploadCsvCommand(
                        stream,
                        "test.csv",
                        "text/csv",
                        100));

            Assert.Equal(
                1,
                result.TotalRows);

            Assert.Equal(
                0,
                result.InsertedRows);

            Assert.Equal(
                1,
                result.DuplicateRows);

            Assert.Empty(
                result.Errors);

            Assert.Empty(
                recordRepository.InsertedRecords);

            var upload =
                Assert.Single(
                    uploadRepository.Uploads);

            Assert.Equal(
                Domain.Enums.CsvUploadRowStatus.Duplicate,
                Assert.Single(upload.Rows).Status);

            Assert.Equal(
                1,
                unitOfWork.TransactionCalls);
        }

        [Fact]
        public async Task HandleAsync_WithMixedRows_ShouldReturnCorrectSummary()
        {
            var existingKey =
                new ImportedRecordKey(
                    "AST-1004",
                    "MINE-EAST",
                    "PLANT-C",
                    new DateOnly(2026, 8, 5),
                    145.50m);

            var recordRepository =
                new FakeImportedRecordRepository();

            recordRepository.ExistingKeys.Add(
                existingKey);

            var uploadRepository =
                new FakeUploadRepository();

            var unitOfWork =
                new FakeUnitOfWork();

            var handler =
                CreateHandler(
                    new FakeCsvReader(
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
                                2,
                                "REC-0001",
                                "AST-1001",
                                "MINE-NORTH",
                                "PLANT-A",
                                "2026-08-01",
                                "125.50",
                                "TON",
                                "Valid"),

                            new CsvRowDto(
                                3,
                                "REC-0002",
                                "AST-1002",
                                "MINE-NORTH",
                                "PLANT-A",
                                "2026-08-01",
                                "118.25",
                                "TON",
                                "Valid"),

                            new CsvRowDto(
                                4,
                                "REC-0003",
                                "AST-1004",
                                "MINE-EAST",
                                "PLANT-C",
                                "2026-08-05",
                                "145.50",
                                "TON",
                                "Existing"),

                            new CsvRowDto(
                                5,
                                "REC-0004",
                                "AST-1004",
                                "MINE-EAST",
                                "PLANT-C",
                                "2026-08-05",
                                "145.50",
                                "TON",
                                "Duplicate"),

                            new CsvRowDto(
                                6,
                                "REC-0005",
                                "INVALID",
                                "MINE-NORTH",
                                "PLANT-A",
                                "2026-08-01",
                                "100.00",
                                "TON",
                                "Invalid")
                            ])),
                    new FakeStructureValidator(
                        CsvStructureValidationResult.Success()),
                    recordRepository,
                    uploadRepository,
                    unitOfWork);

            await using var stream =
                new MemoryStream();

            var result =
                await handler.HandleAsync(
                    new UploadCsvCommand(
                        stream,
                        "test.csv",
                        "text/csv",
                        100));

            Assert.Equal(
                5,
                result.TotalRows);

            Assert.Equal(
                2,
                result.InsertedRows);

            Assert.Equal(
                2,
                result.DuplicateRows);

            Assert.Single(
                result.Errors);

            Assert.Equal(
                6,
                Assert.Single(result.Errors).RowNumber);

            Assert.Equal(
                2,
                recordRepository.InsertedRecords.Count);

            Assert.Equal(
                1,
                recordRepository.GetExistingBusinessKeysCalls);

            var upload =
                Assert.Single(
                    uploadRepository.Uploads);

            Assert.Equal(
                5,
                upload.TotalRows);

            Assert.Equal(
                2,
                upload.InsertedRows);

            Assert.Equal(
                2,
                upload.DuplicateRows);

            Assert.Equal(
                1,
                upload.ErrorRows);

            Assert.Equal(
                5,
                upload.Rows.Count);

            Assert.Equal(
                1,
                unitOfWork.TransactionCalls);
        }

        [Fact]
        public async Task HandleAsync_WithFiveThousandRows_ShouldPerformOneBatchDuplicateLookupAndOneBatchInsert()
        {
            const int rowCount = 5_000;

            var rows =
                new List<CsvRowDto>(
                    capacity: rowCount);

            for (var index = 1;
                 index <= rowCount;
                 index++)
            {
                var assetNumber =
                    (index - 1) % 9_999 + 1;

                rows.Add(
                    new CsvRowDto(
                        RowNumber: index + 1,
                        RecordId: $"REC-{index % 10_000:D4}",
                        AssetId: $"AST-{assetNumber:D4}",
                        SourceSite: "MINE-NORTH",
                        DestinationSite: "PLANT-A",
                        EventDate: "2026-08-01",
                        Volume: "100.00",
                        Unit: "TON",
                        Notes: "Batch performance test"));
            }

            var csvReader =
                new FakeCsvReader(
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
                        rows));

            var structureValidator =
                new FakeStructureValidator(
                    CsvStructureValidationResult.Success());

            var recordRepository =
                new FakeImportedRecordRepository();

            var uploadRepository =
                new FakeUploadRepository();

            var unitOfWork =
                new FakeUnitOfWork();

            var handler =
                CreateHandler(
                    csvReader,
                    structureValidator,
                    recordRepository,
                    uploadRepository,
                    unitOfWork);

            await using var stream =
                new MemoryStream();

            var stopwatch =
                System.Diagnostics.Stopwatch.StartNew();

            var result =
                await handler.HandleAsync(
                    new UploadCsvCommand(
                        stream,
                        "large-batch-test.csv",
                        "text/csv",
                        500_000));

            stopwatch.Stop();

            Assert.Equal(
                rowCount,
                result.TotalRows);

            Assert.Equal(
                rowCount,
                result.InsertedRows);

            Assert.Equal(
                0,
                result.DuplicateRows);

            Assert.Empty(
                result.Errors);

            Assert.Equal(
                1,
                recordRepository.GetExistingBusinessKeysCalls);

            Assert.Equal(
                rowCount,
                recordRepository.InsertedRecords.Count);

            Assert.Equal(
                1,
                recordRepository.AddRangeCalls);

            Assert.Equal(
                1,
                unitOfWork.TransactionCalls);

            Assert.True(
                stopwatch.Elapsed < TimeSpan.FromSeconds(30),
                $"Processing {rowCount:N0} rows took " +
                $"{stopwatch.Elapsed.TotalSeconds:F2} seconds.");
        }

        private static UploadCsvCommandHandler CreateHandler(
            ICsvReader csvReader,
            ICsvStructureValidator structureValidator,
            IImportedRecordRepository recordRepository,
            IUploadRepository uploadRepository,
            IUnitOfWork unitOfWork)
        {
            return new UploadCsvCommandHandler(
                csvReader,
                structureValidator,
                recordRepository,
                uploadRepository,
                unitOfWork,
                new CsvRowValidator(),
                new UploadCsvCommandValidator(
                    new CsvUploadOptions()),
                NullLogger<UploadCsvCommandHandler>.Instance);
        }

        private sealed class FakeCsvReader : ICsvReader
        {
            private readonly CsvReadResult _result;

            public FakeCsvReader(
                CsvReadResult result)
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

        private sealed class FakeStructureValidator
            : ICsvStructureValidator
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

            public HashSet<ImportedRecordKey> ExistingKeys { get; } = [];

            public int GetExistingBusinessKeysCalls
            {
                get;
                private set;
            }

            public int AddRangeCalls
            {
                get;
                private set;
            }

            public Task<IReadOnlyCollection<ImportedRecordKey>>
                GetExistingBusinessKeysAsync(
                    IReadOnlyCollection<ImportedRecordKey> businessKeys,
                    CancellationToken cancellationToken = default)
            {
                GetExistingBusinessKeysCalls++;

                var result =
                    businessKeys
                        .Where(ExistingKeys.Contains)
                        .ToArray();

                return Task.FromResult<
                    IReadOnlyCollection<ImportedRecordKey>>(
                        result);
            }

            public Task AddRangeAsync(
                IReadOnlyCollection<ImportedRecord> records,
                CancellationToken cancellationToken = default)
            {
                AddRangeCalls++;

                InsertedRecords.AddRange(
                    records);

                return Task.CompletedTask;
            }
        }

        private sealed class FakeUploadRepository
            : IUploadRepository
        {
            public List<CsvUpload> Uploads { get; } = [];

            public int GetSuccessfulUploadCalls
            {
                get;
                private set;
            }

            public Task AddAsync(
                CsvUpload upload,
                CancellationToken cancellationToken = default)
            {
                Uploads.Add(upload);

                return Task.CompletedTask;
            }

            public Task<CsvUpload?> GetByIdAsync(
                Guid id,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(
                    Uploads.FirstOrDefault(
                        upload => upload.Id == id));
            }

            public Task<CsvUpload?>
                GetSuccessfulUploadByFileHashAsync(
                    string fileHash,
                    CancellationToken cancellationToken = default)
            {
                GetSuccessfulUploadCalls++;

                var upload =
                    Uploads
                        .Where(upload =>
                            string.Equals(
                                upload.FileHash,
                                fileHash,
                                StringComparison.OrdinalIgnoreCase))
                        .Where(upload =>
                            upload.Status ==
                                Domain.Enums.CsvUploadStatus.Completed ||
                            upload.Status ==
                                Domain.Enums.CsvUploadStatus.CompletedWithErrors)
                        .OrderByDescending(
                            upload => upload.UploadedAtUtc)
                        .FirstOrDefault();

                return Task.FromResult(
                    upload);
            }
        }

        private sealed class FakeUnitOfWork
            : IUnitOfWork
        {
            public int TransactionCalls
            {
                get;
                private set;
            }

            public int SaveChangesCalls
            {
                get;
                private set;
            }

            public async Task ExecuteInTransactionAsync(
                Func<CancellationToken, Task> operation,
                CancellationToken cancellationToken = default)
            {
                TransactionCalls++;

                await operation(
                    cancellationToken);
            }

            public Task<int> SaveChangesAsync(
                CancellationToken cancellationToken = default)
            {
                SaveChangesCalls++;

                return Task.FromResult(1);
            }
        }
    }
}