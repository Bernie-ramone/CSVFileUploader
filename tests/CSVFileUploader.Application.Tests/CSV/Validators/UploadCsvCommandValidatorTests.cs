using System.Text;
using CSVFileUploader.Application.Common.Models;
using CSVFileUploader.Application.CSV.UploadCsv;
using CSVFileUploader.Application.CSV.Validators;
using FluentValidation.TestHelper;

namespace CSVFileUploader.Application.Tests.CSV.Validators
{

    public sealed class UploadCsvCommandValidatorTests
    {
        [Fact]
        public async Task Validate_WithValidCsv_ShouldPass()
        {
            var validator =
                CreateValidator();

            await using var stream =
                CreateReadableStream();

            var command =
                CreateCommand(
                    stream,
                    fileName: "data.csv",
                    contentType: "text/csv",
                    fileSize: 100);

            var result =
                await validator.TestValidateAsync(
                    command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithUpperCaseCsvExtension_ShouldPass()
        {
            var validator =
                CreateValidator();

            await using var stream =
                CreateReadableStream();

            var command =
                CreateCommand(
                    stream,
                    fileName: "DATA.CSV",
                    contentType: "text/csv",
                    fileSize: 100);

            var result =
                await validator.TestValidateAsync(
                    command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithNonCsvExtension_ShouldFail()
        {
            var validator =
                CreateValidator();

            await using var stream =
                CreateReadableStream();

            var command =
                CreateCommand(
                    stream,
                    fileName: "data.txt",
                    contentType: "text/csv",
                    fileSize: 100);

            var result =
                await validator.TestValidateAsync(
                    command);

            result.ShouldHaveValidationErrorFor(
                    x => x.FileName);
        }

        [Fact]
        public async Task Validate_WithPathInFileName_ShouldFail()
        {
            var validator =
                CreateValidator();

            await using var stream =
                CreateReadableStream();

            var command =
                CreateCommand(
                    stream,
                    fileName: @"..\data.csv",
                    contentType: "text/csv",
                    fileSize: 100);

            var result =
                await validator.TestValidateAsync(
                    command);

            result.ShouldHaveValidationErrorFor(
                x => x.FileName);
        }

        [Fact]
        public async Task Validate_WithAbsolutePathInFileName_ShouldFail()
        {
            var validator =
                CreateValidator();

            await using var stream =
                CreateReadableStream();

            var command =
                CreateCommand(
                    stream,
                    fileName: @"C:\Temp\data.csv",
                    contentType: "text/csv",
                    fileSize: 100);

            var result =
                await validator.TestValidateAsync(
                    command);

            result.ShouldHaveValidationErrorFor(
                x => x.FileName);
        }

        [Fact]
        public async Task Validate_WithEmptyFileName_ShouldFail()
        {
            var validator =
                CreateValidator();

            await using var stream =
                CreateReadableStream();

            var command =
                CreateCommand(
                    stream,
                    fileName: string.Empty,
                    contentType: "text/csv",
                    fileSize: 100);

            var result =
                await validator.TestValidateAsync(
                    command);

            result.ShouldHaveValidationErrorFor(
                x => x.FileName);
        }

        [Fact]
        public async Task Validate_WithZeroFileSize_ShouldFail()
        {
            var validator =
                CreateValidator();

            await using var stream =
                CreateReadableStream();

            var command =
                CreateCommand(
                    stream,
                    fileName: "data.csv",
                    contentType: "text/csv",
                    fileSize: 0);

            var result =
                await validator.TestValidateAsync(
                    command);

            result.ShouldHaveValidationErrorFor(
                x => x.FileSize);
        }

        [Fact]
        public async Task Validate_WhenFileExceedsMaximumSize_ShouldFail()
        {
            var validator =
                CreateValidator(
                    maximumFileSizeInBytes: 1024);

            await using var stream =
                CreateReadableStream();

            var command =
                CreateCommand(
                    stream,
                    fileName: "data.csv",
                    contentType: "text/csv",
                    fileSize: 1025);

            var result =
                await validator.TestValidateAsync(
                    command);

            result.ShouldHaveValidationErrorFor(
                x => x.FileSize);
        }

        [Fact]
        public async Task Validate_WithUnsupportedContentType_ShouldFail()
        {
            var validator =
                CreateValidator();

            await using var stream =
                CreateReadableStream();

            var command =
                CreateCommand(
                    stream,
                    fileName: "data.csv",
                    contentType: "application/json",
                    fileSize: 100);

            var result =
                await validator.TestValidateAsync(
                    command);

            result.ShouldHaveValidationErrorFor(
                x => x.ContentType);
        }

        [Fact]
        public async Task Validate_WithEmptyContentType_ShouldPass()
        {
            var validator =
                CreateValidator();

            await using var stream =
                CreateReadableStream();

            var command =
                CreateCommand(
                    stream,
                    fileName: "data.csv",
                    contentType: null,
                    fileSize: 100);

            var result =
                await validator.TestValidateAsync(
                    command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithApplicationVndMsExcelContentType_ShouldPass()
        {
            var validator =
                CreateValidator();

            await using var stream =
                CreateReadableStream();

            var command =
                CreateCommand(
                    stream,
                    fileName: "data.csv",
                    contentType: "application/vnd.ms-excel",
                    fileSize: 100);

            var result =
                await validator.TestValidateAsync(
                    command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithUnreadableStream_ShouldFail()
        {
            var validator =
                CreateValidator();

            await using var stream =
                new NonReadableStream();

            var command =
                CreateCommand(
                    stream,
                    fileName: "data.csv",
                    contentType: "text/csv",
                    fileSize: 100);

            var result =
                await validator.TestValidateAsync(
                    command);

            result.ShouldHaveValidationErrorFor(
                x => x.FileStream);
        }

        private static UploadCsvCommandValidator CreateValidator(
            long maximumFileSizeInBytes =
                10 * 1024 * 1024)
        {
            return new UploadCsvCommandValidator(
                new CsvUploadOptions
                {
                    MaximumFileSizeInBytes =
                        maximumFileSizeInBytes
                });
        }

        private static UploadCsvCommand CreateCommand(
            Stream stream,
            string fileName,
            string? contentType,
            long fileSize)
        {
            return new UploadCsvCommand(
                stream,
                fileName,
                contentType,
                fileSize);
        }

        private static MemoryStream CreateReadableStream()
        {
            return new MemoryStream(
                Encoding.UTF8.GetBytes(
                    "test"));
        }

        private sealed class NonReadableStream
            : Stream
        {
            public override bool CanRead => false;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => 0;

            public override long Position
            {
                get => 0;
                set => throw new NotSupportedException();
            }

            public override void Flush()
            {
            }

            public override int Read(
                byte[] buffer,
                int offset,
                int count)
            {
                throw new NotSupportedException();
            }

            public override long Seek(
                long offset,
                SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(
                long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(
                byte[] buffer,
                int offset,
                int count)
            {
                throw new NotSupportedException();
            }
        }
    }
}