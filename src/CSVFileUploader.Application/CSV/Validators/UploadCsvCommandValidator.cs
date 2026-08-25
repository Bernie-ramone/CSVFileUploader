using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Application.CSV.UploadCsv;
using FluentValidation;

namespace CSVFileUploader.Application.CSV.Validators
{

    public sealed class UploadCsvCommandValidator
        : AbstractValidator<UploadCsvCommand>
    {
        private static readonly HashSet<string> AllowedContentTypes =
            new(
                StringComparer.OrdinalIgnoreCase)
            {
            "text/csv",
            "application/csv",
            "application/vnd.ms-excel"
            };

        private readonly CsvUploadOptions _options;

        public UploadCsvCommandValidator(
            CsvUploadOptions options)
        {
            _options = options;

            RuleFor(x => x.FileStream)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .Must(
                    stream =>
                        stream is not null &&
                        stream.CanRead)
                .WithMessage(
                    "The uploaded file cannot be read.");

            RuleFor(x => x.FileName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(
                    fileName =>
                        string.Equals(
                            Path.GetExtension(fileName),
                            ".csv",
                            StringComparison.OrdinalIgnoreCase))
                .WithMessage(
                    "The uploaded file must be a CSV file.")
                .Must(
                    IsSafeFileName)
                .WithMessage(
                    "The uploaded file name is invalid.");

            RuleFor(x => x.FileSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(
                    _options.MaximumFileSizeInBytes)
                .WithMessage(
                    $"The uploaded file cannot exceed " +
                    $"{_options.MaximumFileSizeInBytes / 1024 / 1024} MB.");

            RuleFor(x => x.ContentType)
                .Must(
                    contentType =>
                        string.IsNullOrWhiteSpace(contentType) ||
                        AllowedContentTypes.Contains(
                            contentType.Trim()))
                .WithMessage(
                    "The uploaded file content type is not supported.");
        }

        private static bool IsSafeFileName(
            string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            if (!string.Equals(
                    Path.GetFileName(fileName),
                    fileName,
                    StringComparison.Ordinal))
            {
                return false;
            }

            if (fileName.Contains(
                    Path.DirectorySeparatorChar) ||
                fileName.Contains(
                    Path.AltDirectorySeparatorChar))
            {
                return false;
            }

            return fileName.IndexOfAny(
                       Path.GetInvalidFileNameChars())
                   < 0;
        }
    }
}