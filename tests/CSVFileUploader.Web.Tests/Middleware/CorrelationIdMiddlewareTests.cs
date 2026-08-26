using CSVFileUploader.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace CSVFileUploader.Web.Tests.Middleware
{

    public sealed class CorrelationIdMiddlewareContextTests
    {
        [Fact]
        public async Task InvokeAsync_ShouldStoreCorrelationIdInHttpContextItems()
        {
            var context =
                new DefaultHttpContext();

            var middleware =
                new CorrelationIdMiddleware(
                    _ => Task.CompletedTask,
                    NullLogger<CorrelationIdMiddleware>.Instance);

            await middleware.InvokeAsync(
                context);

            var responseCorrelationId =
                context.Response.Headers[
                    CorrelationIdMiddleware.HeaderName]
                    .ToString();

            var storedCorrelationId =
                context.Items[
                    CorrelationIdMiddleware.ContextItemKey]
                    ?.ToString();

            Assert.False(
                string.IsNullOrWhiteSpace(
                    responseCorrelationId));

            Assert.Equal(
                responseCorrelationId,
                storedCorrelationId);
        }

        [Fact]
        public async Task InvokeAsync_ShouldStoreIncomingCorrelationId()
        {
            var correlationId =
                Guid.NewGuid().ToString("D");

            var context =
                new DefaultHttpContext();

            context.Request.Headers[
                CorrelationIdMiddleware.HeaderName] =
                correlationId;

            var middleware =
                new CorrelationIdMiddleware(
                    _ => Task.CompletedTask,
                    NullLogger<CorrelationIdMiddleware>.Instance);

            await middleware.InvokeAsync(
                context);

            Assert.Equal(
                correlationId,
                context.Items[
                    CorrelationIdMiddleware.ContextItemKey]
                    ?.ToString());
        }
    }
}