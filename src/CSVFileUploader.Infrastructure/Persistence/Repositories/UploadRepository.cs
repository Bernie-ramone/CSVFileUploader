using CSVFileUploader.Application.Common.Interfaces;
using CSVFileUploader.Domain.Entities;
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
    }
}
