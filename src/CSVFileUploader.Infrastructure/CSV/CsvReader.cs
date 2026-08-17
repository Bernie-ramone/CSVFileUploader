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

            var rowNumber = 1;

            while (await csv.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();

                rowNumber++;

                var row = new CsvRowDto(
                    RowNumber: rowNumber,

                    RecordId: GetRequiredField(
                        csv,
                        CsvFileDefinition.RecordId),

                    AssetId: GetRequiredField(
                        csv,
                        CsvFileDefinition.AssetId),

                    SourceSite: GetRequiredField(
                        csv,
                        CsvFileDefinition.SourceSite),

                    DestinationSite: GetRequiredField(
                        csv,
                        CsvFileDefinition.DestinationSite),

                    EventDate: GetRequiredField(
                        csv,
                        CsvFileDefinition.EventDate),

                    Volume: GetRequiredField(
                        csv,
                        CsvFileDefinition.Volume),

                    Unit: GetOptionalField(
                        csv,
                        CsvFileDefinition.Unit),

                    Notes: GetOptionalField(
                        csv,
                        CsvFileDefinition.Notes));

                rows.Add(row);
            }

            return new CsvReadResult(
                headers,
                rows);
        }

        private static string GetRequiredField(
            CsvHelper.CsvReader csv,
            string fieldName)
        {
            return csv.GetField<string>(fieldName)?.Trim()
                ?? string.Empty;
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