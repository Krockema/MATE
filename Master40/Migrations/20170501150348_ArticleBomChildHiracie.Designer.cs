using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Master40.Data;

namespace Master40.Migrations
{
    [DbContext(typeof(MasterDBContext))]
    [Migration("20170501150348_ArticleBomChildHiracie")]
    partial class ArticleBomChildHiracie
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

                    b.Property<int?>("ArticleChildId");

                    b.Property<int>("ArticleParentId");

                    b.Property<string>("Name");

                    b.Property<int>("Quantity");

                    b.HasKey("ArticleBomId");

                    b.HasIndex("ArticleChildId");

                    b.HasIndex("ArticleParentId");

                    b.ToTable("ArticleBoms");
                });

            modelBuilder.Entity("Master40.Models.DB.ArticleBomPart", b =>
                {
                    b.Property<int>("ArticleBomPartsId")
                        .ValueGeneratedOnAdd();

                    b.HasKey("ArticleBomPartsId");

                    b.ToTable("ArticleBomParts");
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

            modelBuilder.Entity("Master40.Models.DB.Machine", b =>
                {
                    b.Property<int>("MachineId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Capacity");

                    b.Property<string>("Name");

                    b.HasKey("MachineId");

                    b.ToTable("Machines");
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

            modelBuilder.Entity("Master40.Models.DB.OperationChart", b =>
                {
                    b.Property<int>("OperationChartId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ArticleBomPartId");

                    b.Property<int>("Duration");

                    b.Property<int?>("MachineId");

                    b.Property<int?>("MachineToolId");

                    b.Property<string>("Name");

                    b.HasKey("OperationChartId");

                    b.HasIndex("ArticleBomPartId");

                    b.HasIndex("MachineId");

                    b.HasIndex("MachineToolId");

                    b.ToTable("OperationCharts");
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

                    b.Property<int>("Quantity");

                    b.Property<int>("ArticleId");

                    b.Property<int>("OrderId");

                    b.HasKey("OrderPartId");

                    b.HasIndex("ArticleId");

                    b.HasIndex("OrderId");

                    b.ToTable("OrderParts");
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

                    b.Property<int>("Quantity");

                    b.Property<int>("ArticleId");

                    b.Property<int>("PurchaseId");

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

                    b.Property<string>("LinkUrl")
                        .HasMaxLength(255);

                    b.Property<int>("MenuId");

                    b.Property<int?>("MenuOrder");

                    b.Property<string>("MenuText")
                        .HasMaxLength(50);

                    b.Property<int?>("ParentMenuItemId");

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
                    b.HasOne("Master40.Models.DB.Article", "ArticleChild")
                        .WithMany("ArticleChilds")
                        .HasForeignKey("ArticleChildId");

                    b.HasOne("Master40.Models.DB.Article", "ArticleParent")
                        .WithMany("ArticleBoms")
                        .HasForeignKey("ArticleParentId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master40.Models.DB.MachineTool", b =>
                {
                    b.HasOne("Master40.Models.DB.Machine", "Machine")
                        .WithMany("MachineTools")
                        .HasForeignKey("MachineId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master40.Models.DB.OperationChart", b =>
                {
                    b.HasOne("Master40.Models.DB.ArticleBomPart", "ArticleBomPart")
                        .WithMany()
                        .HasForeignKey("ArticleBomPartId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master40.Models.DB.Machine", "Machine")
                        .WithMany()
                        .HasForeignKey("MachineId");

                    b.HasOne("Master40.Models.DB.MachineTool", "MachineTool")
                        .WithMany()
                        .HasForeignKey("MachineToolId");
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
