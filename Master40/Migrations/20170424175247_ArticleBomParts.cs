using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Master40.Migrations
{
    public partial class ArticleBomParts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBoms_Article_ArticleID",
                table: "ArticleBoms");

            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBoms_ArticleBoms_ParrentArticleBomID",
                table: "ArticleBoms");

            migrationBuilder.DropIndex(
                name: "IX_ArticleBoms_ArticleID",
                table: "ArticleBoms");

            migrationBuilder.DropIndex(
                name: "IX_ArticleBoms_ParrentArticleBomID",
                table: "ArticleBoms");

            migrationBuilder.DropColumn(
                name: "Count",
                table: "ArticleBoms");

            migrationBuilder.DropColumn(
                name: "ParrentArticleBomID",
                table: "ArticleBoms");

            migrationBuilder.RenameColumn(
                name: "ArticleID",
                table: "ArticleBoms",
                newName: "ArticleId");

            migrationBuilder.RenameColumn(
                name: "ArticleBomID",
                table: "ArticleBoms",
                newName: "ArticleBomId");

            migrationBuilder.AlterColumn<int>(
                name: "ArticleId",
                table: "ArticleBoms",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.CreateTable(
                name: "ArticleBomParts",
                columns: table => new
                {
                    ArticleBomPartsId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ArticleBomId = table.Column<int>(nullable: false),
                    ArticleId = table.Column<int>(nullable: false),
                    Count = table.Column<double>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ParrentArticleBomPartId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleBomParts", x => x.ArticleBomPartsId);
                    table.ForeignKey(
                        name: "FK_ArticleBomParts_ArticleBoms_ArticleBomId",
                        column: x => x.ArticleBomId,
                        principalTable: "ArticleBoms",
                        principalColumn: "ArticleBomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleBomParts_Article_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Article",
                        principalColumn: "ArticleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleBomParts_ArticleBomParts_ParrentArticleBomPartId",
                        column: x => x.ParrentArticleBomPartId,
                        principalTable: "ArticleBomParts",
                        principalColumn: "ArticleBomPartsId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleBoms_ArticleId",
                table: "ArticleBoms",
                column: "ArticleId",
                unique: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleBoms_Article_ArticleId",
                table: "ArticleBoms",
                column: "ArticleId",
                principalTable: "Article",
                principalColumn: "ArticleID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBoms_Article_ArticleId",
                table: "ArticleBoms");

            migrationBuilder.DropTable(
                name: "ArticleBomParts");

            migrationBuilder.DropIndex(
                name: "IX_ArticleBoms_ArticleId",
                table: "ArticleBoms");

            migrationBuilder.RenameColumn(
                name: "ArticleId",
                table: "ArticleBoms",
                newName: "ArticleID");

            migrationBuilder.RenameColumn(
                name: "ArticleBomId",
                table: "ArticleBoms",
                newName: "ArticleBomID");

            migrationBuilder.AlterColumn<int>(
                name: "ArticleID",
                table: "ArticleBoms",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Count",
                table: "ArticleBoms",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ParrentArticleBomID",
                table: "ArticleBoms",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleBoms_ArticleID",
                table: "ArticleBoms",
                column: "ArticleID");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleBoms_ParrentArticleBomID",
                table: "ArticleBoms",
                column: "ParrentArticleBomID");

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
    }
}
