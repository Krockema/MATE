using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Master40.Data.Context;

namespace Master40.Migrations
{
    [DbContext(typeof(MasterDBContext))]
    [Migration("20170506163606_3TableBOM")]
    partial class _3TableBOM
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Master40.Models.DB.Article", b =>
                {
                    b.Property<int>("ArticleId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ArticleTypeId");

                    b.Property<DateTime>("CreationDate");

                    b.Property<int>("DeliveryPeriod");

                    b.Property<string>("Name");

                    b.Property<double>("Price");

                    b.Property<int>("UnitId");

                    b.HasKey("ArticleId");

                    b.HasIndex("ArticleTypeId");

                    b.HasIndex("UnitId");

                    b.ToTable("Article");
                });

            modelBuilder.Entity("Master40.Models.DB.ArticleBom", b =>
                {
                    b.Property<int>("ArticleBomId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ArticleId");

                    b.Property<string>("Name");

                    b.HasKey("ArticleBomId");

                    b.HasIndex("ArticleId");

                    b.ToTable("ArticleBoms");
                });

            modelBuilder.Entity("Master40.Models.DB.ArticleBomItem", b =>
                {
                    b.Property<int>("ArticleBomItemId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ArticleBomId");

                    b.Property<int>("ArticleId");

                    b.Property<string>("Name");

                    b.Property<decimal>("Quantity");

                    b.HasKey("ArticleBomItemId");

                    b.HasIndex("ArticleBomId");

                    b.HasIndex("ArticleId");

                    b.ToTable("ArticleBomItems");
                });

            modelBuilder.Entity("Master40.Models.DB.ArticleToDemand", b =>
                {
                    b.Property<int>("ArticleToDemandId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ArticleId");

                    b.Property<int>("DemandId");

                    b.HasKey("ArticleToDemandId");

                    b.HasIndex("ArticleId");

                    b.HasIndex("DemandId");

                    b.ToTable("ArticleToDemand");
                });

            modelBuilder.Entity("Master40.Models.DB.ArticleToWorkSchedule", b =>
                {
                    b.Property<int>("ArticleId");

                    b.Property<int>("WorkScheduleId");

                    b.Property<int>("Duration");

                    b.HasKey("ArticleId", "WorkScheduleId");

                    b.HasIndex("WorkScheduleId");

                    b.ToTable("ArticleToWorkSchedule");
                });

            modelBuilder.Entity("Master40.Models.DB.ArticleType", b =>
                {
                    b.Property<int>("ArticleTypeId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("ArticleTypeId");

                    b.ToTable("ArticleTypes");
                });

            modelBuilder.Entity("Master40.Models.DB.BusinessPartner", b =>
                {
                    b.Property<int>("BusinessPartnerId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Debitor");

                    b.Property<bool>("Kreditor");

                    b.Property<string>("Name");

                    b.HasKey("BusinessPartnerId");

                    b.ToTable("BusinessPartners");
                });

            modelBuilder.Entity("Master40.Models.DB.Demand", b =>
                {
                    b.Property<int>("DemandId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ArticleToDemandId");

                    b.Property<int>("Quantity");

                    b.HasKey("DemandId");

                    b.ToTable("Demands");
                });

            modelBuilder.Entity("Master40.Models.DB.DemandOrder", b =>
                {
                    b.Property<int>("DemandOrderId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DemandId");

                    b.Property<int>("OrderPartId");

                    b.Property<int>("Quantity");

                    b.HasKey("DemandOrderId");

                    b.HasIndex("DemandId")
                        .IsUnique();

                    b.HasIndex("OrderPartId");

                    b.ToTable("DemandOrders");
                });

            modelBuilder.Entity("Master40.Models.DB.DemandPurchase", b =>
                {
                    b.Property<int>("DemandPurchseId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DemandId");

                    b.Property<int>("PurchasePartId");

                    b.Property<int>("Quantity");

                    b.HasKey("DemandPurchseId");

                    b.HasIndex("DemandId")
                        .IsUnique();

                    b.HasIndex("PurchasePartId");

                    b.ToTable("DemandPurchases");
                });

            modelBuilder.Entity("Master40.Models.DB.DemandStock", b =>
                {
                    b.Property<int>("DemandStockId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DemandId");

                    b.Property<int>("Quantity");

                    b.Property<int>("StockId");

                    b.HasKey("DemandStockId");

                    b.HasIndex("DemandId")
                        .IsUnique();

                    b.HasIndex("StockId");

                    b.ToTable("DemandStock");
                });

            modelBuilder.Entity("Master40.Models.DB.Machine", b =>
                {
                    b.Property<int>("MachineId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Capacity");

                    b.Property<int?>("MachineGroupId");

                    b.Property<string>("Name");

                    b.HasKey("MachineId");

                    b.HasIndex("MachineGroupId");

                    b.ToTable("Machines");
                });

            modelBuilder.Entity("Master40.Models.DB.MachineGroup", b =>
                {
                    b.Property<int>("MachineGroupId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Count");

                    b.Property<string>("Name");

                    b.Property<int>("WorkScheduleItemId");

                    b.HasKey("MachineGroupId");

                    b.ToTable("MachineGroups");
                });

            modelBuilder.Entity("Master40.Models.DB.MachineTool", b =>
                {
                    b.Property<int>("MachineToolId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("MachineId");

                    b.Property<int>("SetupTime");

                    b.HasKey("MachineToolId");

                    b.HasIndex("MachineId");

                    b.ToTable("MachineTools");
                });

            modelBuilder.Entity("Master40.Models.DB.Order", b =>
                {
                    b.Property<int>("OrderId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BusinessPartnerId");

                    b.Property<int>("DueTime");

                    b.Property<string>("Name");

                    b.HasKey("OrderId");

                    b.HasIndex("BusinessPartnerId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("Master40.Models.DB.OrderPart", b =>
                {
                    b.Property<int>("OrderPartId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ArticleId");

                    b.Property<int>("OrderId");

                    b.Property<int>("Quantity");

                    b.HasKey("OrderPartId");

                    b.HasIndex("ArticleId");

                    b.HasIndex("OrderId");

                    b.ToTable("OrderParts");
                });

            modelBuilder.Entity("Master40.Models.DB.ProductionOrder", b =>
                {
                    b.Property<int>("ProductionOrderId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ArticleId");

                    b.Property<string>("Name");

                    b.Property<int>("Quantity");

                    b.HasKey("ProductionOrderId");

                    b.HasIndex("ArticleId");

                    b.ToTable("ProductionOrders");
                });

            modelBuilder.Entity("Master40.Models.DB.ProductionOrderBom", b =>
                {
                    b.Property<int>("ProductionOrderBomId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<int>("ProductionOrderId");

                    b.HasKey("ProductionOrderBomId");

                    b.HasIndex("ProductionOrderId");

                    b.ToTable("ProductionOrderBoms");
                });

            modelBuilder.Entity("Master40.Models.DB.ProductionOrderBomItem", b =>
                {
                    b.Property<int>("ProductionOrderBomItemId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("End");

                    b.Property<string>("Name");

                    b.Property<int>("ProductionOderBomId");

                    b.Property<int>("ProductionOrderId");

                    b.Property<decimal>("Quantity");

                    b.Property<int>("Start");

                    b.HasKey("ProductionOrderBomItemId");

                    b.HasIndex("ProductionOderBomId");

                    b.HasIndex("ProductionOrderId");

                    b.ToTable("ProductionOrderBomItems");
                });

            modelBuilder.Entity("Master40.Models.DB.ProductionOrderToProductionOrderWorkSchedule", b =>
                {
                    b.Property<int>("ProductionOrderToProductionOrderWorkScheduleId")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("ProductionOrderId");

                    b.Property<int?>("ProductionOrderWorkScheduleId");

                    b.HasKey("ProductionOrderToProductionOrderWorkScheduleId");

                    b.HasIndex("ProductionOrderId");

                    b.HasIndex("ProductionOrderWorkScheduleId");

                    b.ToTable("ProductionOrderToProductionOrderWorkSchedules");
                });

            modelBuilder.Entity("Master40.Models.DB.ProductionOrderWorkSchedule", b =>
                {
                    b.Property<int>("ProductionOrderWorkScheduleId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Duration");

                    b.Property<int>("HirachieNumber");

                    b.Property<int?>("MachineGroupId");

                    b.Property<int?>("MachineToolId");

                    b.Property<string>("Name");

                    b.HasKey("ProductionOrderWorkScheduleId");

                    b.HasIndex("MachineGroupId");

                    b.HasIndex("MachineToolId");

                    b.ToTable("ProductionOrderWorkSchedule");
                });

            modelBuilder.Entity("Master40.Models.DB.Purchase", b =>
                {
                    b.Property<int>("PurchaseId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BusinessPartnerId");

                    b.Property<int>("DueTime");

                    b.Property<string>("Name");

                    b.HasKey("PurchaseId");

                    b.HasIndex("BusinessPartnerId");

                    b.ToTable("Purchases");
                });

            modelBuilder.Entity("Master40.Models.DB.PurchasePart", b =>
                {
                    b.Property<int>("PurchasePartId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ArticleId");

                    b.Property<int>("PurchaseId");

                    b.Property<int>("Quantity");

                    b.HasKey("PurchasePartId");

                    b.HasIndex("ArticleId");

                    b.HasIndex("PurchaseId");

                    b.ToTable("PurchaseParts");
                });

            modelBuilder.Entity("Master40.Models.DB.Stock", b =>
                {
                    b.Property<int>("StockId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ArticleForeignKey");

                    b.Property<decimal>("Current");

                    b.Property<decimal>("Max");

                    b.Property<decimal>("Min");

                    b.Property<string>("Name");

                    b.HasKey("StockId");

                    b.HasIndex("ArticleForeignKey")
                        .IsUnique();

                    b.ToTable("Stock");
                });

            modelBuilder.Entity("Master40.Models.DB.Unit", b =>
                {
                    b.Property<int>("UnitId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("UnitId");

                    b.ToTable("Units");
                });

            modelBuilder.Entity("Master40.Models.DB.WorkSchedule", b =>
                {
                    b.Property<int>("WorkScheduleId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Duration");

                    b.Property<int>("HirachieNumber");

                    b.Property<int?>("MachineGroupId");

                    b.Property<int?>("MachineToolId");

                    b.Property<string>("Name");

                    b.HasKey("WorkScheduleId");

                    b.HasIndex("MachineGroupId")
                        .IsUnique();

                    b.HasIndex("MachineToolId");

                    b.ToTable("WorkSchedule");
                });

            modelBuilder.Entity("Master40.Models.Menu", b =>
                {
                    b.Property<int>("MenuId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("MenuName");

                    b.HasKey("MenuId");

                    b.ToTable("Menus");
                });

            modelBuilder.Entity("Master40.Models.MenuItem", b =>
                {
                    b.Property<int>("MenuItemId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Action");

                    b.Property<string>("LinkUrl")
                        .HasMaxLength(255);

                    b.Property<int>("MenuId");

                    b.Property<int?>("MenuOrder");

                    b.Property<string>("MenuText")
                        .HasMaxLength(50);

                    b.Property<int?>("ParentMenuItemId");

                    b.Property<string>("Symbol");

                    b.HasKey("MenuItemId");

                    b.HasIndex("MenuId");

                    b.HasIndex("ParentMenuItemId");

                    b.ToTable("MenuItems");
                });

            modelBuilder.Entity("Master40.Models.DB.Article", b =>
                {
                    b.HasOne("Master40.Models.DB.ArticleType", "ArticleType")
                        .WithMany()
                        .HasForeignKey("ArticleTypeId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master40.Models.DB.Unit", "Unit")
                        .WithMany()
                        .HasForeignKey("UnitId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master40.Models.DB.ArticleBom", b =>
                {
                    b.HasOne("Master40.Models.DB.Article", "Article")
                        .WithMany("ArticleBoms")
                        .HasForeignKey("ArticleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master40.Models.DB.ArticleBomItem", b =>
                {
                    b.HasOne("Master40.Models.DB.ArticleBom", "ArticleBom")
                        .WithMany("ArticleBomItems")
                        .HasForeignKey("ArticleBomId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master40.Models.DB.Article", "Article")
                        .WithMany("ArticleBomItems")
                        .HasForeignKey("ArticleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master40.Models.DB.ArticleToDemand", b =>
                {
                    b.HasOne("Master40.Models.DB.Article", "Article")
                        .WithMany("ArtilceToDemand")
                        .HasForeignKey("ArticleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master40.Models.DB.Demand", "Demand")
                        .WithMany("ArtilceToDemand")
                        .HasForeignKey("DemandId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master40.Models.DB.ArticleToWorkSchedule", b =>
                {
                    b.HasOne("Master40.Models.DB.Article", "Article")
                        .WithMany("ArticleToWorkSchedules")
                        .HasForeignKey("ArticleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master40.Models.DB.WorkSchedule", "WorkSchedule")
                        .WithMany("ArticleToWorkSchedules")
                        .HasForeignKey("WorkScheduleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master40.Models.DB.DemandOrder", b =>
                {
                    b.HasOne("Master40.Models.DB.Demand", "Demand")
                        .WithOne("DemandOrders")
                        .HasForeignKey("Master40.Models.DB.DemandOrder", "DemandId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master40.Models.DB.OrderPart", "OrderPart")
                        .WithMany("DemandOdrders")
                        .HasForeignKey("OrderPartId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master40.Models.DB.DemandPurchase", b =>
                {
                    b.HasOne("Master40.Models.DB.Demand", "Demand")
                        .WithOne("DemandPurchases")
                        .HasForeignKey("Master40.Models.DB.DemandPurchase", "DemandId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master40.Models.DB.PurchasePart", "PurchasePart")
                        .WithMany("DemandPurchases")
                        .HasForeignKey("PurchasePartId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master40.Models.DB.DemandStock", b =>
                {
                    b.HasOne("Master40.Models.DB.Demand", "Demand")
                        .WithOne("DemandStocks")
                        .HasForeignKey("Master40.Models.DB.DemandStock", "DemandId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master40.Models.DB.Stock", "Stock")
                        .WithMany("DemandStock")
                        .HasForeignKey("StockId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master40.Models.DB.Machine", b =>
                {
                    b.HasOne("Master40.Models.DB.MachineGroup")
                        .WithMany("Machines")
                        .HasForeignKey("MachineGroupId");
                });

            modelBuilder.Entity("Master40.Models.DB.MachineTool", b =>
                {
                    b.HasOne("Master40.Models.DB.Machine", "Machine")
                        .WithMany("MachineTools")
                        .HasForeignKey("MachineId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master40.Models.DB.Order", b =>
                {
                    b.HasOne("Master40.Models.DB.BusinessPartner", "BusinessPartner")
                        .WithMany("Orders")
                        .HasForeignKey("BusinessPartnerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master40.Models.DB.OrderPart", b =>
                {
                    b.HasOne("Master40.Models.DB.Article", "Article")
                        .WithMany()
                        .HasForeignKey("ArticleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master40.Models.DB.Order", "Order")
                        .WithMany("OrderParts")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master40.Models.DB.ProductionOrder", b =>
                {
                    b.HasOne("Master40.Models.DB.Article", "Article")
                        .WithMany("ProductionOrders")
                        .HasForeignKey("ArticleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master40.Models.DB.ProductionOrderBom", b =>
                {
                    b.HasOne("Master40.Models.DB.ProductionOrder", "ProductionOrder")
                        .WithMany("ProductionOrderBoms")
                        .HasForeignKey("ProductionOrderId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master40.Models.DB.ProductionOrderBomItem", b =>
                {
                    b.HasOne("Master40.Models.DB.ProductionOrderBom", "ProductionOderBom")
                        .WithMany("ProductionOrderBomItems")
                        .HasForeignKey("ProductionOderBomId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master40.Models.DB.ProductionOrder", "ProductionOrder")
                        .WithMany("ProductionOrderBomItems")
                        .HasForeignKey("ProductionOrderId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master40.Models.DB.ProductionOrderToProductionOrderWorkSchedule", b =>
                {
                    b.HasOne("Master40.Models.DB.ProductionOrder", "ProductionOrder")
                        .WithMany("ProductionOrderToProductionOrderWorkSchedule")
                        .HasForeignKey("ProductionOrderId");

                    b.HasOne("Master40.Models.DB.ProductionOrderWorkSchedule", "ProductionOrderWorkSchedule")
                        .WithMany("ProductionOrderToWorkSchedules")
                        .HasForeignKey("ProductionOrderWorkScheduleId");
                });

            modelBuilder.Entity("Master40.Models.DB.ProductionOrderWorkSchedule", b =>
                {
                    b.HasOne("Master40.Models.DB.MachineGroup", "MachineGroup")
                        .WithMany()
                        .HasForeignKey("MachineGroupId");

                    b.HasOne("Master40.Models.DB.MachineTool", "MachineTool")
                        .WithMany()
                        .HasForeignKey("MachineToolId");
                });

            modelBuilder.Entity("Master40.Models.DB.Purchase", b =>
                {
                    b.HasOne("Master40.Models.DB.BusinessPartner", "BusinessPartner")
                        .WithMany("Purchases")
                        .HasForeignKey("BusinessPartnerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master40.Models.DB.PurchasePart", b =>
                {
                    b.HasOne("Master40.Models.DB.Article", "Article")
                        .WithMany()
                        .HasForeignKey("ArticleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master40.Models.DB.Purchase", "Purchase")
                        .WithMany("PurchaseParts")
                        .HasForeignKey("PurchaseId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master40.Models.DB.Stock", b =>
                {
                    b.HasOne("Master40.Models.DB.Article", "Article")
                        .WithOne("Stock")
                        .HasForeignKey("Master40.Models.DB.Stock", "ArticleForeignKey")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master40.Models.DB.WorkSchedule", b =>
                {
                    b.HasOne("Master40.Models.DB.MachineGroup", "MachineGroup")
                        .WithOne("WorkSchedule")
                        .HasForeignKey("Master40.Models.DB.WorkSchedule", "MachineGroupId");

                    b.HasOne("Master40.Models.DB.MachineTool", "MachineTool")
                        .WithMany()
                        .HasForeignKey("MachineToolId");
                });

            modelBuilder.Entity("Master40.Models.MenuItem", b =>
                {
                    b.HasOne("Master40.Models.Menu", "Menu")
                        .WithMany("MenuItems")
                        .HasForeignKey("MenuId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master40.Models.MenuItem", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentMenuItemId");
                });
        }
    }
}
