using Microsoft.Extensions.Primitives;

namespace CSVFileUploader.Web.Middleware
{

    public sealed class CorrelationIdMiddleware
    {
        public const string HeaderName =
            "X-Correlation-ID";

        public const string ContextItemKey =
            "CorrelationId";

        private readonly RequestDelegate _next;

        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(
            RequestDelegate next,
            ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context)
        {
            var correlationId =
                GetOrCreateCorrelationId(
                    context);

            context.Items[ContextItemKey] =
                correlationId;

            context.Response.Headers[HeaderName] =
                correlationId;

            using var loggingScope =
                _logger.BeginScope(
                    new Dictionary<string, object>
                    {
                        ["CorrelationId"] =
                            correlationId,

                        ["TraceIdentifier"] =
                            context.TraceIdentifier
                    });

            _logger.LogDebug(
                "Request started. Method={Method}, Path={Path}.",
                context.Request.Method,
                context.Request.Path);

            try
            {
                await _next(context);

                _logger.LogDebug(
                    "Request completed. StatusCode={StatusCode}.",
                    context.Response.StatusCode);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Request failed. StatusCode={StatusCode}.",
                    context.Response.StatusCode);

                throw;
            }
        }

        private static string GetOrCreateCorrelationId(
            HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(
                    HeaderName,
                    out StringValues value) &&
                !StringValues.IsNullOrEmpty(value))
            {
                var incoming =
                    value.ToString().Trim();

                if (Guid.TryParse(
                        incoming,
                        out var parsed))
                {
                    return parsed.ToString("D");
                }
            }

            return Guid.NewGuid().ToString("D");
        }
    }
}