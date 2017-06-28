using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Master40.DB.Migrations
{
    public partial class _3TableBOM : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBoms_Article_ArticleChildId",
                table: "ArticleBoms");

            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBoms_Article_ArticleParentId",
                table: "ArticleBoms");

            migrationBuilder.DropForeignKey(
                name: "FK_ArticleToDemand_Demand_DemandId",
                table: "ArticleToDemand");

            migrationBuilder.DropForeignKey(
                name: "FK_ArticleToWorkSchedule_OperationCharts_WorkScheduleId",
                table: "ArticleToWorkSchedule");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandOrder_Demand_DemandId",
                table: "DemandOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandOrder_OrderParts_OrderPartId",
                table: "DemandOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandPurchase_Demand_DemandId",
                table: "DemandPurchase");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandPurchase_PurchaseParts_PurchasePartId",
                table: "DemandPurchase");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandStock_Demand_DemandId",
                table: "DemandStock");

            migrationBuilder.DropForeignKey(
                name: "FK_Machines_MachineGroup_MachineGroupId",
                table: "Machines");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrder_Article_ArticleId",
                table: "ProductionOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrderBom_ProductionOrder_ParentProductionOrderId",
                table: "ProductionOrderBom");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrderBom_ProductionOrder_ProductionOrderId",
                table: "ProductionOrderBom");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrderToProductionOrderWorkSchedule_ProductionOrder_ProductionOrderId",
                table: "ProductionOrderToProductionOrderWorkSchedule");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrderToProductionOrderWorkSchedule_ProductionOrderWorkSchedule_ProductionOrderWorkScheduleId",
                table: "ProductionOrderToProductionOrderWorkSchedule");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrderWorkSchedule_MachineGroup_MachineGroupId",
                table: "ProductionOrderWorkSchedule");

            migrationBuilder.DropForeignKey(
                name: "FK_OperationCharts_MachineGroup_MachineGroupId",
                table: "OperationCharts");

            migrationBuilder.DropForeignKey(
                name: "FK_OperationCharts_MachineTools_MachineToolId",
                table: "OperationCharts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OperationCharts",
                table: "OperationCharts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductionOrderToProductionOrderWorkSchedule",
                table: "ProductionOrderToProductionOrderWorkSchedule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductionOrderBom",
                table: "ProductionOrderBom");

            migrationBuilder.DropIndex(
                name: "IX_ProductionOrderBom_ParentProductionOrderId",
                table: "ProductionOrderBom");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductionOrder",
                table: "ProductionOrder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MachineGroup",
                table: "MachineGroup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DemandPurchase",
                table: "DemandPurchase");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DemandOrder",
                table: "DemandOrder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Demand",
                table: "Demand");

            migrationBuilder.DropIndex(
                name: "IX_ArticleBoms_ArticleChildId",
                table: "ArticleBoms");

            migrationBuilder.DropIndex(
                name: "IX_ArticleBoms_ArticleParentId",
                table: "ArticleBoms");

            migrationBuilder.DropColumn(
                name: "End",
                table: "ProductionOrderBom");

            migrationBuilder.DropColumn(
                name: "ParentProductionOrderId",
                table: "ProductionOrderBom");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ProductionOrderBom");

            migrationBuilder.DropColumn(
                name: "Start",
                table: "ProductionOrderBom");

            migrationBuilder.DropColumn(
                name: "ArticleChildId",
                table: "ArticleBoms");

            migrationBuilder.DropColumn(
                name: "ArticleParentId",
                table: "ArticleBoms");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ArticleBoms");

            migrationBuilder.RenameTable(
                name: "OperationCharts",
                newName: "WorkSchedule");

            migrationBuilder.RenameTable(
                name: "ProductionOrderToProductionOrderWorkSchedule",
                newName: "ProductionOrderToProductionOrderWorkSchedules");

            migrationBuilder.RenameTable(
                name: "ProductionOrderBom",
                newName: "ProductionOrderBoms");

            migrationBuilder.RenameTable(
                name: "ProductionOrder",
                newName: "ProductionOrders");

            migrationBuilder.RenameTable(
                name: "MachineGroup",
                newName: "MachineGroups");

            migrationBuilder.RenameTable(
                name: "DemandPurchase",
                newName: "DemandPurchases");

            migrationBuilder.RenameTable(
                name: "DemandOrder",
                newName: "DemandOrders");

            migrationBuilder.RenameTable(
                name: "Demand",
                newName: "Demands");

            migrationBuilder.RenameIndex(
                name: "IX_OperationCharts_MachineToolId",
                table: "WorkSchedule",
                newName: "IX_WorkSchedule_MachineToolId");

            migrationBuilder.RenameIndex(
                name: "IX_OperationCharts_MachineGroupId",
                table: "WorkSchedule",
                newName: "IX_WorkSchedule_MachineGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionOrderToProductionOrderWorkSchedule_ProductionOrderWorkScheduleId",
                table: "ProductionOrderToProductionOrderWorkSchedules",
                newName: "IX_ProductionOrderToProductionOrderWorkSchedules_ProductionOrderWorkScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionOrderToProductionOrderWorkSchedule_ProductionOrderId",
                table: "ProductionOrderToProductionOrderWorkSchedules",
                newName: "IX_ProductionOrderToProductionOrderWorkSchedules_ProductionOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionOrderBom_ProductionOrderId",
                table: "ProductionOrderBoms",
                newName: "IX_ProductionOrderBoms_ProductionOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionOrder_ArticleId",
                table: "ProductionOrders",
                newName: "IX_ProductionOrders_ArticleId");

            migrationBuilder.RenameIndex(
                name: "IX_DemandPurchase_PurchasePartId",
                table: "DemandPurchases",
                newName: "IX_DemandPurchases_PurchasePartId");

            migrationBuilder.RenameIndex(
                name: "IX_DemandPurchase_DemandId",
                table: "DemandPurchases",
                newName: "IX_DemandPurchases_DemandId");

            migrationBuilder.RenameIndex(
                name: "IX_DemandOrder_OrderPartId",
                table: "DemandOrders",
                newName: "IX_DemandOrders_OrderPartId");

            migrationBuilder.RenameIndex(
                name: "IX_DemandOrder_DemandId",
                table: "DemandOrders",
                newName: "IX_DemandOrders_DemandId");

            migrationBuilder.AddColumn<int>(
                name: "ArticleId",
                table: "ArticleBoms",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkSchedule",
                table: "WorkSchedule",
                column: "WorkScheduleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductionOrderToProductionOrderWorkSchedules",
                table: "ProductionOrderToProductionOrderWorkSchedules",
                column: "ProductionOrderToProductionOrderWorkScheduleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductionOrderBoms",
                table: "ProductionOrderBoms",
                column: "ProductionOrderBomId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductionOrders",
                table: "ProductionOrders",
                column: "ProductionOrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MachineGroups",
                table: "MachineGroups",
                column: "MachineGroupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DemandPurchases",
                table: "DemandPurchases",
                column: "DemandPurchseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DemandOrders",
                table: "DemandOrders",
                column: "DemandOrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Demands",
                table: "Demands",
                column: "DemandId");

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
                name: "IX_ArticleBoms_ArticleId",
                table: "ArticleBoms",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleBomItems_ArticleBomId",
                table: "ArticleBomItems",
                column: "ArticleBomId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleBomItems_ArticleId",
                table: "ArticleBomItems",
                column: "ArticleId");

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
                name: "FK_ArticleToDemand_Demands_DemandId",
                table: "ArticleToDemand",
                column: "DemandId",
                principalTable: "Demands",
                principalColumn: "DemandId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleToWorkSchedule_WorkSchedule_WorkScheduleId",
                table: "ArticleToWorkSchedule",
                column: "WorkScheduleId",
                principalTable: "WorkSchedule",
                principalColumn: "WorkScheduleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DemandOrders_Demands_DemandId",
                table: "DemandOrders",
                column: "DemandId",
                principalTable: "Demands",
                principalColumn: "DemandId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DemandOrders_OrderParts_OrderPartId",
                table: "DemandOrders",
                column: "OrderPartId",
                principalTable: "OrderParts",
                principalColumn: "OrderPartId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DemandPurchases_Demands_DemandId",
                table: "DemandPurchases",
                column: "DemandId",
                principalTable: "Demands",
                principalColumn: "DemandId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DemandPurchases_PurchaseParts_PurchasePartId",
                table: "DemandPurchases",
                column: "PurchasePartId",
                principalTable: "PurchaseParts",
                principalColumn: "PurchasePartId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DemandStock_Demands_DemandId",
                table: "DemandStock",
                column: "DemandId",
                principalTable: "Demands",
                principalColumn: "DemandId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_MachineGroups_MachineGroupId",
                table: "Machines",
                column: "MachineGroupId",
                principalTable: "MachineGroups",
                principalColumn: "MachineGroupId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrders_Article_ArticleId",
                table: "ProductionOrders",
                column: "ArticleId",
                principalTable: "Article",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrderBoms_ProductionOrders_ProductionOrderId",
                table: "ProductionOrderBoms",
                column: "ProductionOrderId",
                principalTable: "ProductionOrders",
                principalColumn: "ProductionOrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrderToProductionOrderWorkSchedules_ProductionOrders_ProductionOrderId",
                table: "ProductionOrderToProductionOrderWorkSchedules",
                column: "ProductionOrderId",
                principalTable: "ProductionOrders",
                principalColumn: "ProductionOrderId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrderToProductionOrderWorkSchedules_ProductionOrderWorkSchedule_ProductionOrderWorkScheduleId",
                table: "ProductionOrderToProductionOrderWorkSchedules",
                column: "ProductionOrderWorkScheduleId",
                principalTable: "ProductionOrderWorkSchedule",
                principalColumn: "ProductionOrderWorkScheduleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrderWorkSchedule_MachineGroups_MachineGroupId",
                table: "ProductionOrderWorkSchedule",
                column: "MachineGroupId",
                principalTable: "MachineGroups",
                principalColumn: "MachineGroupId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkSchedule_MachineGroups_MachineGroupId",
                table: "WorkSchedule",
                column: "MachineGroupId",
                principalTable: "MachineGroups",
                principalColumn: "MachineGroupId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkSchedule_MachineTools_MachineToolId",
                table: "WorkSchedule",
                column: "MachineToolId",
                principalTable: "MachineTools",
                principalColumn: "MachineToolId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleBoms_Article_ArticleId",
                table: "ArticleBoms");

            migrationBuilder.DropForeignKey(
                name: "FK_ArticleToDemand_Demands_DemandId",
                table: "ArticleToDemand");

            migrationBuilder.DropForeignKey(
                name: "FK_ArticleToWorkSchedule_WorkSchedule_WorkScheduleId",
                table: "ArticleToWorkSchedule");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandOrders_Demands_DemandId",
                table: "DemandOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandOrders_OrderParts_OrderPartId",
                table: "DemandOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandPurchases_Demands_DemandId",
                table: "DemandPurchases");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandPurchases_PurchaseParts_PurchasePartId",
                table: "DemandPurchases");

            migrationBuilder.DropForeignKey(
                name: "FK_DemandStock_Demands_DemandId",
                table: "DemandStock");

            migrationBuilder.DropForeignKey(
                name: "FK_Machines_MachineGroups_MachineGroupId",
                table: "Machines");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrders_Article_ArticleId",
                table: "ProductionOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrderBoms_ProductionOrders_ProductionOrderId",
                table: "ProductionOrderBoms");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrderToProductionOrderWorkSchedules_ProductionOrders_ProductionOrderId",
                table: "ProductionOrderToProductionOrderWorkSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrderToProductionOrderWorkSchedules_ProductionOrderWorkSchedule_ProductionOrderWorkScheduleId",
                table: "ProductionOrderToProductionOrderWorkSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrderWorkSchedule_MachineGroups_MachineGroupId",
                table: "ProductionOrderWorkSchedule");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkSchedule_MachineGroups_MachineGroupId",
                table: "WorkSchedule");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkSchedule_MachineTools_MachineToolId",
                table: "WorkSchedule");

            migrationBuilder.DropTable(
                name: "ArticleBomItems");

            migrationBuilder.DropTable(
                name: "ProductionOrderBomItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkSchedule",
                table: "WorkSchedule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductionOrderToProductionOrderWorkSchedules",
                table: "ProductionOrderToProductionOrderWorkSchedules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductionOrderBoms",
                table: "ProductionOrderBoms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductionOrders",
                table: "ProductionOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MachineGroups",
                table: "MachineGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DemandPurchases",
                table: "DemandPurchases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DemandOrders",
                table: "DemandOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Demands",
                table: "Demands");

            migrationBuilder.DropIndex(
                name: "IX_ArticleBoms_ArticleId",
                table: "ArticleBoms");

            migrationBuilder.DropColumn(
                name: "ArticleId",
                table: "ArticleBoms");

            migrationBuilder.RenameTable(
                name: "WorkSchedule",
                newName: "OperationCharts");

            migrationBuilder.RenameTable(
                name: "ProductionOrderToProductionOrderWorkSchedules",
                newName: "ProductionOrderToProductionOrderWorkSchedule");

            migrationBuilder.RenameTable(
                name: "ProductionOrderBoms",
                newName: "ProductionOrderBom");

            migrationBuilder.RenameTable(
                name: "ProductionOrders",
                newName: "ProductionOrder");

            migrationBuilder.RenameTable(
                name: "MachineGroups",
                newName: "MachineGroup");

            migrationBuilder.RenameTable(
                name: "DemandPurchases",
                newName: "DemandPurchase");

            migrationBuilder.RenameTable(
                name: "DemandOrders",
                newName: "DemandOrder");

            migrationBuilder.RenameTable(
                name: "Demands",
                newName: "Demand");

            migrationBuilder.RenameIndex(
                name: "IX_WorkSchedule_MachineToolId",
                table: "OperationCharts",
                newName: "IX_OperationCharts_MachineToolId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkSchedule_MachineGroupId",
                table: "OperationCharts",
                newName: "IX_OperationCharts_MachineGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionOrderToProductionOrderWorkSchedules_ProductionOrderWorkScheduleId",
                table: "ProductionOrderToProductionOrderWorkSchedule",
                newName: "IX_ProductionOrderToProductionOrderWorkSchedule_ProductionOrderWorkScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionOrderToProductionOrderWorkSchedules_ProductionOrderId",
                table: "ProductionOrderToProductionOrderWorkSchedule",
                newName: "IX_ProductionOrderToProductionOrderWorkSchedule_ProductionOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionOrderBoms_ProductionOrderId",
                table: "ProductionOrderBom",
                newName: "IX_ProductionOrderBom_ProductionOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionOrders_ArticleId",
                table: "ProductionOrder",
                newName: "IX_ProductionOrder_ArticleId");

            migrationBuilder.RenameIndex(
                name: "IX_DemandPurchases_PurchasePartId",
                table: "DemandPurchase",
                newName: "IX_DemandPurchase_PurchasePartId");

            migrationBuilder.RenameIndex(
                name: "IX_DemandPurchases_DemandId",
                table: "DemandPurchase",
                newName: "IX_DemandPurchase_DemandId");

            migrationBuilder.RenameIndex(
                name: "IX_DemandOrders_OrderPartId",
                table: "DemandOrder",
                newName: "IX_DemandOrder_OrderPartId");

            migrationBuilder.RenameIndex(
                name: "IX_DemandOrders_DemandId",
                table: "DemandOrder",
                newName: "IX_DemandOrder_DemandId");

            migrationBuilder.AddColumn<int>(
                name: "End",
                table: "ProductionOrderBom",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParentProductionOrderId",
                table: "ProductionOrderBom",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "ProductionOrderBom",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Start",
                table: "ProductionOrderBom",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ArticleChildId",
                table: "ArticleBoms",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArticleParentId",
                table: "ArticleBoms",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "ArticleBoms",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OperationCharts",
                table: "OperationCharts",
                column: "WorkScheduleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductionOrderToProductionOrderWorkSchedule",
                table: "ProductionOrderToProductionOrderWorkSchedule",
                column: "ProductionOrderToProductionOrderWorkScheduleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductionOrderBom",
                table: "ProductionOrderBom",
                column: "ProductionOrderBomId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductionOrder",
                table: "ProductionOrder",
                column: "ProductionOrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MachineGroup",
                table: "MachineGroup",
                column: "MachineGroupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DemandPurchase",
                table: "DemandPurchase",
                column: "DemandPurchseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DemandOrder",
                table: "DemandOrder",
                column: "DemandOrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Demand",
                table: "Demand",
                column: "DemandId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderBom_ParentProductionOrderId",
                table: "ProductionOrderBom",
                column: "ParentProductionOrderId");

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
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleToDemand_Demand_DemandId",
                table: "ArticleToDemand",
                column: "DemandId",
                principalTable: "Demand",
                principalColumn: "DemandId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleToWorkSchedule_OperationCharts_WorkScheduleId",
                table: "ArticleToWorkSchedule",
                column: "WorkScheduleId",
                principalTable: "OperationCharts",
                principalColumn: "WorkScheduleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DemandOrder_Demand_DemandId",
                table: "DemandOrder",
                column: "DemandId",
                principalTable: "Demand",
                principalColumn: "DemandId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DemandOrder_OrderParts_OrderPartId",
                table: "DemandOrder",
                column: "OrderPartId",
                principalTable: "OrderParts",
                principalColumn: "OrderPartId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DemandPurchase_Demand_DemandId",
                table: "DemandPurchase",
                column: "DemandId",
                principalTable: "Demand",
                principalColumn: "DemandId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DemandPurchase_PurchaseParts_PurchasePartId",
                table: "DemandPurchase",
                column: "PurchasePartId",
                principalTable: "PurchaseParts",
                principalColumn: "PurchasePartId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DemandStock_Demand_DemandId",
                table: "DemandStock",
                column: "DemandId",
                principalTable: "Demand",
                principalColumn: "DemandId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_MachineGroup_MachineGroupId",
                table: "Machines",
                column: "MachineGroupId",
                principalTable: "MachineGroup",
                principalColumn: "MachineGroupId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrder_Article_ArticleId",
                table: "ProductionOrder",
                column: "ArticleId",
                principalTable: "Article",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrderBom_ProductionOrder_ParentProductionOrderId",
                table: "ProductionOrderBom",
                column: "ParentProductionOrderId",
                principalTable: "ProductionOrder",
                principalColumn: "ProductionOrderId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrderBom_ProductionOrder_ProductionOrderId",
                table: "ProductionOrderBom",
                column: "ProductionOrderId",
                principalTable: "ProductionOrder",
                principalColumn: "ProductionOrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrderToProductionOrderWorkSchedule_ProductionOrder_ProductionOrderId",
                table: "ProductionOrderToProductionOrderWorkSchedule",
                column: "ProductionOrderId",
                principalTable: "ProductionOrder",
                principalColumn: "ProductionOrderId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrderToProductionOrderWorkSchedule_ProductionOrderWorkSchedule_ProductionOrderWorkScheduleId",
                table: "ProductionOrderToProductionOrderWorkSchedule",
                column: "ProductionOrderWorkScheduleId",
                principalTable: "ProductionOrderWorkSchedule",
                principalColumn: "ProductionOrderWorkScheduleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrderWorkSchedule_MachineGroup_MachineGroupId",
                table: "ProductionOrderWorkSchedule",
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

            migrationBuilder.AddForeignKey(
                name: "FK_OperationCharts_MachineTools_MachineToolId",
                table: "OperationCharts",
                column: "MachineToolId",
                principalTable: "MachineTools",
                principalColumn: "MachineToolId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
