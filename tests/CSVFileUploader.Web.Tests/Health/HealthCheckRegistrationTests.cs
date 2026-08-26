using CSVFileUploader.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CSVFileUploader.Web.Tests.Health
{

    public sealed class HealthCheckRegistrationTests
    {
        [Fact]
        public void HealthChecks_ShouldContainDatabaseCheck()
        {
            var services =
                new ServiceCollection();

            services.AddLogging();

            services
                .AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>(
                    name: "database",
                    failureStatus: HealthStatus.Unhealthy,
                    tags:
                    [
                        "ready",
                    "database"
                    ]);

            using var provider =
                services.BuildServiceProvider();

            var options =
                provider
                    .GetRequiredService<
                        IOptions<HealthCheckServiceOptions>>()
                    .Value;

            var registration =
                Assert.Single(
                    options.Registrations,
                    x =>
                        x.Name == "database");

            Assert.Equal(
                HealthStatus.Unhealthy,
                registration.FailureStatus);

            Assert.Contains(
                "ready",
                registration.Tags);

            Assert.Contains(
                "database",
                registration.Tags);
        }
    }
}