using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Master40.DB.Migrations
{
    public partial class DatabaseConceptV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBoms_Article_ArticleParentId",
                table: "ArticleBoms");

            migrationBuilder.DropForeignKey(
                name: "FK_OperationCharts_Machines_MachineId",
                table: "OperationCharts");

            migrationBuilder.DropIndex(
                name: "IX_OperationCharts_MachineId",
                table: "OperationCharts");

            migrationBuilder.RenameColumn(
                name: "MachineId",
                table: "OperationCharts",
                newName: "MachineGroupId");

            migrationBuilder.RenameColumn(
                name: "ArticleBomPartId",
                table: "OperationCharts",
                newName: "HirachieNumber");

            migrationBuilder.RenameColumn(
                name: "OperationChartId",
                table: "OperationCharts",
                newName: "WorkScheduleId");

            migrationBuilder.AddColumn<int>(
                name: "MachineGroupId",
                table: "Machines",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "ArticleBoms",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "ArticleParentId",
                table: "ArticleBoms",
                nullable: true,
                oldClrType: typeof(int));

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
                        name: "FK_ArticleToWorkSchedule_OperationCharts_WorkScheduleId",
                        column: x => x.WorkScheduleId,
                        principalTable: "OperationCharts",
                        principalColumn: "WorkScheduleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Demand",
                columns: table => new
                {
                    DemandId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ArticleToDemandId = table.Column<int>(nullable: false),
                    Quantity = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Demand", x => x.DemandId);
                });

            migrationBuilder.CreateTable(
                name: "MachineGroup",
                columns: table => new
                {
                    MachineGroupId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Count = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    WorkScheduleItemId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineGroup", x => x.MachineGroupId);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOrder",
                columns: table => new
                {
                    ProductionOrderId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ArticleId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Quantity = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrder", x => x.ProductionOrderId);
                    table.ForeignKey(
                        name: "FK_ProductionOrder_Article_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Article",
                        principalColumn: "ArticleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticleToDemand",
                columns: table => new
                {
                    ArticleToDemandId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ArticleId = table.Column<int>(nullable: false),
                    DemandId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleToDemand", x => x.ArticleToDemandId);
                    table.ForeignKey(
                        name: "FK_ArticleToDemand_Article_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Article",
                        principalColumn: "ArticleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleToDemand_Demand_DemandId",
                        column: x => x.DemandId,
                        principalTable: "Demand",
                        principalColumn: "DemandId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DemandOrder",
                columns: table => new
                {
                    DemandOrderId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DemandId = table.Column<int>(nullable: false),
                    OrderPartId = table.Column<int>(nullable: false),
                    Quantity = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandOrder", x => x.DemandOrderId);
                    table.ForeignKey(
                        name: "FK_DemandOrder_Demand_DemandId",
                        column: x => x.DemandId,
                        principalTable: "Demand",
                        principalColumn: "DemandId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemandOrder_OrderParts_OrderPartId",
                        column: x => x.OrderPartId,
                        principalTable: "OrderParts",
                        principalColumn: "OrderPartId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DemandPurchase",
                columns: table => new
                {
                    DemandPurchseId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DemandId = table.Column<int>(nullable: false),
                    PurchasePartId = table.Column<int>(nullable: false),
                    Quantity = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandPurchase", x => x.DemandPurchseId);
                    table.ForeignKey(
                        name: "FK_DemandPurchase_Demand_DemandId",
                        column: x => x.DemandId,
                        principalTable: "Demand",
                        principalColumn: "DemandId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemandPurchase_PurchaseParts_PurchasePartId",
                        column: x => x.PurchasePartId,
                        principalTable: "PurchaseParts",
                        principalColumn: "PurchasePartId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DemandStock",
                columns: table => new
                {
                    DemandStockId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DemandId = table.Column<int>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    StockId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandStock", x => x.DemandStockId);
                    table.ForeignKey(
                        name: "FK_DemandStock_Demand_DemandId",
                        column: x => x.DemandId,
                        principalTable: "Demand",
                        principalColumn: "DemandId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemandStock_Stock_StockId",
                        column: x => x.StockId,
                        principalTable: "Stock",
                        principalColumn: "StockId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOrderWorkSchedule",
                columns: table => new
                {
                    ProductionOrderWorkScheduleId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Duration = table.Column<int>(nullable: false),
                    HirachieNumber = table.Column<int>(nullable: false),
                    MachineGroupId = table.Column<int>(nullable: true),
                    MachineToolId = table.Column<int>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrderWorkSchedule", x => x.ProductionOrderWorkScheduleId);
                    table.ForeignKey(
                        name: "FK_ProductionOrderWorkSchedule_MachineGroup_MachineGroupId",
                        column: x => x.MachineGroupId,
                        principalTable: "MachineGroup",
                        principalColumn: "MachineGroupId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderWorkSchedule_MachineTools_MachineToolId",
                        column: x => x.MachineToolId,
                        principalTable: "MachineTools",
                        principalColumn: "MachineToolId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOrderBom",
                columns: table => new
                {
                    ProductionOrderBomId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    End = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ParentProductionOrderId = table.Column<int>(nullable: true),
                    ProductionOrderId = table.Column<int>(nullable: false),
                    Quantity = table.Column<decimal>(nullable: false),
                    Start = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrderBom", x => x.ProductionOrderBomId);
                    table.ForeignKey(
                        name: "FK_ProductionOrderBom_ProductionOrder_ParentProductionOrderId",
                        column: x => x.ParentProductionOrderId,
                        principalTable: "ProductionOrder",
                        principalColumn: "ProductionOrderId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderBom_ProductionOrder_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "ProductionOrder",
                        principalColumn: "ProductionOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOrderToProductionOrderWorkSchedule",
                columns: table => new
                {
                    ProductionOrderToProductionOrderWorkScheduleId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ProductionOrderId = table.Column<int>(nullable: true),
                    ProductionOrderWorkScheduleId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrderToProductionOrderWorkSchedule", x => x.ProductionOrderToProductionOrderWorkScheduleId);
                    table.ForeignKey(
                        name: "FK_ProductionOrderToProductionOrderWorkSchedule_ProductionOrder_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "ProductionOrder",
                        principalColumn: "ProductionOrderId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderToProductionOrderWorkSchedule_ProductionOrderWorkSchedule_ProductionOrderWorkScheduleId",
                        column: x => x.ProductionOrderWorkScheduleId,
                        principalTable: "ProductionOrderWorkSchedule",
                        principalColumn: "ProductionOrderWorkScheduleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Machines_MachineGroupId",
                table: "Machines",
                column: "MachineGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationCharts_MachineGroupId",
                table: "OperationCharts",
                column: "MachineGroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleToDemand_ArticleId",
                table: "ArticleToDemand",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleToDemand_DemandId",
                table: "ArticleToDemand",
                column: "DemandId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleToWorkSchedule_WorkScheduleId",
                table: "ArticleToWorkSchedule",
                column: "WorkScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandOrder_DemandId",
                table: "DemandOrder",
                column: "DemandId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DemandOrder_OrderPartId",
                table: "DemandOrder",
                column: "OrderPartId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandPurchase_DemandId",
                table: "DemandPurchase",
                column: "DemandId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DemandPurchase_PurchasePartId",
                table: "DemandPurchase",
                column: "PurchasePartId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandStock_DemandId",
                table: "DemandStock",
                column: "DemandId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DemandStock_StockId",
                table: "DemandStock",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrder_ArticleId",
                table: "ProductionOrder",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderBom_ParentProductionOrderId",
                table: "ProductionOrderBom",
                column: "ParentProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderBom_ProductionOrderId",
                table: "ProductionOrderBom",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderToProductionOrderWorkSchedule_ProductionOrderId",
                table: "ProductionOrderToProductionOrderWorkSchedule",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderToProductionOrderWorkSchedule_ProductionOrderWorkScheduleId",
                table: "ProductionOrderToProductionOrderWorkSchedule",
                column: "ProductionOrderWorkScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderWorkSchedule_MachineGroupId",
                table: "ProductionOrderWorkSchedule",
                column: "MachineGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderWorkSchedule_MachineToolId",
                table: "ProductionOrderWorkSchedule",
                column: "MachineToolId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleBoms_Article_ArticleParentId",
                table: "ArticleBoms",
                column: "ArticleParentId",
                principalTable: "Article",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_MachineGroup_MachineGroupId",
                table: "Machines",
                column: "MachineGroupId",
                principalTable: "MachineGroup",
                principalColumn: "MachineGroupId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OperationCharts_MachineGroup_MachineGroupId",
                table: "OperationCharts",
                column: "MachineGroupId",
                principalTable: "MachineGroup",
                principalColumn: "MachineGroupId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBoms_Article_ArticleParentId",
                table: "ArticleBoms");

            migrationBuilder.DropForeignKey(
                name: "FK_Machines_MachineGroup_MachineGroupId",
                table: "Machines");

            migrationBuilder.DropForeignKey(
                name: "FK_OperationCharts_MachineGroup_MachineGroupId",
                table: "OperationCharts");

            migrationBuilder.DropTable(
                name: "ArticleToDemand");

            migrationBuilder.DropTable(
                name: "ArticleToWorkSchedule");

            migrationBuilder.DropTable(
                name: "DemandOrder");

            migrationBuilder.DropTable(
                name: "DemandPurchase");

            migrationBuilder.DropTable(
                name: "DemandStock");

            migrationBuilder.DropTable(
                name: "ProductionOrderBom");

            migrationBuilder.DropTable(
                name: "ProductionOrderToProductionOrderWorkSchedule");

            migrationBuilder.DropTable(
                name: "Demand");

            migrationBuilder.DropTable(
                name: "ProductionOrder");

            migrationBuilder.DropTable(
                name: "ProductionOrderWorkSchedule");

            migrationBuilder.DropTable(
                name: "MachineGroup");

            migrationBuilder.DropIndex(
                name: "IX_Machines_MachineGroupId",
                table: "Machines");

            migrationBuilder.DropIndex(
                name: "IX_OperationCharts_MachineGroupId",
                table: "OperationCharts");

            migrationBuilder.DropColumn(
                name: "MachineGroupId",
                table: "Machines");

            migrationBuilder.RenameColumn(
                name: "MachineGroupId",
                table: "OperationCharts",
                newName: "MachineId");

            migrationBuilder.RenameColumn(
                name: "HirachieNumber",
                table: "OperationCharts",
                newName: "ArticleBomPartId");

            migrationBuilder.RenameColumn(
                name: "WorkScheduleId",
                table: "OperationCharts",
                newName: "OperationChartId");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "ArticleBoms",
                nullable: false,
                oldClrType: typeof(decimal));

            migrationBuilder.AlterColumn<int>(
                name: "ArticleParentId",
                table: "ArticleBoms",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperationCharts_MachineId",
                table: "OperationCharts",
                column: "MachineId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleBoms_Article_ArticleParentId",
                table: "ArticleBoms",
                column: "ArticleParentId",
                principalTable: "Article",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OperationCharts_Machines_MachineId",
                table: "OperationCharts",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "MachineId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
