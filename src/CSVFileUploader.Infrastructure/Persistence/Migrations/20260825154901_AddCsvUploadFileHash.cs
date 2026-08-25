using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSVFileUploader.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCsvUploadFileHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileHash",
                table: "CsvUploads",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CsvUploads_FileHash",
                table: "CsvUploads",
                column: "FileHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CsvUploads_FileHash",
                table: "CsvUploads");

            migrationBuilder.DropColumn(
                name: "FileHash",
                table: "CsvUploads");
        }
    }
}
