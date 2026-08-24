using CSVFileUploader.Application.Common.Interfaces;
using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Application.DTOs;
using CSVFileUploader.Domain.Entities;
using FluentValidation;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CSVFileUploader.Application.CSV.UploadCsv
{
    public sealed class UploadCsvCommandHandler
    {
        private readonly ICsvReader _csvReader;
        private readonly ICsvStructureValidator _structureValidator;
        private readonly IImportedRecordRepository _recordRepository;
        private readonly IUploadRepository _uploadRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CsvRowDto> _rowValidator;
        private readonly IValidator<UploadCsvCommand> _commandValidator;
        private readonly ILogger<UploadCsvCommandHandler> _logger;

        public UploadCsvCommandHandler(
            ICsvReader csvReader,
            ICsvStructureValidator structureValidator,
            IImportedRecordRepository recordRepository,
            IUploadRepository uploadRepository,
            IUnitOfWork unitOfWork,
            IValidator<CsvRowDto> rowValidator,
            IValidator<UploadCsvCommand> commandValidator,
            ILogger<UploadCsvCommandHandler> logger)
        {
            _csvReader = csvReader;
            _structureValidator = structureValidator;
            _recordRepository = recordRepository;
            _uploadRepository = uploadRepository;
            _unitOfWork = unitOfWork;
            _rowValidator = rowValidator;
            _commandValidator = commandValidator;
            _logger = logger;
        }

        public async Task<UploadCsvResult> HandleAsync(
            UploadCsvCommand command,
            CancellationToken cancellationToken = default)
        {
            var commandValidation =
                await _commandValidator.ValidateAsync(
                    command,
                    cancellationToken);

            if (!commandValidation.IsValid)
            {
                var commandErrors = commandValidation.Errors
                    .Select(error => new CsvUploadError(
                        0,
                        error.ErrorMessage))
                    .ToArray();

                return new UploadCsvResult(
                    0,
                    0,
                    0,
                    commandErrors);
            }

            var upload = CsvUpload.Start(
                command.FileName,
                DateTimeOffset.UtcNow);

            _logger.LogInformation(
                "Starting CSV upload {UploadId} for file {FileName} with size {FileSize} bytes.",
                upload.Id,
                command.FileName,
                command.FileSize);

            await _uploadRepository.AddAsync(
                upload,
                cancellationToken);

            var readResult = await _csvReader.ReadAsync(
                command.FileStream,
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var structureValidation =
                _structureValidator.Validate(
                    readResult.Headers);

            if (!structureValidation.IsValid)
            {
                var structureErrors = structureValidation.Errors
                    .Select(error => new CsvUploadError(
                        0,
                        error))
                    .ToArray();

                upload.MarkAsFailed();

                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);

                _logger.LogWarning(
                    "CSV upload {UploadId} failed CSV structure validation for file {FileName}.",
                    upload.Id,
                    command.FileName);

                return new UploadCsvResult(
                    readResult.Rows.Count,
                    0,
                    0,
                    structureErrors);
            }

            var validRows = new List<ValidatedCsvRow>();

            var errors = new List<CsvUploadError>();

            foreach (var row in readResult.Rows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var validationResult =
                    await _rowValidator.ValidateAsync(
                        row,
                        cancellationToken);

                if (!validationResult.IsValid)
                {
                    var errorMessage = string.Join(
                        "; ",
                        validationResult.Errors.Select(
                            error => error.ErrorMessage));

                    errors.AddRange(
                        validationResult.Errors.Select(
                            error => new CsvUploadError(
                                row.RowNumber,
                                error.ErrorMessage)));

                    upload.AddRow(
                        CsvUploadRow.Invalid(
                            row.RowNumber,
                            row.RecordId,
                            errorMessage));

                    continue;
                }

                var record = CreateDomainRecord(row);

                validRows.Add(
                    new ValidatedCsvRow(
                        row,
                        record));
            }

            var validRecords = validRows
                .Select(x => x.Record)
                .ToArray();

            // Duplicate detection inside THIS uploaded file.
            // The index identifies the later occurrence, so
            // the first occurrence remains eligible for insertion.
            var duplicateIndexes =
                FindDuplicateIndexesWithinFile(
                    validRows);

            // Duplicate detection against records already in DB.
            var businessKeys = validRecords
                .Select(record => record.BusinessKey)
                .ToHashSet();

            var existingKeys =
                await _recordRepository.GetExistingBusinessKeysAsync(
                    businessKeys,
                    cancellationToken);

            var recordsToInsert =
                new List<ImportedRecord>();

            for (var index = 0;
                 index < validRows.Count;
                 index++)
            {
                var validatedRow = validRows[index];

                var record = validatedRow.Record;

                var isDuplicateInFile =
                    duplicateIndexes.Contains(index);

                var existsInDatabase =
                    existingKeys.Contains(
                        record.BusinessKey);

                if (isDuplicateInFile ||
                    existsInDatabase)
                {
                    record.MarkAsDuplicate();

                    var duplicateReason =
                        isDuplicateInFile
                            ? "Duplicate row in uploaded file."
                            : "Record already exists in the database.";

                    upload.AddRow(
                        CsvUploadRow.Duplicate(
                            validatedRow.Row.RowNumber,
                            validatedRow.Row.RecordId,
                            duplicateReason));

                    continue;
                }

                recordsToInsert.Add(record);

                upload.AddRow(
                    CsvUploadRow.Imported(
                        validatedRow.Row.RowNumber,
                        validatedRow.Row.RecordId));
            }

            var duplicateCount =
                validRecords.Length -
                recordsToInsert.Count;

            var errorRowCount = errors
                .Select(error => error.RowNumber)
                .Where(rowNumber => rowNumber > 0)
                .Distinct()
                .Count();

            upload.Complete(
                readResult.Rows.Count,
                recordsToInsert.Count,
                duplicateCount,
                errorRowCount);

            if (recordsToInsert.Count > 0)
            {
                await _recordRepository.AddRangeAsync(
                    recordsToInsert,
                    cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            _logger.LogInformation(
                "CSV upload {UploadId} completed for file {FileName}. " +
                "TotalRows={TotalRows}, " +
                "InsertedRows={InsertedRows}, " +
                "DuplicateRows={DuplicateRows}, " +
                "ErrorRows={ErrorRows}.",
                upload.Id,
                command.FileName,
                readResult.Rows.Count,
                recordsToInsert.Count,
                duplicateCount,
                errorRowCount);

            return new UploadCsvResult(
                readResult.Rows.Count,
                recordsToInsert.Count,
                duplicateCount,
                errors);
        }

        private static ImportedRecord CreateDomainRecord(
            CsvRowDto row)
        {
            var eventDate = DateOnly.ParseExact(
                row.EventDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture);

            var volume = decimal.Parse(
                row.Volume,
                NumberStyles.Number,
                CultureInfo.InvariantCulture);

            return ImportedRecord.Create(
                row.RecordId,
                row.AssetId,
                row.SourceSite,
                row.DestinationSite,
                eventDate,
                volume,
                row.Unit,
                row.Notes);
        }

        private static HashSet<int>
            FindDuplicateIndexesWithinFile(
                IReadOnlyList<ValidatedCsvRow> rows)
        {
            var seenKeys =
                new HashSet<
                    CSVFileUploader.Domain.ValueObjects.ImportedRecordKey>();

            var duplicateIndexes =
                new HashSet<int>();

            for (var index = 0;
                 index < rows.Count;
                 index++)
            {
                var businessKey =
                    rows[index].Record.BusinessKey;

                if (!seenKeys.Add(businessKey))
                {
                    duplicateIndexes.Add(index);
                }
            }

            return duplicateIndexes;
        }
    }
}