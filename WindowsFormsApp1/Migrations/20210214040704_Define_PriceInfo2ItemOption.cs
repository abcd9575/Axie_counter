using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WindowsFormsApp1.Migrations
{
    public partial class Define_PriceInfo2ItemOption : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Server",
                table: "PriceInfo",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ItemOptions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    Stat = table.Column<string>(nullable: false),
                    Value = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceInfoItemOption",
                columns: table => new
                {
                    PriceInfoId = table.Column<int>(nullable: false),
                    ItemOptionId = table.Column<int>(nullable: false),
                    Id = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceInfoItemOption", x => new { x.ItemOptionId, x.PriceInfoId });
                    table.ForeignKey(
                        name: "FK_PriceInfoItemOption_ItemOptions_ItemOptionId",
                        column: x => x.ItemOptionId,
                        principalTable: "ItemOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PriceInfoItemOption_PriceInfo_PriceInfoId",
                        column: x => x.PriceInfoId,
                        principalTable: "PriceInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PriceInfoItemOption_PriceInfoId",
                table: "PriceInfoItemOption",
                column: "PriceInfoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriceInfoItemOption");

            migrationBuilder.DropTable(
                name: "ItemOptions");

            migrationBuilder.DropColumn(
                name: "Server",
                table: "PriceInfo");
        }
    }
}
