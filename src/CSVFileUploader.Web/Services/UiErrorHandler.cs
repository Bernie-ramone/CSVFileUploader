using Microsoft.Extensions.Logging;

namespace CSVFileUploader.Web.Services
{

    public sealed class UiErrorHandler
    {
        private readonly ILogger<UiErrorHandler> _logger;

        public UiErrorHandler(
            ILogger<UiErrorHandler> logger)
        {
            _logger = logger;
        }

        public string Handle(
            Exception exception,
            string operation)
        {
            ArgumentNullException.ThrowIfNull(exception);

            if (exception is OperationCanceledException)
            {
                throw exception;
            }

            var userMessage =
                exception switch
                {
                    IOException =>
                        "The file could not be read.",

                    ArgumentException =>
                        "The supplied information is invalid.",

                    InvalidOperationException =>
                        "The operation could not be completed.",

                    _ =>
                        "An unexpected error occurred. " +
                        "Please try again."
                };

            _logger.LogError(
                exception,
                "UI operation {Operation} failed.",
                operation);

            return userMessage;
        }
    }
}