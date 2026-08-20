using CSVFileUploader.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CSVFileUploader.Integration.Tests.Infrastructure
{
    public sealed class TestDatabase : IAsyncDisposable
    {
        private const string Server =
            "(localdb)\\MSSQLLocalDB";

        private readonly string _databaseName;

        public TestDatabase()
        {
            _databaseName =
                $"CSVFileUploaderTest_{Guid.NewGuid():N}";
        }

        public string ConnectionString =>
            $"Server={Server};" +
            $"Database={_databaseName};" +
            "Trusted_Connection=True;" +
            "TrustServerCertificate=True;" +
            "MultipleActiveResultSets=True";

        public ApplicationDbContext CreateContext()
        {
            var options =
                new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(ConnectionString)
                    .Options;

            return new ApplicationDbContext(options);
        }

        public async Task InitializeAsync()
        {
            await using var context =
                CreateContext();

            await context.Database.MigrateAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await using var context =
                CreateContext();

            await context.Database.EnsureDeletedAsync();
        }
    }
}
