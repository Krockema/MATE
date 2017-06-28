using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Master40.DB.Migrations
{
    public partial class FancyDemandTPH : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleToDemand_Demands_DemandId",
                table: "ArticleToDemand");

            migrationBuilder.DropTable(
                name: "DemandOrders");

            migrationBuilder.DropTable(
                name: "DemandPurchases");

            migrationBuilder.DropTable(
                name: "DemandStock");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArticleToDemand",
                table: "ArticleToDemand");

            migrationBuilder.DropIndex(
                name: "IX_ArticleToDemand_ArticleId",
                table: "ArticleToDemand");

            migrationBuilder.DropColumn(
                name: "ArticleToDemandId",
                table: "ArticleToDemand");

            migrationBuilder.RenameColumn(
                name: "DemandId",
                table: "ArticleToDemand",
                newName: "DemandToProviderId");

            migrationBuilder.RenameIndex(
                name: "IX_ArticleToDemand_DemandId",
                table: "ArticleToDemand",
                newName: "IX_ArticleToDemand_DemandToProviderId");

            migrationBuilder.RenameColumn(
                name: "ArticleToDemandId",
                table: "Demands",
                newName: "ArticleId");

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "ArticleToDemand",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DemandRequesterId",
                table: "Demands",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Demands",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsProvided",
                table: "Demands",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OrderPartId",
                table: "Demands",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductionOrderBomId",
                table: "Demands",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductionOrderId",
                table: "Demands",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchasePartId",
                table: "Demands",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StockId",
                table: "Demands",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArticleToDemand",
                table: "ArticleToDemand",
                columns: new[] { "ArticleId", "DemandToProviderId" });

            migrationBuilder.CreateIndex(
                name: "IX_Demands_ArticleId",
                table: "Demands",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_Demands_DemandRequesterId",
                table: "Demands",
                column: "DemandRequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Demands_OrderPartId",
                table: "Demands",
                column: "OrderPartId");

            migrationBuilder.CreateIndex(
                name: "IX_Demands_ProductionOrderBomId",
                table: "Demands",
                column: "ProductionOrderBomId");

            migrationBuilder.CreateIndex(
                name: "IX_Demands_ProductionOrderId",
                table: "Demands",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Demands_PurchasePartId",
                table: "Demands",
                column: "PurchasePartId");

            migrationBuilder.CreateIndex(
                name: "IX_Demands_StockId",
                table: "Demands",
                column: "StockId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleToDemand_Demands_DemandToProviderId",
                table: "ArticleToDemand",
                column: "DemandToProviderId",
                principalTable: "Demands",
                principalColumn: "DemandId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Demands_Article_ArticleId",
                table: "Demands",
                column: "ArticleId",
                principalTable: "Article",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Demands_Demands_DemandRequesterId",
                table: "Demands",
                column: "DemandRequesterId",
                principalTable: "Demands",
                principalColumn: "DemandId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Demands_OrderParts_OrderPartId",
                table: "Demands",
                column: "OrderPartId",
                principalTable: "OrderParts",
                principalColumn: "OrderPartId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Demands_ProductionOrderBoms_ProductionOrderBomId",
                table: "Demands",
                column: "ProductionOrderBomId",
                principalTable: "ProductionOrderBoms",
                principalColumn: "ProductionOrderBomId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Demands_ProductionOrders_ProductionOrderId",
                table: "Demands",
                column: "ProductionOrderId",
                principalTable: "ProductionOrders",
                principalColumn: "ProductionOrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Demands_PurchaseParts_PurchasePartId",
                table: "Demands",
                column: "PurchasePartId",
                principalTable: "PurchaseParts",
                principalColumn: "PurchasePartId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Demands_Stock_StockId",
                table: "Demands",
                column: "StockId",
                principalTable: "Stock",
                principalColumn: "StockId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleToDemand_Demands_DemandToProviderId",
                table: "ArticleToDemand");

            migrationBuilder.DropForeignKey(
                name: "FK_Demands_Article_ArticleId",
                table: "Demands");

            migrationBuilder.DropForeignKey(
                name: "FK_Demands_Demands_DemandRequesterId",
                table: "Demands");

            migrationBuilder.DropForeignKey(
                name: "FK_Demands_OrderParts_OrderPartId",
                table: "Demands");

            migrationBuilder.DropForeignKey(
                name: "FK_Demands_ProductionOrderBoms_ProductionOrderBomId",
                table: "Demands");

            migrationBuilder.DropForeignKey(
                name: "FK_Demands_ProductionOrders_ProductionOrderId",
                table: "Demands");

            migrationBuilder.DropForeignKey(
                name: "FK_Demands_PurchaseParts_PurchasePartId",
                table: "Demands");

            migrationBuilder.DropForeignKey(
                name: "FK_Demands_Stock_StockId",
                table: "Demands");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArticleToDemand",
                table: "ArticleToDemand");

            migrationBuilder.DropIndex(
                name: "IX_Demands_ArticleId",
                table: "Demands");

            migrationBuilder.DropIndex(
                name: "IX_Demands_DemandRequesterId",
                table: "Demands");

            migrationBuilder.DropIndex(
                name: "IX_Demands_OrderPartId",
                table: "Demands");

            migrationBuilder.DropIndex(
                name: "IX_Demands_ProductionOrderBomId",
                table: "Demands");

            migrationBuilder.DropIndex(
                name: "IX_Demands_ProductionOrderId",
                table: "Demands");

            migrationBuilder.DropIndex(
                name: "IX_Demands_PurchasePartId",
                table: "Demands");

            migrationBuilder.DropIndex(
                name: "IX_Demands_StockId",
                table: "Demands");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ArticleToDemand");

            migrationBuilder.DropColumn(
                name: "DemandRequesterId",
                table: "Demands");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Demands");

            migrationBuilder.DropColumn(
                name: "IsProvided",
                table: "Demands");

            migrationBuilder.DropColumn(
                name: "OrderPartId",
                table: "Demands");

            migrationBuilder.DropColumn(
                name: "ProductionOrderBomId",
                table: "Demands");

            migrationBuilder.DropColumn(
                name: "ProductionOrderId",
                table: "Demands");

            migrationBuilder.DropColumn(
                name: "PurchasePartId",
                table: "Demands");

            migrationBuilder.DropColumn(
                name: "StockId",
                table: "Demands");

            migrationBuilder.RenameColumn(
                name: "DemandToProviderId",
                table: "ArticleToDemand",
                newName: "DemandId");

            migrationBuilder.RenameIndex(
                name: "IX_ArticleToDemand_DemandToProviderId",
                table: "ArticleToDemand",
                newName: "IX_ArticleToDemand_DemandId");

            migrationBuilder.RenameColumn(
                name: "ArticleId",
                table: "Demands",
                newName: "ArticleToDemandId");

            migrationBuilder.AddColumn<int>(
                name: "ArticleToDemandId",
                table: "ArticleToDemand",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArticleToDemand",
                table: "ArticleToDemand",
                column: "ArticleToDemandId");

            migrationBuilder.CreateTable(
                name: "DemandOrders",
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
                    table.PrimaryKey("PK_DemandOrders", x => x.DemandOrderId);
                    table.ForeignKey(
                        name: "FK_DemandOrders_Demands_DemandId",
                        column: x => x.DemandId,
                        principalTable: "Demands",
                        principalColumn: "DemandId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemandOrders_OrderParts_OrderPartId",
                        column: x => x.OrderPartId,
                        principalTable: "OrderParts",
                        principalColumn: "OrderPartId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DemandPurchases",
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
                    table.PrimaryKey("PK_DemandPurchases", x => x.DemandPurchseId);
                    table.ForeignKey(
                        name: "FK_DemandPurchases_Demands_DemandId",
                        column: x => x.DemandId,
                        principalTable: "Demands",
                        principalColumn: "DemandId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemandPurchases_PurchaseParts_PurchasePartId",
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
                        name: "FK_DemandStock_Demands_DemandId",
                        column: x => x.DemandId,
                        principalTable: "Demands",
                        principalColumn: "DemandId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemandStock_Stock_StockId",
                        column: x => x.StockId,
                        principalTable: "Stock",
                        principalColumn: "StockId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleToDemand_ArticleId",
                table: "ArticleToDemand",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandOrders_DemandId",
                table: "DemandOrders",
                column: "DemandId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DemandOrders_OrderPartId",
                table: "DemandOrders",
                column: "OrderPartId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandPurchases_DemandId",
                table: "DemandPurchases",
                column: "DemandId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DemandPurchases_PurchasePartId",
                table: "DemandPurchases",
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

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleToDemand_Demands_DemandId",
                table: "ArticleToDemand",
                column: "DemandId",
                principalTable: "Demands",
                principalColumn: "DemandId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
