using CSVFileUploader.Application.Common.Models;

namespace CSVFileUploader.Application.Common.Interfaces
{
    public interface ICsvStructureValidator
    {
        CsvStructureValidationResult Validate(IReadOnlyCollection<string> headers);
    }
}
