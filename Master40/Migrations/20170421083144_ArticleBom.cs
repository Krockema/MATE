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
                name: "FK_ArticleBom_Article_ArticleID",
                table: "ArticleBom");

            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBom_ArticleBom_ParrentArticleBomID",
                table: "ArticleBom");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArticleBom",
                table: "ArticleBom");

            migrationBuilder.RenameTable(
                name: "ArticleBom",
                newName: "ArticleBoms");

            migrationBuilder.RenameIndex(
                name: "IX_ArticleBom_ParrentArticleBomID",
                table: "ArticleBoms",
                newName: "IX_ArticleBoms_ParrentArticleBomID");

            migrationBuilder.RenameIndex(
                name: "IX_ArticleBom_ArticleID",
                table: "ArticleBoms",
                newName: "IX_ArticleBoms_ArticleID");

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
                column: "ArticleBomID");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleBoms_Article_ArticleID",
                table: "ArticleBoms",
                column: "ArticleID",
                principalTable: "Article",
                principalColumn: "ArticleID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleBoms_ArticleBoms_ParrentArticleBomID",
                table: "ArticleBoms",
                column: "ParrentArticleBomID",
                principalTable: "ArticleBoms",
                principalColumn: "ArticleBomID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBoms_Article_ArticleID",
                table: "ArticleBoms");

            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBoms_ArticleBoms_ParrentArticleBomID",
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
                name: "IX_ArticleBoms_ParrentArticleBomID",
                table: "ArticleBom",
                newName: "IX_ArticleBom_ParrentArticleBomID");

            migrationBuilder.RenameIndex(
                name: "IX_ArticleBoms_ArticleID",
                table: "ArticleBom",
                newName: "IX_ArticleBom_ArticleID");

            migrationBuilder.AlterColumn<int>(
                name: "Count",
                table: "ArticleBom",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArticleBom",
                table: "ArticleBom",
                column: "ArticleBomID");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleBom_Article_ArticleID",
                table: "ArticleBom",
                column: "ArticleID",
                principalTable: "Article",
                principalColumn: "ArticleID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleBom_ArticleBom_ParrentArticleBomID",
                table: "ArticleBom",
                column: "ParrentArticleBomID",
                principalTable: "ArticleBom",
                principalColumn: "ArticleBomID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
