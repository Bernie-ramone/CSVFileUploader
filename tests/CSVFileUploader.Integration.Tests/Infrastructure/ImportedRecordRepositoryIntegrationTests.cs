using CSVFileUploader.Domain.Entities;
using CSVFileUploader.Integration.Tests.Infrastructure;
using CSVFileUploader.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CSVFileUploader.Integration.Tests.Infrastructure
{
    public class ImportedRecordRepositoryIntegrationTests
    {
        [Fact]
        public async Task Database_ShouldRejectDuplicateBusinessKey()
        {
            await using var database =
                new TestDatabase();

            await database.InitializeAsync();

            await using var context =
                database.CreateContext();

            var repository =
                new ImportedRecordRepository(
                    context);

            var firstRecord =
                ImportedRecord.Create(
                    "REC-TEST-001",
                    "AST-9999",
                    "MINE-TEST",
                    "PLANT-TEST",
                    new DateOnly(2026, 8, 19),
                    100.00m,
                    "TON",
                    "First");

            var duplicateRecord =
                ImportedRecord.Create(
                    "REC-TEST-002",
                    "AST-9999",
                    "MINE-TEST",
                    "PLANT-TEST",
                    new DateOnly(2026, 8, 19),
                    100.00m,
                    "TON",
                    "Duplicate");

            // Stage and commit the first record.
            await repository.AddRangeAsync(
                [firstRecord]);

            await context.SaveChangesAsync();

            // Stage the duplicate record.
            await repository.AddRangeAsync(
                [duplicateRecord]);

            // The database unique constraint should reject
            // the duplicate when SaveChangesAsync is executed.
            await Assert.ThrowsAsync<DbUpdateException>(
                () => context.SaveChangesAsync());
        }
    }
}