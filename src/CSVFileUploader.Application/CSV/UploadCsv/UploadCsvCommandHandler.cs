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
        private readonly IImportedRecordRepository _repository;
        private readonly IValidator<CsvRowDto> _rowValidator;
        private readonly IValidator<UploadCsvCommand> _commandValidator;
        private readonly ILogger<UploadCsvCommandHandler> _logger;



        public UploadCsvCommandHandler(
            ICsvReader csvReader,
            ICsvStructureValidator structureValidator,
            IImportedRecordRepository repository,
            IValidator<CsvRowDto> rowValidator,
            IValidator<UploadCsvCommand> commandValidator,
            ILogger<UploadCsvCommandHandler> logger)
        {
            _csvReader = csvReader;
            _structureValidator = structureValidator;
            _repository = repository;
            _rowValidator = rowValidator;
            _commandValidator = commandValidator;
            _logger = logger;
        }

        public async Task<UploadCsvResult> HandleAsync(UploadCsvCommand command, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Starting CSV upload for file {FileName} with size {FileSize} bytes.", 
                command.FileName, 
                command.FileSize);
            
            var commandValidation = await _commandValidator.ValidateAsync(command, cancellationToken);

            if (!commandValidation.IsValid)
            {
                var commandErrors = commandValidation.Errors
                    .Select(error => new CsvUploadError(
                        0,
                        error.ErrorMessage))
                    .ToArray();

                return new UploadCsvResult(
                    TotalRows: 0,
                    InsertedRows: 0,
                    DuplicateRows: 0,
                    Errors: commandErrors);
            }

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

                return new UploadCsvResult(
                    TotalRows: 0,
                    InsertedRows: 0,
                    DuplicateRows: 0,
                    Errors: structureErrors);
            }

            var validRecords = new List<ImportedRecord>();
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
                    errors.AddRange(
                        validationResult.Errors.Select(
                            error => new CsvUploadError(
                                row.RowNumber,
                                error.ErrorMessage)));

                    continue;
                }

                validRecords.Add(
                    CreateDomainRecord(row));
            }

            if (validRecords.Count == 0)
            {
                return new UploadCsvResult(
                    TotalRows: readResult.Rows.Count,
                    InsertedRows: 0,
                    DuplicateRows: 0,
                    Errors: errors);
            }

            var duplicateRecords =
    FindDuplicatesWithinFile(validRecords);

            var businessKeys = validRecords
                .Select(record => record.BusinessKey)
                .ToHashSet();

            var existingKeys =
                await _repository.GetExistingBusinessKeysAsync(
                    businessKeys,
                    cancellationToken);

            var recordsToInsert =
                new List<ImportedRecord>();

            foreach (var record in validRecords)
            {
                var isDuplicateInFile =
                    duplicateRecords.Contains(record);

                var existsInDatabase =
                    existingKeys.Contains(record.BusinessKey);

                if (isDuplicateInFile || existsInDatabase)
                {
                    record.MarkAsDuplicate();
                    continue;
                }

                recordsToInsert.Add(record);
            }

            if (recordsToInsert.Count > 0)
            {
                await _repository.AddRangeAsync(
                    recordsToInsert,
                    cancellationToken);
            }

            var duplicateCount =
                validRecords.Count -
                recordsToInsert.Count;

            _logger.LogInformation(
                "CSV upload completed for file {FileName}. " +
                "TotalRows={TotalRows}, InsertedRows={InsertedRows}, " +
                "DuplicateRows={DuplicateRows}, ErrorRows={ErrorRows}.",
                command.FileName,
                readResult.Rows.Count,
                recordsToInsert.Count,
                duplicateCount,
                errors.Count);

            return new UploadCsvResult(
                TotalRows: readResult.Rows.Count,
                InsertedRows: recordsToInsert.Count,
                DuplicateRows: duplicateCount,
                Errors: errors);
        }

        private static HashSet<ImportedRecord> FindDuplicatesWithinFile(IReadOnlyCollection<ImportedRecord> records)
        {
            var seenKeys =
                new HashSet<Domain.ValueObjects.ImportedRecordKey>();

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
    }
}
