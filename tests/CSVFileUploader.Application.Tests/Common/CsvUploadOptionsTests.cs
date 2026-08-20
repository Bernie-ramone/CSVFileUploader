using CSVFileUploader.Application.Common.Models;

namespace CSVFileUploader.Application.Tests.Common
{
    public class CsvUploadOptionsTests
    {
        [Fact]
        public void DefaultMaximumFileSize_ShouldBe10Mb()
        {
            var options = new CsvUploadOptions();

            Assert.Equal(
                10 * 1024 * 1024,
                options.MaximumFileSizeInBytes);
        }

        [Fact]
        public void InvalidMaximumFileSize_ShouldThrow()
        {
            var options = new CsvUploadOptions
            {
                MaximumFileSizeInBytes = 0
            };

            Assert.Throws<InvalidOperationException>(
                () => CsvUploadOptionsValidator.Validate(options));
        }
    }
}
