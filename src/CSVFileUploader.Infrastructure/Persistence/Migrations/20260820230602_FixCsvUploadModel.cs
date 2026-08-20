using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSVFileUploader.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixCsvUploadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CsvUploads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UploadedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TotalRows = table.Column<int>(type: "int", nullable: false),
                    InsertedRows = table.Column<int>(type: "int", nullable: false),
                    DuplicateRows = table.Column<int>(type: "int", nullable: false),
                    ErrorRows = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CsvUploads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CsvUploadRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CsvUploadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CsvUploadRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CsvUploadRows_CsvUploads_CsvUploadId",
                        column: x => x.CsvUploadId,
                        principalTable: "CsvUploads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_CsvUploadRows_Upload_RowNumber",
                table: "CsvUploadRows",
                columns: new[] { "CsvUploadId", "RowNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CsvUploadRows");

            migrationBuilder.DropTable(
                name: "CsvUploads");
        }
    }
}
