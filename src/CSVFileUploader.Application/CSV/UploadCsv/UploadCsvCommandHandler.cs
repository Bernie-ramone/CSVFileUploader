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
            _logger.LogInformation(
                "Starting CSV upload for file {FileName} with size {FileSize} bytes.",
                command.FileName,
                command.FileSize);

            // ---------------------------------------------------------
            // 1. Validate the upload command
            // ---------------------------------------------------------

            var commandValidation =
                await _commandValidator.ValidateAsync(
                    command,
                    cancellationToken);

            if (!commandValidation.IsValid)
            {
                var commandErrors = commandValidation.Errors
                    .Select(error => new CsvUploadError(
                        RowNumber: 0,
                        Message: error.ErrorMessage))
                    .ToArray();

                return new UploadCsvResult(
                    TotalRows: 0,
                    InsertedRows: 0,
                    DuplicateRows: 0,
                    Errors: commandErrors);
            }

            // ---------------------------------------------------------
            // 2. Create upload audit record
            // ---------------------------------------------------------

            var upload = CsvUpload.Start(
                command.FileName,
                DateTimeOffset.UtcNow);

            await _uploadRepository.AddAsync(
                upload,
                cancellationToken);

            // ---------------------------------------------------------
            // 3. Read CSV
            // ---------------------------------------------------------

            var readResult = await _csvReader.ReadAsync(
                command.FileStream,
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            // ---------------------------------------------------------
            // 4. Validate CSV structure
            // ---------------------------------------------------------

            var structureValidation =
                _structureValidator.Validate(
                    readResult.Headers);

            if (!structureValidation.IsValid)
            {
                var structureErrors = structureValidation.Errors
                    .Select(error => new CsvUploadError(
                        RowNumber: 0,
                        Message: error))
                    .ToArray();

                // The file itself could not be processed as a valid CSV
                // structure, so this upload is considered failed.
                upload.MarkAsFailed();

                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);

                return new UploadCsvResult(
                    TotalRows: readResult.Rows.Count,
                    InsertedRows: 0,
                    DuplicateRows: 0,
                    Errors: structureErrors);
            }

            // ---------------------------------------------------------
            // 5. Validate rows and create Domain entities
            // ---------------------------------------------------------

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
                        validationResult.Errors
                            .Select(error => error.ErrorMessage));

                    errors.AddRange(
                        validationResult.Errors.Select(
                            error => new CsvUploadError(
                                RowNumber: row.RowNumber,
                                Message: error.ErrorMessage)));

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
                        Row: row,
                        Record: record));
            }

            // ---------------------------------------------------------
            // 6. Detect duplicates inside the uploaded file
            // ---------------------------------------------------------

            var validRecords = validRows
                .Select(x => x.Record)
                .ToArray();

            var duplicateRecords =
                FindDuplicatesWithinFile(
                    validRecords);

            // ---------------------------------------------------------
            // 7. Detect records that already exist in the database
            // ---------------------------------------------------------

            var businessKeys = validRecords
                .Select(record => record.BusinessKey)
                .ToHashSet();

            var existingKeys =
                await _recordRepository.GetExistingBusinessKeysAsync(
                    businessKeys,
                    cancellationToken);

            // ---------------------------------------------------------
            // 8. Mark duplicate rows and determine what to insert
            // ---------------------------------------------------------

            var recordsToInsert =
                new List<ImportedRecord>();

            foreach (var validatedRow in validRows)
            {
                var record = validatedRow.Record;

                var isDuplicateInFile =
                    duplicateRecords.Contains(record);

                var existsInDatabase =
                    existingKeys.Contains(
                        record.BusinessKey);

                if (isDuplicateInFile ||
                    existsInDatabase)
                {
                    record.MarkAsDuplicate();

                    upload.AddRow(
                        CsvUploadRow.Duplicate(
                            validatedRow.Row.RowNumber,
                            validatedRow.Row.RecordId));

                    continue;
                }

                recordsToInsert.Add(record);

                upload.AddRow(
                    CsvUploadRow.Imported(
                        validatedRow.Row.RowNumber,
                        validatedRow.Row.RecordId));
            }

            // ---------------------------------------------------------
            // 9. Calculate final upload statistics
            // ---------------------------------------------------------

            var duplicateCount =
                validRecords.Length -
                recordsToInsert.Count;

            var errorRowCount = errors
                .Select(error => error.RowNumber)
                .Where(rowNumber => rowNumber > 0)
                .Distinct()
                .Count();

            // ---------------------------------------------------------
            // 10. Complete upload audit
            // ---------------------------------------------------------

            upload.Complete(
                totalRows: readResult.Rows.Count,
                insertedRows: recordsToInsert.Count,
                duplicateRows: duplicateCount,
                errorRows: errorRowCount);

            // ---------------------------------------------------------
            // 11. Stage imported records
            // ---------------------------------------------------------

            if (recordsToInsert.Count > 0)
            {
                await _recordRepository.AddRangeAsync(
                    recordsToInsert,
                    cancellationToken);
            }

            // ---------------------------------------------------------
            // 12. Persist everything in one Unit of Work
            // ---------------------------------------------------------

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            // ---------------------------------------------------------
            // 13. Log result
            // ---------------------------------------------------------

            _logger.LogInformation(
                "CSV upload completed for file {FileName}. " +
                "TotalRows={TotalRows}, " +
                "InsertedRows={InsertedRows}, " +
                "DuplicateRows={DuplicateRows}, " +
                "ErrorRows={ErrorRows}.",
                command.FileName,
                readResult.Rows.Count,
                recordsToInsert.Count,
                duplicateCount,
                errorRowCount);

            // ---------------------------------------------------------
            // 14. Return result
            // ---------------------------------------------------------

            return new UploadCsvResult(
                TotalRows: readResult.Rows.Count,
                InsertedRows: recordsToInsert.Count,
                DuplicateRows: duplicateCount,
                Errors: errors);
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

        private static HashSet<ImportedRecord>
            FindDuplicatesWithinFile(
                IReadOnlyCollection<ImportedRecord> records)
        {
            var seenKeys =
                new HashSet<
                    CSVFileUploader.Domain.ValueObjects.ImportedRecordKey>();

            var duplicateRecords =
                new HashSet<ImportedRecord>();

            foreach (var record in records)
            {
                if (!seenKeys.Add(record.BusinessKey))
                {
                    duplicateRecords.Add(record);
                }
            }

            return duplicateRecords;
        }
    }
}