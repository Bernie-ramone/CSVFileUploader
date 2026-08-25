using CSVFileUploader.Application.Common.Interfaces;
using CSVFileUploader.Domain.Entities;
using CSVFileUploader.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSVFileUploader.Integration.Tests.Infrastructure
{
    public sealed class FailingImportedRecordRepository
    : IImportedRecordRepository
    {
        private readonly IImportedRecordRepository _inner;

        public FailingImportedRecordRepository(
            IImportedRecordRepository inner)
        {
            _inner = inner;
        }

        public Task<IReadOnlyCollection<ImportedRecordKey>>
            GetExistingBusinessKeysAsync(
                IReadOnlyCollection<ImportedRecordKey> businessKeys,
                CancellationToken cancellationToken = default)
        {
            return _inner.GetExistingBusinessKeysAsync(
                businessKeys,
                cancellationToken);
        }

        public Task AddRangeAsync(
            IReadOnlyCollection<ImportedRecord> records,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "Simulated database persistence failure.");
        }
    }
}
