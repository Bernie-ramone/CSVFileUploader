using CSVFileUploader.Application.Common.Interfaces;
using CSVFileUploader.Domain.Entities;
using CSVFileUploader.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CSVFileUploader.Infrastructure.Persistence.Repositories
{
    public sealed class UploadRepository
    : IUploadRepository
    {
        private readonly ApplicationDbContext _context;

        public UploadRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            CsvUpload upload,
            CancellationToken cancellationToken = default)
        {
            await _context.CsvUploads.AddAsync(
                upload,
                cancellationToken);
        }

        public Task<CsvUpload?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return _context.CsvUploads
                .Include(upload => upload.Rows)
                .FirstOrDefaultAsync(
                    upload => upload.Id == id,
                    cancellationToken);
        }

        public Task<CsvUpload?> GetSuccessfulUploadByFileHashAsync(
            string fileHash,
            CancellationToken cancellationToken = default)
        {
            return _context.CsvUploads
                .AsNoTracking()
                .Where(upload =>
                    upload.FileHash == fileHash &&
                    (upload.Status ==
                        CsvUploadStatus.Completed ||
                     upload.Status ==
                        CsvUploadStatus.CompletedWithErrors))
                .OrderByDescending(
                    upload => upload.UploadedAtUtc)
                .FirstOrDefaultAsync(
                    cancellationToken);
        }
    }
}
