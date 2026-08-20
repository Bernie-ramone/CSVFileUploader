using System;
using System.Collections.Generic;
using System.Text;

namespace CSVFileUploader.Application.Common.Models
{
    public static class CsvUploadOptionsValidator
    {
        public static void Validate(
            CsvUploadOptions options)
        {
            if (options.MaximumFileSizeInBytes <= 0)
            {
                throw new InvalidOperationException(
                    "CsvUpload:MaximumFileSizeInBytes " +
                    "must be greater than zero.");
            }
        }
    }
}
