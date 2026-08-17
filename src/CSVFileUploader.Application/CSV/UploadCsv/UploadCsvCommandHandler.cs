using System.Globalization;
using CSVFileUploader.Application.Common.Interfaces;
using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Application.CSV.Validators;
using CSVFileUploader.Domain.Entities;
using CSVFileUploader.Domain.Enums;
using FluentValidation;

namespace CSVFileUploader.Application.CSV.UploadCsv
{
    public sealed class UploadCsvCommandHandler
    {
        private readonly ICsvReader _csvReader;
        private readonly ICsvStructureValidator _structureValidator;
        private readonly IImportedRecordRepository _repository;
        private readonly IValidator<DTOs.CsvRowDto> _rowValidator;
        private readonly IValidator<UploadCsvCommand> _commandValidator;

        public UploadCsvCommandHandler(
            ICsvReader csvReader,
            ICsvStructureValidator structureValidator,
            IImportedRecordRepository repository,
            IValidator<DTOs.CsvRowDto> rowValidator,
            IValidator<UploadCsvCommand> commandValidator)
        {
            _csvReader = csvReader;
            _structureValidator = structureValidator;
            _repository = repository;
            _rowValidator = rowValidator;
            _commandValidator = commandValidator;
        }

        public async Task<UploadCsvResult> HandleAsync(
            UploadCsvCommand command,
            CancellationToken cancellationToken = default)
        {
            var readResult = await _csvReader.ReadAsync(
                command.FileStream,
                cancellationToken);

            var structureValidation = _structureValidator.Validate(
                readResult.Headers);

            if (!structureValidation.IsValid)
            {
                var errors = structureValidation.Errors
                    .Select(error => new CsvUploadError(0, error))
                    .ToArray();

                return new UploadCsvResult(
                    TotalRows: 0,
                    InsertedRows: 0,
                    DuplicateRows: 0,
                    Errors: errors);
            }

            var records = new List<ImportedRecord>();
            var errorsList = new List<CsvUploadError>();
            var duplicates = 0;

            foreach (var row in readResult.Rows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var validationResult =
                    await _rowValidator.ValidateAsync(
                        row,
                        cancellationToken);

                if (!validationResult.IsValid)
                {
                    errorsList.AddRange(
                        validationResult.Errors.Select(
                            error => new CsvUploadError(
                                row.RowNumber,
                                error.ErrorMessage)));

                    continue;
                }

                var record = CreateDomainRecord(row);

                var exists =
                    await _repository.ExistsByBusinessKeyAsync(
                        record.BusinessKey,
                        cancellationToken);

                if (exists)
                {
                    record.MarkAsDuplicate();
                    duplicates++;
                }

                records.Add(record);
            }

            var recordsToInsert = records
                .Where(record =>
                    record.Status != ImportRecordStatus.Duplicate)
                .ToArray();

            if (recordsToInsert.Length > 0)
            {
                await _repository.AddRangeAsync(
                    recordsToInsert,
                    cancellationToken);
            }

            return new UploadCsvResult(
                TotalRows: readResult.Rows.Count,
                InsertedRows: recordsToInsert.Length,
                DuplicateRows: duplicates,
                Errors: errorsList);
        }

        private static ImportedRecord CreateDomainRecord(
            DTOs.CsvRowDto row)
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
