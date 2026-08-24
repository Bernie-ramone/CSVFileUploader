using CSVFileUploader.Application.Common.Interfaces;
using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Application.DTOs.UploadHistory;
using Microsoft.EntityFrameworkCore;

namespace CSVFileUploader.Infrastructure.Persistence.Repositories
{
    public sealed class UploadHistoryRepository
     : IUploadHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public UploadHistoryRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<UploadHistoryItemDto>> GetHistoryAsync(
                int pageNumber,
                int pageSize,
                CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber));
            }

            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            var query = _context.CsvUploads
                .AsNoTracking()
                .OrderByDescending(
                    x => x.UploadedAtUtc);

            var totalCount =
                await query.CountAsync(
                    cancellationToken);

            var items =
                await query
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(pageSize)
                    .Select(x =>
                        new UploadHistoryItemDto(
                            x.Id,
                            x.FileName,
                            x.UploadedAtUtc,
                            x.TotalRows,
                            x.InsertedRows,
                            x.DuplicateRows,
                            x.ErrorRows,
                            x.Status))
                    .ToListAsync(
                        cancellationToken);

            return new PagedResult<UploadHistoryItemDto>(
                items,
                pageNumber,
                pageSize,
                totalCount);
        }

        public async Task<UploadHistoryDetailDto?>
            GetDetailsAsync(
                Guid id,
                CancellationToken cancellationToken = default)
        {
            var upload =
                await _context.CsvUploads
                    .AsNoTracking()
                    .Include(x => x.Rows)
                    .SingleOrDefaultAsync(
                        x => x.Id == id,
                        cancellationToken);

            if (upload is null)
            {
                return null;
            }

            var rows = upload.Rows
                .OrderBy(x => x.RowNumber)
                .Select(x =>
                    new UploadHistoryRowDto(
                        x.RowNumber,
                        x.RecordId,
                        x.Status,
                        x.ErrorMessage))
                .ToArray();

            return new UploadHistoryDetailDto(
                upload.Id,
                upload.FileName,
                upload.UploadedAtUtc,
                upload.TotalRows,
                upload.InsertedRows,
                upload.DuplicateRows,
                upload.ErrorRows,
                upload.Status,
                rows);
        }
    }
}
