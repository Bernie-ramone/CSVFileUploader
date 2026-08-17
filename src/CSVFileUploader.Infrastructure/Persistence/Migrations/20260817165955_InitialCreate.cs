using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSVFileUploader.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportedRecords",
                columns: table => new
                {
                    RecordId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssetId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceSite = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DestinationSite = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportedRecords", x => x.RecordId);
                });

            migrationBuilder.CreateIndex(
                name: "UX_ImportedRecords_BusinessKey",
                table: "ImportedRecords",
                columns: new[] { "AssetId", "SourceSite", "DestinationSite", "EventDate", "Volume" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportedRecords");
        }
    }
}
