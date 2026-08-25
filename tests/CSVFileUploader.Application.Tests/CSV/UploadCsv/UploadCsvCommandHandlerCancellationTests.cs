using CSVFileUploader.Application.Common.Interfaces;
using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Application.CSV.UploadCsv;
using CSVFileUploader.Application.CSV.Validators;
using CSVFileUploader.Application.DTOs;
using CSVFileUploader.Domain.Entities;
using CSVFileUploader.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging.Abstractions;

namespace CSVFileUploader.Application.Tests.CSV.UploadCsv
{

    public sealed class UploadCsvCommandHandlerCancellationTests
    {
        [Fact]
        public async Task HandleAsync_WhenCsvReadIsCancelled_ShouldPropagateCancellation()
        {
            using var cancellationTokenSource =
                new CancellationTokenSource();

            var cancellationToken =
                cancellationTokenSource.Token;

            var csvReader =
                new CancellingCsvReader(
                    cancellationToken);

            var uploadRepository =
                new FakeUploadRepository();

            var recordRepository =
                new FakeImportedRecordRepository();

            var unitOfWork =
                new FakeUnitOfWork();

            var handler =
                CreateHandler(
                    csvReader,
                    recordRepository,
                    uploadRepository,
                    unitOfWork);

            await using var stream =
                new MemoryStream();

            var command =
                new UploadCsvCommand(
                    stream,
                    "cancelled.csv",
                    "text/csv",
                    100);

            await Assert.ThrowsAsync<OperationCanceledException>(
                () => handler.HandleAsync(
                    command,
                    cancellationToken));

            Assert.Empty(
                uploadRepository.Uploads);

            Assert.Empty(
                recordRepository.InsertedRecords);

            Assert.Equal(
                0,
                unitOfWork.TransactionCalls);

            Assert.Equal(
                0,
                unitOfWork.SaveChangesCalls);
        }

        [Fact]
        public async Task HandleAsync_WhenCancellationOccursDuringRowValidation_ShouldPropagateCancellation()
        {
            using var cancellationTokenSource =
                new CancellationTokenSource();

            var csvReader =
                new SuccessfulCsvReader(
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
                            "Test")
                        ]));

            var rowValidator =
                new CancellingRowValidator(
                    cancellationTokenSource);

            var uploadRepository =
                new FakeUploadRepository();

            var recordRepository =
                new FakeImportedRecordRepository();

            var unitOfWork =
                new FakeUnitOfWork();

            var handler =
                new UploadCsvCommandHandler(
                    csvReader,
                    new SuccessfulStructureValidator(),
                    recordRepository,
                    uploadRepository,
                    unitOfWork,
                    rowValidator,
                    new UploadCsvCommandValidator(
                        new CsvUploadOptions()),
                    NullLogger<UploadCsvCommandHandler>.Instance);

            await using var stream =
                new MemoryStream();

            var command =
                new UploadCsvCommand(
                    stream,
                    "cancelled-validation.csv",
                    "text/csv",
                    100);

            await Assert.ThrowsAsync<OperationCanceledException>(
                () => handler.HandleAsync(
                    command,
                    cancellationTokenSource.Token));

            Assert.Empty(
                uploadRepository.Uploads);

            Assert.Empty(
                recordRepository.InsertedRecords);

            Assert.Equal(
                0,
                unitOfWork.TransactionCalls);

            Assert.Equal(
                0,
                unitOfWork.SaveChangesCalls);
        }

        private static UploadCsvCommandHandler CreateHandler(
            ICsvReader csvReader,
            IImportedRecordRepository recordRepository,
            IUploadRepository uploadRepository,
            IUnitOfWork unitOfWork)
        {
            return new UploadCsvCommandHandler(
                csvReader,
                new SuccessfulStructureValidator(),
                recordRepository,
                uploadRepository,
                unitOfWork,
                new CsvRowValidator(),
                new UploadCsvCommandValidator(
                    new CsvUploadOptions()),
                NullLogger<UploadCsvCommandHandler>.Instance);
        }

        private sealed class CancellingCsvReader
            : ICsvReader
        {
            private readonly CancellationToken _cancellationToken;

            public CancellingCsvReader(
                CancellationToken cancellationToken)
            {
                _cancellationToken = cancellationToken;
            }

            public Task<CsvReadResult> ReadAsync(
                Stream stream,
                CancellationToken cancellationToken = default)
            {
                throw new OperationCanceledException(
                    _cancellationToken);
            }
        }

        private sealed class SuccessfulCsvReader
            : ICsvReader
        {
            private readonly CsvReadResult _result;

            public SuccessfulCsvReader(
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

        private sealed class SuccessfulStructureValidator
            : ICsvStructureValidator
        {
            public CsvStructureValidationResult Validate(
                IReadOnlyCollection<string> headers)
            {
                return CsvStructureValidationResult.Success();
            }
        }

        private sealed class CancellingRowValidator
            : IValidator<CsvRowDto>
        {
            private readonly CancellationTokenSource _cts;

            public CancellingRowValidator(
                CancellationTokenSource cts)
            {
                _cts = cts;
            }

            public ValidationResult Validate(
                CsvRowDto instance)
            {
                _cts.Cancel();

                throw new OperationCanceledException(
                    _cts.Token);
            }

            public Task<ValidationResult> ValidateAsync(
                CsvRowDto instance,
                CancellationToken cancellation = default)
            {
                _cts.Cancel();

                throw new OperationCanceledException(
                    _cts.Token);
            }

            public ValidationResult Validate(
                IValidationContext context)
            {
                _cts.Cancel();

                throw new OperationCanceledException(
                    _cts.Token);
            }

            public Task<ValidationResult> ValidateAsync(
                IValidationContext context,
                CancellationToken cancellation = default)
            {
                _cts.Cancel();

                throw new OperationCanceledException(
                    _cts.Token);
            }

            public IValidatorDescriptor CreateDescriptor()
            {
                throw new NotSupportedException();
            }

            public bool CanValidateInstancesOfType(
                Type type)
            {
                return type == typeof(CsvRowDto);
            }
        }

        private sealed class FakeImportedRecordRepository
            : IImportedRecordRepository
        {
            public List<ImportedRecord> InsertedRecords { get; } = [];

            public Task<IReadOnlyCollection<ImportedRecordKey>>
                GetExistingBusinessKeysAsync(
                    IReadOnlyCollection<ImportedRecordKey> businessKeys,
                    CancellationToken cancellationToken = default)
            {
                return Task.FromResult<
                    IReadOnlyCollection<ImportedRecordKey>>(
                        []);
            }

            public Task AddRangeAsync(
                IReadOnlyCollection<ImportedRecord> records,
                CancellationToken cancellationToken = default)
            {
                InsertedRecords.AddRange(records);

                return Task.CompletedTask;
            }
        }

        private sealed class FakeUploadRepository
            : IUploadRepository
        {
            public List<CsvUpload> Uploads { get; } = [];

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
                return Task.FromResult<CsvUpload?>(
                    null);
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