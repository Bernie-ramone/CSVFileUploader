using Microsoft.AspNetCore.Authorization;
using CSVFileUploader.Web.Components.Pages;

namespace CSVFileUploader.Web.Tests.Authorization
{

    public sealed class RouteAuthorizationTests
    {
        [Fact]
        public void UploadPage_ShouldRequireAuthorization()
        {
            var authorizeAttribute =
                typeof(Upload)
                    .GetCustomAttributes(
                        typeof(AuthorizeAttribute),
                        inherit: true)
                    .SingleOrDefault();

            Assert.NotNull(
                authorizeAttribute);
        }

        [Fact]
        public void UploadsPage_ShouldRequireAuthorization()
        {
            var authorizeAttribute =
                typeof(Uploads)
                    .GetCustomAttributes(
                        typeof(AuthorizeAttribute),
                        inherit: true)
                    .SingleOrDefault();

            Assert.NotNull(
                authorizeAttribute);
        }

        [Fact]
        public void UploadDetailsPage_ShouldRequireAuthorization()
        {
            var authorizeAttribute =
                typeof(UploadDetails)
                    .GetCustomAttributes(
                        typeof(AuthorizeAttribute),
                        inherit: true)
                    .SingleOrDefault();

            Assert.NotNull(
                authorizeAttribute);
        }

        [Fact]
        public void UploadPage_ShouldNotDefineAnonymousAccess()
        {
            var allowAnonymousAttribute =
                typeof(Upload)
                    .GetCustomAttributes(
                        typeof(AllowAnonymousAttribute),
                        inherit: true)
                    .SingleOrDefault();

            Assert.Null(
                allowAnonymousAttribute);
        }

        [Fact]
        public void UploadsPage_ShouldNotDefineAnonymousAccess()
        {
            var allowAnonymousAttribute =
                typeof(Uploads)
                    .GetCustomAttributes(
                        typeof(AllowAnonymousAttribute),
                        inherit: true)
                    .SingleOrDefault();

            Assert.Null(
                allowAnonymousAttribute);
        }

        [Fact]
        public void UploadDetailsPage_ShouldNotDefineAnonymousAccess()
        {
            var allowAnonymousAttribute =
                typeof(UploadDetails)
                    .GetCustomAttributes(
                        typeof(AllowAnonymousAttribute),
                        inherit: true)
                    .SingleOrDefault();

            Assert.Null(
                allowAnonymousAttribute);
        }
    }
}