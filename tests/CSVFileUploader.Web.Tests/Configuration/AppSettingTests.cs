using System.Text.Json;

namespace CSVFileUploader.Web.Tests.Configuration
{

    public sealed class AppSettingsTests
    {
        [Fact]
        public void AppSettings_ShouldNotContainConnectionString()
        {
            var path =
                GetAppSettingsPath();

            using var document =
                JsonDocument.Parse(
                    File.ReadAllText(path));

            var root =
                document.RootElement;

            Assert.False(
                root.TryGetProperty(
                    "ConnectionStrings",
                    out _));
        }

        [Fact]
        public void AppSettings_ShouldDefineCsvUploadLimits()
        {
            var path =
                GetAppSettingsPath();

            using var document =
                JsonDocument.Parse(
                    File.ReadAllText(path));

            var csvUpload =
                document.RootElement
                    .GetProperty("CsvUpload");

            var maximumFileSize =
                csvUpload
                    .GetProperty(
                        "MaximumFileSizeInBytes")
                    .GetInt64();

            var maximumRowCount =
                csvUpload
                    .GetProperty(
                        "MaximumRowCount")
                    .GetInt32();

            Assert.True(
                maximumFileSize > 0);

            Assert.True(
                maximumRowCount > 0);
        }

        private static string GetAppSettingsPath()
        {
            var path =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "appsettings.json");

            if (File.Exists(path))
            {
                return path;
            }

            var projectDirectory =
                Directory.GetParent(
                    AppContext.BaseDirectory)?
                .Parent?
                .Parent?
                .Parent?
                .Parent?
                .FullName;

            return Path.Combine(
                projectDirectory
                    ?? throw new DirectoryNotFoundException(
                        "Could not locate the Web project directory."),
                "src",
                "CSVFileUploader.Web",
                "appsettings.json");
        }
    }
}