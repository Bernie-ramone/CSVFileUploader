using System.Globalization;
using CSVFileUploader.Application.Common.Interfaces;
using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Application.DTOs;
using CsvHelper;
using CsvHelper.Configuration;

namespace CSVFileUploader.Infrastructure.CSV
{
    public sealed class CsvReader : ICsvReader
    {
        public async Task<CsvReadResult> ReadAsync(
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (!stream.CanRead)
            {
                throw new ArgumentException(
                    "The provided stream cannot be read.",
                    nameof(stream));
            }

            using var textReader = new StreamReader(
                stream,
                leaveOpen: true);

            var configuration = new CsvConfiguration(
                CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim,
                DetectColumnCountChanges = true
            };

            using var csv = new CsvHelper.CsvReader(
                textReader,
                configuration);

            await csv.ReadAsync();

            cancellationToken.ThrowIfCancellationRequested();

            csv.ReadHeader();

            var headers = csv.HeaderRecord
                ?? throw new InvalidOperationException(
                    "The CSV file does not contain a header row.");

            var rows = new List<CsvRowDto>();

            while (await csv.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var row = new CsvRowDto(
                    csv.GetField<string>(CsvFileDefinition.RecordId)
                        ?? string.Empty,

                    csv.GetField<string>(CsvFileDefinition.AssetId)
                        ?? string.Empty,

                    csv.GetField<string>(CsvFileDefinition.SourceSite)
                        ?? string.Empty,

                    csv.GetField<string>(CsvFileDefinition.DestinationSite)
                        ?? string.Empty,

                    DateOnly.ParseExact(
                        csv.GetField<string>(
                            CsvFileDefinition.EventDate)!,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture),

                    decimal.Parse(
                        csv.GetField<string>(
                            CsvFileDefinition.Volume)!,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture),

                    GetOptionalField(
                        csv,
                        CsvFileDefinition.Unit),

                    GetOptionalField(
                        csv,
                        CsvFileDefinition.Notes));

                rows.Add(row);
            }

            return new CsvReadResult(
                headers,
                rows);
        }

        private static string? GetOptionalField(
            CsvHelper.CsvReader csv,
            string fieldName)
        {
            var value = csv.GetField<string>(fieldName);

            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
