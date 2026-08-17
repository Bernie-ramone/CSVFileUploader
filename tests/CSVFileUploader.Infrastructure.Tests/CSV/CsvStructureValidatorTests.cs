using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Infrastructure.CSV;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSVFileUploader.Infrastructure.Tests.CSV
{
    public class CsvStructureValidatorTests
    {
        private readonly CsvStructureValidator _validator = new();

        [Fact]
        public void Validate_WithExpectedHeaders_ShouldBeValid()
        {
            var result = _validator.Validate(
                CsvFileDefinition.OrderedHeaders);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_WithMissingRequiredHeader_ShouldBeInvalid()
        {
            var headers =
                CsvFileDefinition.OrderedHeaders
                    .Where(x => x != CsvFileDefinition.AssetId)
                    .ToArray();

            var result = _validator.Validate(headers);

            Assert.False(result.IsValid);

            Assert.Contains(
                result.Errors,
                error => error.Contains("AssetId"));
        }

        [Fact]
        public void Validate_WithUnexpectedHeader_ShouldBeInvalid()
        {
            var headers =
                CsvFileDefinition.OrderedHeaders
                    .Select(x => x)
                    .Append("UnexpectedColumn")
                    .ToArray();

            var result = _validator.Validate(headers);

            Assert.False(result.IsValid);

            Assert.Contains(
                result.Errors,
                error => error.Contains("UnexpectedColumn"));
        }

        [Fact]
        public void Validate_WithDuplicateHeader_ShouldBeInvalid()
        {
            var headers =
                CsvFileDefinition.OrderedHeaders
                    .Append(CsvFileDefinition.AssetId)
                    .ToArray();

            var result = _validator.Validate(headers);

            Assert.False(result.IsValid);

            Assert.Contains(
                result.Errors,
                error => error.Contains("Duplicate column"));
        }

        [Fact]
        public void Validate_WithIncorrectOrder_ShouldBeInvalid()
        {
            var headers = CsvFileDefinition.OrderedHeaders
                .Reverse()
                .ToArray();

            var result = _validator.Validate(headers);

            Assert.False(result.IsValid);
        }
    }
}
