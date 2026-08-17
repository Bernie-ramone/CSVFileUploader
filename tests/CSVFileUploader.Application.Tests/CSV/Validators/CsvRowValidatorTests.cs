using CSVFileUploader.Application.CSV.Validators;
using CSVFileUploader.Application.DTOs;


namespace CSVFileUploader.Application.Tests.CSV.Validators
{
    public class CsvRowValidatorTests
    {
        private readonly CsvRowValidator _validator = new();

        [Fact]
        public async Task ValidRow_ShouldPassValidation()
        {
            var row = CreateValidRow();

            var result = await _validator.ValidateAsync(row);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task MissingRecordId_ShouldFailValidation()
        {
            var row = CreateValidRow() with
            {
                RecordId = string.Empty
            };

            var result = await _validator.ValidateAsync(row);

            Assert.False(result.IsValid);

            Assert.Contains(
                result.Errors,
                error => error.PropertyName == "RecordId");
        }

        [Fact]
        public async Task InvalidRecordIdFormat_ShouldFailValidation()
        {
            var row = CreateValidRow() with
            {
                RecordId = "INVALID"
            };

            var result = await _validator.ValidateAsync(row);

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task InvalidAssetIdFormat_ShouldFailValidation()
        {
            var row = CreateValidRow() with
            {
                AssetId = "ASSET-001"
            };

            var result = await _validator.ValidateAsync(row);

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task InvalidDate_ShouldFailValidation()
        {
            var row = CreateValidRow() with
            {
                EventDate = "08/01/2026"
            };

            var result = await _validator.ValidateAsync(row);

            Assert.False(result.IsValid);

            Assert.Contains(
                result.Errors,
                error => error.PropertyName == "EventDate");
        }

        [Fact]
        public async Task InvalidDateValue_ShouldFailValidation()
        {
            var row = CreateValidRow() with
            {
                EventDate = "2026-99-99"
            };

            var result = await _validator.ValidateAsync(row);

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task InvalidVolume_ShouldFailValidation()
        {
            var row = CreateValidRow() with
            {
                Volume = "ABC"
            };

            var result = await _validator.ValidateAsync(row);

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task NegativeVolume_ShouldFailValidation()
        {
            var row = CreateValidRow() with
            {
                Volume = "-10.50"
            };

            var result = await _validator.ValidateAsync(row);

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task VolumeWithMoreThanTwoDecimals_ShouldFailValidation()
        {
            var row = CreateValidRow() with
            {
                Volume = "125.999"
            };

            var result = await _validator.ValidateAsync(row);

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task EmptyOptionalFields_ShouldPassValidation()
        {
            var row = CreateValidRow() with
            {
                Unit = null,
                Notes = null
            };

            var result = await _validator.ValidateAsync(row);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task InvalidUnit_ShouldFailValidation()
        {
            var row = CreateValidRow() with
            {
                Unit = "KG"
            };

            var result = await _validator.ValidateAsync(row);

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task NotesOverMaximumLength_ShouldFailValidation()
        {
            var row = CreateValidRow() with
            {
                Notes = new string('A', 501)
            };

            var result = await _validator.ValidateAsync(row);

            Assert.False(result.IsValid);
        }

        private static CsvRowDto CreateValidRow()
        {
            return new CsvRowDto(
                RowNumber: 2,
                RecordId: "REC-0001",
                AssetId: "AST-1001",
                SourceSite: "MINE-NORTH",
                DestinationSite: "PLANT-A",
                EventDate: "2026-08-01",
                Volume: "125.50",
                Unit: "TON",
                Notes: "Morning shift");
        }
    }
}
