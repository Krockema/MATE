using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Master40.DB.Migrations
{
    public partial class ArticleBomChildHiracie : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBoms_Article_ArticleId",
                table: "ArticleBoms");

            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBomParts_ArticleBoms_ArticleBomId",
                table: "ArticleBomParts");

            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBomParts_Article_ArticleId",
                table: "ArticleBomParts");

            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBomParts_ArticleBomParts_ParrentArticleBomPartId",
                table: "ArticleBomParts");

            migrationBuilder.DropIndex(
                name: "IX_ArticleBomParts_ArticleBomId",
                table: "ArticleBomParts");

            migrationBuilder.DropIndex(
                name: "IX_ArticleBomParts_ArticleId",
                table: "ArticleBomParts");

            migrationBuilder.DropIndex(
                name: "IX_ArticleBomParts_ParrentArticleBomPartId",
                table: "ArticleBomParts");

            migrationBuilder.DropIndex(
                name: "IX_ArticleBoms_ArticleId",
                table: "ArticleBoms");

            migrationBuilder.DropColumn(
                name: "DeliveryDateTime",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "ArticleBomId",
                table: "ArticleBomParts");

            migrationBuilder.DropColumn(
                name: "ArticleId",
                table: "ArticleBomParts");

            migrationBuilder.DropColumn(
                name: "Count",
                table: "ArticleBomParts");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ArticleBomParts");

            migrationBuilder.DropColumn(
                name: "ParrentArticleBomPartId",
                table: "ArticleBomParts");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "Orders",
                newName: "DueTime");

            migrationBuilder.RenameColumn(
                name: "ArticleId",
                table: "ArticleBoms",
                newName: "ArticleChildId");

            migrationBuilder.AddColumn<int>(
                name: "DueTime",
                table: "Purchases",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ArticleParentId",
                table: "ArticleBoms",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "ArticleBoms",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleBoms_ArticleChildId",
                table: "ArticleBoms",
                column: "ArticleChildId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleBoms_ArticleParentId",
                table: "ArticleBoms",
                column: "ArticleParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleBoms_Article_ArticleChildId",
                table: "ArticleBoms",
                column: "ArticleChildId",
                principalTable: "Article",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleBoms_Article_ArticleParentId",
                table: "ArticleBoms",
                column: "ArticleParentId",
                principalTable: "Article",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBoms_Article_ArticleChildId",
                table: "ArticleBoms");

            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBoms_Article_ArticleParentId",
                table: "ArticleBoms");

            migrationBuilder.DropIndex(
                name: "IX_ArticleBoms_ArticleChildId",
                table: "ArticleBoms");

            migrationBuilder.DropIndex(
                name: "IX_ArticleBoms_ArticleParentId",
                table: "ArticleBoms");

            migrationBuilder.DropColumn(
                name: "DueTime",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "ArticleParentId",
                table: "ArticleBoms");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ArticleBoms");

            migrationBuilder.RenameColumn(
                name: "DueTime",
                table: "Orders",
                newName: "Time");

            migrationBuilder.RenameColumn(
                name: "ArticleChildId",
                table: "ArticleBoms",
                newName: "ArticleId");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDateTime",
                table: "Purchases",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ArticleBomId",
                table: "ArticleBomParts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ArticleId",
                table: "ArticleBomParts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Count",
                table: "ArticleBomParts",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ArticleBomParts",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParrentArticleBomPartId",
                table: "ArticleBomParts",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleBomParts_ArticleBomId",
                table: "ArticleBomParts",
                column: "ArticleBomId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleBomParts_ArticleId",
                table: "ArticleBomParts",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleBomParts_ParrentArticleBomPartId",
                table: "ArticleBomParts",
                column: "ParrentArticleBomPartId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleBoms_ArticleId",
                table: "ArticleBoms",
                column: "ArticleId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleBoms_Article_ArticleId",
                table: "ArticleBoms",
                column: "ArticleId",
                principalTable: "Article",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleBomParts_ArticleBoms_ArticleBomId",
                table: "ArticleBomParts",
                column: "ArticleBomId",
                principalTable: "ArticleBoms",
                principalColumn: "ArticleBomId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleBomParts_Article_ArticleId",
                table: "ArticleBomParts",
                column: "ArticleId",
                principalTable: "Article",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleBomParts_ArticleBomParts_ParrentArticleBomPartId",
                table: "ArticleBomParts",
                column: "ParrentArticleBomPartId",
                principalTable: "ArticleBomParts",
                principalColumn: "ArticleBomPartsId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
