using CSVFileUploader.Application.DTOs.UploadHistory;
using System.Text;

namespace CSVFileUploader.Application.CSV.UploadHistory
{
    public sealed class UploadIssuesCsvExporter
    {
        public byte[] Export(
            UploadHistoryDetailDto upload)
        {
            ArgumentNullException.ThrowIfNull(upload);

            var rows = upload.Rows
                .Where(row =>
                    row.Status !=
                    Domain.Enums.CsvUploadRowStatus.Imported)
                .OrderBy(row => row.RowNumber)
                .ToArray();

            var builder = new StringBuilder();

            builder.AppendLine(
                "RowNumber,RecordId,Status,Message");

            foreach (var row in rows)
            {
                builder
                    .Append(row.RowNumber)
                    .Append(',')
                    .Append(Escape(row.RecordId))
                    .Append(',')
                    .Append(row.Status)
                    .Append(',')
                    .Append(Escape(row.ErrorMessage))
                    .AppendLine();
            }

            return new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: true)
                .GetBytes(builder.ToString());
        }

        private static string Escape(
            string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var escaped = value.Replace(
                "\"",
                "\"\"");

            if (escaped.Contains(',') ||
                escaped.Contains('"') ||
                escaped.Contains('\r') ||
                escaped.Contains('\n'))
            {
                return $"\"{escaped}\"";
            }

            return escaped;
        }
    }
}
