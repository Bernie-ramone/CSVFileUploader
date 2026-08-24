using CSVFileUploader.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace CSVFileUploader.Web.Tests
{
    public class UiErrorHandlerTests
    {
        private readonly UiErrorHandler _handler =
            new(
                NullLogger<UiErrorHandler>.Instance);

        [Fact]
        public void Handle_WithIOException_ShouldReturnFileMessage()
        {
            var exception =
                new IOException("File read failed.");

            var result =
                _handler.Handle(
                    exception,
                    "CSV file upload");

            Assert.Equal(
                "The file could not be read.",
                result);
        }

        [Fact]
        public void Handle_WithUnexpectedException_ShouldReturnGenericMessage()
        {
            var exception =
                new InvalidOperationException(
                    "Sensitive internal information.");

            var result =
                _handler.Handle(
                    exception,
                    "loading upload history");

            Assert.Equal(
                "An unexpected error occurred. Please try again.",
                result);
        }

        [Fact]
        public void Handle_WithCancellation_ShouldRethrow()
        {
            var exception =
                new OperationCanceledException();

            Assert.Throws<OperationCanceledException>(
                () =>
                    _handler.Handle(
                        exception,
                        "CSV file upload"));
        }
    }
}
