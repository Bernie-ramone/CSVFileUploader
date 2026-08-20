using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Application.CSV.UploadCsv;
using FluentValidation;

namespace CSVFileUploader.Application.CSV.Validators
{
    public sealed class UploadCsvCommandValidator : AbstractValidator<UploadCsvCommand>
    {
        private readonly CsvUploadOptions _options;

        public UploadCsvCommandValidator(CsvUploadOptions options)
        {
            _options = options;

            RuleFor(x => x.FileStream)
                .NotNull()
                .Must(stream => stream.CanRead)
                .WithMessage(
                    "The uploaded file cannot be read.");

            RuleFor(x => x.FileName)
                .NotEmpty()
                .Must(fileName =>
                    string.Equals(
                        Path.GetExtension(fileName),
                        ".csv",
                        StringComparison.OrdinalIgnoreCase))
                .WithMessage(
                    "The uploaded file must be a CSV file.");

            RuleFor(x => x.FileSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(
                    _options.MaximumFileSizeInBytes)
                .WithMessage(
                    $"The uploaded file cannot exceed " +
                    $"{_options.MaximumFileSizeInBytes / 1024 / 1024} MB.");
        }
    }
}
