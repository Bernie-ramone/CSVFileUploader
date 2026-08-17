using System.Globalization;
using System.Text.RegularExpressions;
using CSVFileUploader.Application.DTOs;
using FluentValidation;

namespace CSVFileUploader.Application.CSV.Validators
{
    public sealed class CsvRowValidator
    : AbstractValidator<CsvRowDto>
    {
        private const string DateFormat = "yyyy-MM-dd";

        public CsvRowValidator()
        {
            RuleFor(x => x.RecordId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(50)
                .Matches("^REC-\\d{4}$")
                .WithMessage(
                    "RecordId must follow the format REC-####.");

            RuleFor(x => x.AssetId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(50)
                .Matches("^AST-\\d{4}$")
                .WithMessage(
                    "AssetId must follow the format AST-####.");

            RuleFor(x => x.SourceSite)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.DestinationSite)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.EventDate)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(BeValidDate)
                .WithMessage(
                    $"EventDate must use the {DateFormat} format.");

            RuleFor(x => x.Volume)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(BeValidDecimal)
                .WithMessage(
                    "Volume must be a valid decimal number.")
                .Must(HaveMaximumTwoDecimalPlaces)
                .WithMessage(
                    "Volume cannot have more than 2 decimal places.")
                .Must(BeNonNegative)
                .WithMessage(
                    "Volume must be greater than or equal to 0.");

            RuleFor(x => x.Unit)
                .MaximumLength(20)
                .Must(BeTonWhenProvided)
                .WithMessage("Unit must be TON when provided.");

            RuleFor(x => x.Notes)
                .MaximumLength(500);
        }

        private static bool BeValidDate(
            string value)
        {
            return DateOnly.TryParseExact(
                value,
                DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _);
        }

        private static bool BeValidDecimal(
            string value)
        {
            return decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out _);
        }

        private static bool HaveMaximumTwoDecimalPlaces(
            string value)
        {
            if (!decimal.TryParse(
                    value,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var number))
            {
                return false;
            }

            return decimal.Round(number, 2) == number;
        }

        private static bool BeNonNegative(
            string value)
        {
            return decimal.TryParse(
                       value,
                       NumberStyles.Number,
                       CultureInfo.InvariantCulture,
                       out var number)
                   && number >= 0;
        }

        private static bool BeTonWhenProvided(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                   || string.Equals(
                       value,
                       "TON",
                       StringComparison.OrdinalIgnoreCase);
        }

    }
}