using System;
using System.Collections.Generic;
using System.Text;

namespace CSVFileUploader.Domain.Enums
{
    public enum CsvUploadRowStatus
    {
        Imported = 1,
        Duplicate = 2,
        Invalid = 3
    }
}
