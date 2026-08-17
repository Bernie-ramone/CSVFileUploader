using CSVFileUploader.Application.CSV.UploadCsv;
using CSVFileUploader.Application.CSV.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSVFileUploader.Application.Tests.CSV.Validators
{
    public class UploadCsvCommandValidatorTests
    {
        private readonly UploadCsvCommandValidator _validator = new();

        [Fact]
        public async Task ValidCsvCommand_ShouldPass()
        {
            await using var stream =
                new MemoryStream(
                    "test csv"u8.ToArray());

            var command = new UploadCsvCommand(
                stream,
                "test.csv",
                "text/csv",
                stream.Length);

            var result = await _validator.ValidateAsync(command);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task NonCsvFile_ShouldFail()
        {
            await using var stream =
                new MemoryStream(
                    "test"u8.ToArray());

            var command = new UploadCsvCommand(
                stream,
                "test.txt",
                "text/plain",
                stream.Length);

            var result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task EmptyFile_ShouldFail()
        {
            await using var stream =
                new MemoryStream();

            var command = new UploadCsvCommand(
                stream,
                "test.csv",
                "text/csv",
                0);

            var result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
        }
    }
}
