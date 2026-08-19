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
            await using var database = new TestDatabase();

            await database.InitializeAsync();

            await using var context =
                database.CreateContext();

            var repository =
                new ImportedRecordRepository(context);

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

            await repository.AddRangeAsync(
                [firstRecord]);

            await Assert.ThrowsAsync<DbUpdateException>(
                () => repository.AddRangeAsync(
                    [duplicateRecord]));
        }
    }
}