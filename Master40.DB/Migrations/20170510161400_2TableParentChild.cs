using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Master40.DBMigrations
{
    public partial class _2TableParentChild : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBoms_Article_ArticleId",
                table: "ArticleBoms");

            migrationBuilder.DropForeignKey(
                name: "FK_Machines_MachineGroups_MachineGroupId",
                table: "Machines");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrderBoms_ProductionOrders_ProductionOrderId",
                table: "ProductionOrderBoms");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkSchedule_MachineGroups_MachineGroupId",
                table: "WorkSchedule");

            migrationBuilder.DropTable(
                name: "ArticleBomItems");

            migrationBuilder.DropTable(
                name: "ArticleToWorkSchedule");

            migrationBuilder.DropTable(
                name: "ProductionOrderBomItems");

            migrationBuilder.DropIndex(
                name: "IX_WorkSchedule_MachineGroupId",
                table: "WorkSchedule");

            migrationBuilder.DropIndex(
                name: "IX_ProductionOrderBoms_ProductionOrderId",
                table: "ProductionOrderBoms");

            migrationBuilder.DropColumn(
                name: "Count",
                table: "MachineGroups");

            migrationBuilder.DropColumn(
                name: "WorkScheduleItemId",
                table: "MachineGroups");

            migrationBuilder.RenameColumn(
                name: "HirachieNumber",
                table: "WorkSchedule",
                newName: "HierarchyNumber");

            migrationBuilder.RenameColumn(
                name: "HirachieNumber",
                table: "ProductionOrderWorkSchedule",
                newName: "HierarchyNumber");

            migrationBuilder.RenameColumn(
                name: "ProductionOrderId",
                table: "ProductionOrderBoms",
                newName: "Start");

            migrationBuilder.RenameColumn(
                name: "ArticleId",
                table: "ArticleBoms",
                newName: "ArticleChildId");

            migrationBuilder.RenameIndex(
                name: "IX_ArticleBoms_ArticleId",
                table: "ArticleBoms",
                newName: "IX_ArticleBoms_ArticleChildId");

            migrationBuilder.AlterColumn<int>(
                name: "MachineGroupId",
                table: "WorkSchedule",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArticleId",
                table: "WorkSchedule",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "End",
                table: "ProductionOrderBoms",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductionOrderChildId",
                table: "ProductionOrderBoms",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductionOrderParentId",
                table: "ProductionOrderBoms",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "ProductionOrderBoms",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "ProductionOrders",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "MachineTools",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MachineGroupId",
                table: "Machines",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Capacity",
                table: "Machines",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Count",
                table: "Machines",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ArticleParentId",
                table: "ArticleBoms",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "ArticleBoms",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_WorkSchedule_ArticleId",
                table: "WorkSchedule",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSchedule_MachineGroupId",
                table: "WorkSchedule",
                column: "MachineGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderBoms_ProductionOrderChildId",
                table: "ProductionOrderBoms",
                column: "ProductionOrderChildId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderBoms_ProductionOrderParentId",
                table: "ProductionOrderBoms",
                column: "ProductionOrderParentId");

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
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleBoms_Article_ArticleParentId",
                table: "ArticleBoms",
                column: "ArticleParentId",
                principalTable: "Article",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_MachineGroups_MachineGroupId",
                table: "Machines",
                column: "MachineGroupId",
                principalTable: "MachineGroups",
                principalColumn: "MachineGroupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrderBoms_ProductionOrders_ProductionOrderChildId",
                table: "ProductionOrderBoms",
                column: "ProductionOrderChildId",
                principalTable: "ProductionOrders",
                principalColumn: "ProductionOrderId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrderBoms_ProductionOrders_ProductionOrderParentId",
                table: "ProductionOrderBoms",
                column: "ProductionOrderParentId",
                principalTable: "ProductionOrders",
                principalColumn: "ProductionOrderId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkSchedule_Article_ArticleId",
                table: "WorkSchedule",
                column: "ArticleId",
                principalTable: "Article",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkSchedule_MachineGroups_MachineGroupId",
                table: "WorkSchedule",
                column: "MachineGroupId",
                principalTable: "MachineGroups",
                principalColumn: "MachineGroupId",
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

            migrationBuilder.DropForeignKey(
                name: "FK_Machines_MachineGroups_MachineGroupId",
                table: "Machines");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrderBoms_ProductionOrders_ProductionOrderChildId",
                table: "ProductionOrderBoms");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrderBoms_ProductionOrders_ProductionOrderParentId",
                table: "ProductionOrderBoms");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkSchedule_Article_ArticleId",
                table: "WorkSchedule");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkSchedule_MachineGroups_MachineGroupId",
                table: "WorkSchedule");

            migrationBuilder.DropIndex(
                name: "IX_WorkSchedule_ArticleId",
                table: "WorkSchedule");

            migrationBuilder.DropIndex(
                name: "IX_WorkSchedule_MachineGroupId",
                table: "WorkSchedule");

            migrationBuilder.DropIndex(
                name: "IX_ProductionOrderBoms_ProductionOrderChildId",
                table: "ProductionOrderBoms");

            migrationBuilder.DropIndex(
                name: "IX_ProductionOrderBoms_ProductionOrderParentId",
                table: "ProductionOrderBoms");

            migrationBuilder.DropIndex(
                name: "IX_ArticleBoms_ArticleParentId",
                table: "ArticleBoms");

            migrationBuilder.DropColumn(
                name: "ArticleId",
                table: "WorkSchedule");

            migrationBuilder.DropColumn(
                name: "End",
                table: "ProductionOrderBoms");

            migrationBuilder.DropColumn(
                name: "ProductionOrderChildId",
                table: "ProductionOrderBoms");

            migrationBuilder.DropColumn(
                name: "ProductionOrderParentId",
                table: "ProductionOrderBoms");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ProductionOrderBoms");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "MachineTools");

            migrationBuilder.DropColumn(
                name: "Count",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "ArticleParentId",
                table: "ArticleBoms");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ArticleBoms");

            migrationBuilder.RenameColumn(
                name: "HierarchyNumber",
                table: "WorkSchedule",
                newName: "HirachieNumber");

            migrationBuilder.RenameColumn(
                name: "HierarchyNumber",
                table: "ProductionOrderWorkSchedule",
                newName: "HirachieNumber");

            migrationBuilder.RenameColumn(
                name: "Start",
                table: "ProductionOrderBoms",
                newName: "ProductionOrderId");

            migrationBuilder.RenameColumn(
                name: "ArticleChildId",
                table: "ArticleBoms",
                newName: "ArticleId");

            migrationBuilder.RenameIndex(
                name: "IX_ArticleBoms_ArticleChildId",
                table: "ArticleBoms",
                newName: "IX_ArticleBoms_ArticleId");

            migrationBuilder.AlterColumn<int>(
                name: "MachineGroupId",
                table: "WorkSchedule",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "ProductionOrders",
                nullable: false,
                oldClrType: typeof(decimal));

            migrationBuilder.AddColumn<int>(
                name: "Count",
                table: "MachineGroups",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkScheduleItemId",
                table: "MachineGroups",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "MachineGroupId",
                table: "Machines",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "Capacity",
                table: "Machines",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.CreateTable(
                name: "ArticleBomItems",
                columns: table => new
                {
                    ArticleBomItemId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ArticleBomId = table.Column<int>(nullable: false),
                    ArticleId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Quantity = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleBomItems", x => x.ArticleBomItemId);
                    table.ForeignKey(
                        name: "FK_ArticleBomItems_ArticleBoms_ArticleBomId",
                        column: x => x.ArticleBomId,
                        principalTable: "ArticleBoms",
                        principalColumn: "ArticleBomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleBomItems_Article_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Article",
                        principalColumn: "ArticleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticleToWorkSchedule",
                columns: table => new
                {
                    ArticleId = table.Column<int>(nullable: false),
                    WorkScheduleId = table.Column<int>(nullable: false),
                    Duration = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleToWorkSchedule", x => new { x.ArticleId, x.WorkScheduleId });
                    table.ForeignKey(
                        name: "FK_ArticleToWorkSchedule_Article_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Article",
                        principalColumn: "ArticleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleToWorkSchedule_WorkSchedule_WorkScheduleId",
                        column: x => x.WorkScheduleId,
                        principalTable: "WorkSchedule",
                        principalColumn: "WorkScheduleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOrderBomItems",
                columns: table => new
                {
                    ProductionOrderBomItemId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    End = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ProductionOderBomId = table.Column<int>(nullable: false),
                    ProductionOrderId = table.Column<int>(nullable: false),
                    Quantity = table.Column<decimal>(nullable: false),
                    Start = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrderBomItems", x => x.ProductionOrderBomItemId);
                    table.ForeignKey(
                        name: "FK_ProductionOrderBomItems_ProductionOrderBoms_ProductionOderBomId",
                        column: x => x.ProductionOderBomId,
                        principalTable: "ProductionOrderBoms",
                        principalColumn: "ProductionOrderBomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionOrderBomItems_ProductionOrders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "ProductionOrders",
                        principalColumn: "ProductionOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkSchedule_MachineGroupId",
                table: "WorkSchedule",
                column: "MachineGroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderBoms_ProductionOrderId",
                table: "ProductionOrderBoms",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleBomItems_ArticleBomId",
                table: "ArticleBomItems",
                column: "ArticleBomId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleBomItems_ArticleId",
                table: "ArticleBomItems",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleToWorkSchedule_WorkScheduleId",
                table: "ArticleToWorkSchedule",
                column: "WorkScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderBomItems_ProductionOderBomId",
                table: "ProductionOrderBomItems",
                column: "ProductionOderBomId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderBomItems_ProductionOrderId",
                table: "ProductionOrderBomItems",
                column: "ProductionOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleBoms_Article_ArticleId",
                table: "ArticleBoms",
                column: "ArticleId",
                principalTable: "Article",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_MachineGroups_MachineGroupId",
                table: "Machines",
                column: "MachineGroupId",
                principalTable: "MachineGroups",
                principalColumn: "MachineGroupId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrderBoms_ProductionOrders_ProductionOrderId",
                table: "ProductionOrderBoms",
                column: "ProductionOrderId",
                principalTable: "ProductionOrders",
                principalColumn: "ProductionOrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkSchedule_MachineGroups_MachineGroupId",
                table: "WorkSchedule",
                column: "MachineGroupId",
                principalTable: "MachineGroups",
                principalColumn: "MachineGroupId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
