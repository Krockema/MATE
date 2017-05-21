using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Master40.Migrations
{
    public partial class ArticleBom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBom_Article_ArticleId",
                table: "ArticleBom");

            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBom_ArticleBom_ParrentArticleBomId",
                table: "ArticleBom");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArticleBom",
                table: "ArticleBom");

            migrationBuilder.RenameTable(
                name: "ArticleBom",
                newName: "ArticleBoms");

            migrationBuilder.RenameIndex(
                name: "IX_ArticleBom_ParrentArticleBomId",
                table: "ArticleBoms",
                newName: "IX_ArticleBoms_ParrentArticleBomId");

            migrationBuilder.RenameIndex(
                name: "IX_ArticleBom_ArticleId",
                table: "ArticleBoms",
                newName: "IX_ArticleBoms_ArticleId");

            migrationBuilder.AlterColumn<double>(
                name: "Count",
                table: "ArticleBoms",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ArticleBoms",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArticleBoms",
                table: "ArticleBoms",
                column: "ArticleBomId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleBoms_Article_ArticleId",
                table: "ArticleBoms",
                column: "ArticleId",
                principalTable: "Article",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleBoms_ArticleBoms_ParrentArticleBomId",
                table: "ArticleBoms",
                column: "ParrentArticleBomId",
                principalTable: "ArticleBoms",
                principalColumn: "ArticleBomId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBoms_Article_ArticleId",
                table: "ArticleBoms");

            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBoms_ArticleBoms_ParrentArticleBomId",
                table: "ArticleBoms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArticleBoms",
                table: "ArticleBoms");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ArticleBoms");

            migrationBuilder.RenameTable(
                name: "ArticleBoms",
                newName: "ArticleBom");

            migrationBuilder.RenameIndex(
                name: "IX_ArticleBoms_ParrentArticleBomId",
                table: "ArticleBom",
                newName: "IX_ArticleBom_ParrentArticleBomId");

            migrationBuilder.RenameIndex(
                name: "IX_ArticleBoms_ArticleId",
                table: "ArticleBom",
                newName: "IX_ArticleBom_ArticleId");

            migrationBuilder.AlterColumn<int>(
                name: "Count",
                table: "ArticleBom",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArticleBom",
                table: "ArticleBom",
                column: "ArticleBomId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleBom_Article_ArticleId",
                table: "ArticleBom",
                column: "ArticleId",
                principalTable: "Article",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleBom_ArticleBom_ParrentArticleBomId",
                table: "ArticleBom",
                column: "ParrentArticleBomId",
                principalTable: "ArticleBom",
                principalColumn: "ArticleBomId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
