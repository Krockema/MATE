using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Master40.Migrations
{
    public partial class FirstFullDbConcept : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessPartners",
                columns: table => new
                {
                    BusinessPartnerId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Debitor = table.Column<bool>(nullable: false),
                    Kreditor = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessPartners", x => x.BusinessPartnerId);
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    MachineId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Capacity = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.MachineId);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BusinessPartnerId = table.Column<int>(nullable: false),
                    DeliveryDateTime = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Orders_BusinessPartners_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "BusinessPartnerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Purchases",
                columns: table => new
                {
                    PurchaseId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BusinessPartnerId = table.Column<int>(nullable: false),
                    DeliveryDateTime = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.PurchaseId);
                    table.ForeignKey(
                        name: "FK_Purchases_BusinessPartners_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "BusinessPartnerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MachineTools",
                columns: table => new
                {
                    MachineToolId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    MachineId = table.Column<int>(nullable: false),
                    SetupTime = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineTools", x => x.MachineToolId);
                    table.ForeignKey(
                        name: "FK_MachineTools_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderParts",
                columns: table => new
                {
                    OrderPartId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Quantity = table.Column<int>(nullable: false),
                    ArticleId = table.Column<int>(nullable: false),
                    OrderId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderParts", x => x.OrderPartId);
                    table.ForeignKey(
                        name: "FK_OrderParts_Article_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Article",
                        principalColumn: "ArticleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderParts_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseParts",
                columns: table => new
                {
                    PurchasePartId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Quantity = table.Column<int>(nullable: false),
                    ArticleId = table.Column<int>(nullable: false),
                    PurchaseId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseParts", x => x.PurchasePartId);
                    table.ForeignKey(
                        name: "FK_PurchaseParts_Article_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Article",
                        principalColumn: "ArticleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseParts_Purchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchases",
                        principalColumn: "PurchaseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OperationCharts",
                columns: table => new
                {
                    OperationChartId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ArticleBomPartId = table.Column<int>(nullable: false),
                    Duration = table.Column<int>(nullable: false),
                    MachineId = table.Column<int>(nullable: true),
                    MachineToolId = table.Column<int>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationCharts", x => x.OperationChartId);
                    table.ForeignKey(
                        name: "FK_OperationCharts_ArticleBomParts_ArticleBomPartId",
                        column: x => x.ArticleBomPartId,
                        principalTable: "ArticleBomParts",
                        principalColumn: "ArticleBomPartsId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OperationCharts_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperationCharts_MachineTools_MachineToolId",
                        column: x => x.MachineToolId,
                        principalTable: "MachineTools",
                        principalColumn: "MachineToolId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MachineTools_MachineId",
                table: "MachineTools",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationCharts_ArticleBomPartId",
                table: "OperationCharts",
                column: "ArticleBomPartId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationCharts_MachineId",
                table: "OperationCharts",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationCharts_MachineToolId",
                table: "OperationCharts",
                column: "MachineToolId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BusinessPartnerId",
                table: "Orders",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderParts_ArticleId",
                table: "OrderParts",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderParts_OrderId",
                table: "OrderParts",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_BusinessPartnerId",
                table: "Purchases",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseParts_ArticleId",
                table: "PurchaseParts",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseParts_PurchaseId",
                table: "PurchaseParts",
                column: "PurchaseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperationCharts");

            migrationBuilder.DropTable(
                name: "OrderParts");

            migrationBuilder.DropTable(
                name: "PurchaseParts");

            migrationBuilder.DropTable(
                name: "MachineTools");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Purchases");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "BusinessPartners");
        }
    }
}
