namespace CSVFileUploader.Application.CSV.UploadHistory
{
    public sealed record GetUploadHistoryQuery(
     int PageNumber = 1,
     int PageSize = 20);
}
