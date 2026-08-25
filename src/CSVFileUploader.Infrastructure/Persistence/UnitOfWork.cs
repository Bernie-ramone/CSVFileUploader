using CSVFileUploader.Application.Common.Interfaces;

namespace CSVFileUploader.Infrastructure.Persistence
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    cancellationToken);

            try
            {
                await operation(cancellationToken);

                await _context.SaveChangesAsync(
                    cancellationToken);

                await transaction.CommitAsync(
                    cancellationToken);
            }
            catch
            {
                try
                {
                    await transaction.RollbackAsync(
                        cancellationToken);
                }
                finally
                {
                    // Remove every entity that was tracked by the
                    // failed transaction so a subsequent SaveChangesAsync()
                    // cannot accidentally retry the failed business data.
                    _context.ChangeTracker.Clear();
                }

                throw;
            }
        }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(
                cancellationToken);
        }
    }
}
