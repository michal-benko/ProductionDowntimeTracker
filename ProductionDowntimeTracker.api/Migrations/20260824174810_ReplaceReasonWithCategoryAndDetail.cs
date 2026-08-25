using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductionDowntimeTracker.api.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceReasonWithCategoryAndDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                table: "DowntimeRecords");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "DowntimeRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Detail",
                table: "DowntimeRecords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DowntimeCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DowntimeCategories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DowntimeCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Porucha senzoru" },
                    { 2, "Problém s PLC logikou" },
                    { 3, "Zaseknutý robot" },
                    { 4, "Ostatní" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DowntimeRecords_CategoryId",
                table: "DowntimeRecords",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_DowntimeRecords_DowntimeCategories_CategoryId",
                table: "DowntimeRecords",
                column: "CategoryId",
                principalTable: "DowntimeCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DowntimeRecords_DowntimeCategories_CategoryId",
                table: "DowntimeRecords");

            migrationBuilder.DropTable(
                name: "DowntimeCategories");

            migrationBuilder.DropIndex(
                name: "IX_DowntimeRecords_CategoryId",
                table: "DowntimeRecords");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "DowntimeRecords");

            migrationBuilder.DropColumn(
                name: "Detail",
                table: "DowntimeRecords");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "DowntimeRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
