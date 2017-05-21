using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Master40.DB.Repository;

namespace Master40.Migrations
{
    [DbContext(typeof(MasterDBContext))]
    [Migration("20170424175247_ArticleBomParts")]
    partial class ArticleBomParts
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

                    b.Property<int?>("ArticleId");

                    b.Property<string>("Name");

                    b.HasKey("ArticleBomId");

                    b.HasIndex("ArticleId")
                        .IsUnique();

                    b.ToTable("ArticleBoms");
                });

            modelBuilder.Entity("Master40.Models.DB.ArticleBomPart", b =>
                {
                    b.Property<int>("ArticleBomPartsId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ArticleBomId");

                    b.Property<int>("ArticleId");

                    b.Property<double>("Count");

                    b.Property<string>("Name");

                    b.Property<int?>("ParrentArticleBomPartId");

                    b.HasKey("ArticleBomPartsId");

                    b.HasIndex("ArticleBomId");

                    b.HasIndex("ArticleId");

                    b.HasIndex("ParrentArticleBomPartId");

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
                    b.HasOne("Master40.Models.DB.Article", "Article")
                        .WithOne("ArticleBom")
                        .HasForeignKey("Master40.Models.DB.ArticleBom", "ArticleId");
                });

            modelBuilder.Entity("Master40.Models.DB.ArticleBomPart", b =>
                {
                    b.HasOne("Master40.Models.DB.ArticleBom", "ArticleBom")
                        .WithMany("ArticleBomParts")
                        .HasForeignKey("ArticleBomId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master40.Models.DB.Article", "Article")
                        .WithMany()
                        .HasForeignKey("ArticleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master40.Models.DB.ArticleBomPart", "ParrentArticleBomPart")
                        .WithMany("ChildArticleBomParts")
                        .HasForeignKey("ParrentArticleBomPartId");
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
