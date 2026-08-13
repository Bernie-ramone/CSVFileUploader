namespace CSVFileUploader.Application.Common.Models
{
    public sealed record CsvStructureValidationResult(
    bool IsValid,
    IReadOnlyCollection<string> Errors)
    {
        public static CsvStructureValidationResult Success() =>
            new(true, []);

        public static CsvStructureValidationResult Failure(
            params string[] errors) =>
            new(false, errors);
    }
}
