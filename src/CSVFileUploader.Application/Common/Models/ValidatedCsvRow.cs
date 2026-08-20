using CSVFileUploader.Application.DTOs;
using CSVFileUploader.Domain.Entities;

namespace CSVFileUploader.Application.Common.Models
{
    public sealed record ValidatedCsvRow(CsvRowDto Row, ImportedRecord Record);
}
