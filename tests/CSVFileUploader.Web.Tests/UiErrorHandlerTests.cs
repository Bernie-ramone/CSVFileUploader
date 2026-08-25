using CSVFileUploader.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace CSVFileUploader.Web.Tests.Services
{

    public sealed class UiErrorHandlerTests
    {
        private readonly UiErrorHandler _handler =
            new(
                NullLogger<UiErrorHandler>.Instance);

        [Fact]
        public void Handle_WithIOException_ShouldReturnFileMessage()
        {
            var exception =
                new IOException(
                    "Internal file-system failure.");

            var result =
                _handler.Handle(
                    exception,
                    "CSV file upload");

            Assert.Equal(
                "The file could not be read.",
                result);
        }

        [Fact]
        public void Handle_WithArgumentException_ShouldReturnValidationMessage()
        {
            var exception =
                new ArgumentException(
                    "Internal argument details.");

            var result =
                _handler.Handle(
                    exception,
                    "CSV file upload");

            Assert.Equal(
                "The supplied information is invalid.",
                result);
        }

        [Fact]
        public void Handle_WithInvalidOperationException_ShouldReturnSafeMessage()
        {
            var exception =
                new InvalidOperationException(
                    "Sensitive internal information.");

            var result =
                _handler.Handle(
                    exception,
                    "loading upload history");

            Assert.Equal(
                "The operation could not be completed.",
                result);
        }

        [Fact]
        public void Handle_WithUnexpectedException_ShouldReturnGenericMessage()
        {
            var exception =
                new Exception(
                    "Database password and internal details.");

            var result =
                _handler.Handle(
                    exception,
                    "loading upload details");

            Assert.Equal(
                "An unexpected error occurred. Please try again.",
                result);

            Assert.DoesNotContain(
                "Database password",
                result,
                StringComparison.Ordinal);

            Assert.DoesNotContain(
                "internal details",
                result,
                StringComparison.Ordinal);
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

        [Fact]
        public void Handle_WithNullException_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                    _handler.Handle(
                        null!,
                        "CSV file upload"));
        }
    }
}