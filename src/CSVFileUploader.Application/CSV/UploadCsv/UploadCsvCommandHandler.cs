using CSVFileUploader.Application.Common.Interfaces;
using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Domain.Entities;
using CSVFileUploader.Domain.Enums;

namespace CSVFileUploader.Application.CSV.UploadCsv
{
    public sealed class UploadCsvCommandHandler
    {
        private readonly ICsvReader _csvReader;
        private readonly ICsvStructureValidator _structureValidator;
        private readonly IImportedRecordRepository _repository;

        public UploadCsvCommandHandler(
            ICsvReader csvReader,
            ICsvStructureValidator structureValidator,
            IImportedRecordRepository repository)
        {
            _csvReader = csvReader;
            _structureValidator = structureValidator;
            _repository = repository;
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
            var duplicates = 0;

            foreach (var row in readResult.Rows)
            {
                var record = ImportedRecord.Create(
                    row.RecordId,
                    row.AssetId,
                    row.SourceSite,
                    row.DestinationSite,
                    row.EventDate,
                    row.Volume,
                    row.Unit,
                    row.Notes);

                var exists = await _repository.ExistsByBusinessKeyAsync(
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
                .Where(record => record.Status != ImportRecordStatus.Duplicate)
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
                Errors: []);
        }
    }
}
