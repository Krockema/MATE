using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Master40.DB.Migrations
{
    public partial class menuItemExtension : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OperationCharts_ArticleBomParts_ArticleBomPartId",
                table: "OperationCharts");

            migrationBuilder.DropTable(
                name: "ArticleBomParts");

            migrationBuilder.DropIndex(
                name: "IX_OperationCharts_ArticleBomPartId",
                table: "OperationCharts");

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "MenuItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Symbol",
                table: "MenuItems",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Symbol",
                table: "MenuItems");

            migrationBuilder.CreateTable(
                name: "ArticleBomParts",
                columns: table => new
                {
                    ArticleBomPartsId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleBomParts", x => x.ArticleBomPartsId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperationCharts_ArticleBomPartId",
                table: "OperationCharts",
                column: "ArticleBomPartId");

            migrationBuilder.AddForeignKey(
                name: "FK_OperationCharts_ArticleBomParts_ArticleBomPartId",
                table: "OperationCharts",
                column: "ArticleBomPartId",
                principalTable: "ArticleBomParts",
                principalColumn: "ArticleBomPartsId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
