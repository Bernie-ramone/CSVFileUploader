using CSVFileUploader.Application.Common.Interfaces;
using CSVFileUploader.Application.Common.Models;

namespace CSVFileUploader.Infrastructure.CSV
{
    public sealed class CsvStructureValidator
    : ICsvStructureValidator
    {
        public CsvStructureValidationResult Validate(
            IReadOnlyCollection<string> headers)
        {
            if (headers.Count == 0)
            {
                return CsvStructureValidationResult.Failure(
                    "The CSV file does not contain any columns.");
            }

            var normalizedHeaders = headers
                .Select(header => header.Trim())
                .ToArray();

            var errors = new List<string>();

            ValidateDuplicates(normalizedHeaders, errors);
            ValidateRequiredHeaders(normalizedHeaders, errors);
            ValidateUnexpectedHeaders(normalizedHeaders, errors);
            ValidateColumnOrder(normalizedHeaders, errors);

            return errors.Count == 0
                ? CsvStructureValidationResult.Success()
                : CsvStructureValidationResult.Failure(
                    errors.ToArray());
        }

        private static void ValidateDuplicates(
            IReadOnlyCollection<string> headers,
            ICollection<string> errors)
        {
            var duplicates = headers
                .GroupBy(
                    header => header,
                    StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();

            foreach (var duplicate in duplicates)
            {
                errors.Add(
                    $"Duplicate column '{duplicate}' was found.");
            }
        }

        private static void ValidateRequiredHeaders(
            IReadOnlyCollection<string> headers,
            ICollection<string> errors)
        {
            foreach (var requiredHeader in CsvFileDefinition.RequiredHeaders)
            {
                var exists = headers.Any(
                    header => string.Equals(
                        header,
                        requiredHeader,
                        StringComparison.OrdinalIgnoreCase));

                if (!exists)
                {
                    errors.Add(
                        $"Required column '{requiredHeader}' is missing.");
                }
            }
        }

        private static void ValidateUnexpectedHeaders(
            IReadOnlyCollection<string> headers,
            ICollection<string> errors)
        {
            foreach (var header in headers)
            {
                var isExpected = CsvFileDefinition.OrderedHeaders.Any(
                    expected => string.Equals(
                        expected,
                        header,
                        StringComparison.OrdinalIgnoreCase));

                if (!isExpected)
                {
                    errors.Add(
                        $"Unexpected column '{header}' was found.");
                }
            }
        }

        private static void ValidateColumnOrder(
            IReadOnlyCollection<string> headers,
            ICollection<string> errors)
        {
            var actualHeaders = headers.ToArray();

            if (actualHeaders.Count() != CsvFileDefinition.OrderedHeaders.Count)
            {
                errors.Add(
                    $"Expected exactly " +
                    $"{CsvFileDefinition.OrderedHeaders.Count} columns, " +
                    $"but found {actualHeaders.Count()}.");

                return;
            }

            for (var index = 0;
                 index < CsvFileDefinition.OrderedHeaders.Count;
                 index++)
            {
                if (!string.Equals(
                        actualHeaders[index],
                        CsvFileDefinition.OrderedHeaders[index],
                        StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(
                        $"Column {index + 1} should be " +
                        $"'{CsvFileDefinition.OrderedHeaders[index]}', " +
                        $"but found '{actualHeaders[index]}'.");
                }
            }
        }
    }
}
