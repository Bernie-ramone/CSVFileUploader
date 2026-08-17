using CSVFileUploader.Application.CSV.UploadCsv;
using FluentValidation;

namespace CSVFileUploader.Application.CSV.Validators
{
    public sealed class UploadCsvCommandValidator
    : AbstractValidator<UploadCsvCommand>
    {
        private const long MaximumFileSize = 10 * 1024 * 1024;

        public UploadCsvCommandValidator()
        {
            RuleFor(x => x.FileStream)
                .NotNull()
                .Must(stream => stream.CanRead)
                .WithMessage("The uploaded file cannot be read.");

            RuleFor(x => x.FileName)
                .NotEmpty()
                .Must(fileName =>
                    string.Equals(
                        Path.GetExtension(fileName),
                        ".csv",
                        StringComparison.OrdinalIgnoreCase))
                .WithMessage("The uploaded file must be a CSV file.");

            RuleFor(x => x.FileSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(MaximumFileSize)
                .WithMessage(
                    $"The uploaded file cannot exceed " +
                    $"{MaximumFileSize / 1024 / 1024} MB.");
        }
    }
}
