using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Master40.Data;

namespace Master40.Migrations
{
    [DbContext(typeof(MasterDBContext))]
    [Migration("20170422211423_Menu")]
    partial class Menu
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Master_DB_Test.Models.DB.Article", b =>
                {
                    b.Property<int>("ArticleID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ArticleTypeID");

                    b.Property<DateTime>("CreationDate");

                    b.Property<int>("DeliveryPeriod");

                    b.Property<string>("Name");

                    b.Property<double>("Price");

                    b.Property<int>("UnitID");

                    b.HasKey("ArticleID");

                    b.HasIndex("ArticleTypeID");

                    b.HasIndex("UnitID");

                    b.ToTable("Article");
                });

            modelBuilder.Entity("Master_DB_Test.Models.DB.ArticleBom", b =>
                {
                    b.Property<int>("ArticleBomID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ArticleID");

                    b.Property<double>("Count");

                    b.Property<string>("Name");

                    b.Property<int?>("ParrentArticleBomID");

                    b.HasKey("ArticleBomID");

                    b.HasIndex("ArticleID");

                    b.HasIndex("ParrentArticleBomID");

                    b.ToTable("ArticleBoms");
                });

            modelBuilder.Entity("Master_DB_Test.Models.DB.ArticleType", b =>
                {
                    b.Property<int>("ArticleTypeID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("ArticleTypeID");

                    b.ToTable("ArticleTypes");
                });

            modelBuilder.Entity("Master_DB_Test.Models.DB.Stock", b =>
                {
                    b.Property<int>("StockID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ArticleForeignKey");

                    b.Property<decimal>("Current");

                    b.Property<decimal>("Max");

                    b.Property<decimal>("Min");

                    b.Property<string>("Name");

                    b.HasKey("StockID");

                    b.HasIndex("ArticleForeignKey")
                        .IsUnique();

                    b.ToTable("Stock");
                });

            modelBuilder.Entity("Master_DB_Test.Models.DB.Unit", b =>
                {
                    b.Property<int>("UnitID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("UnitID");

                    b.ToTable("Units");
                });

            modelBuilder.Entity("Menu", b =>
                {
                    b.Property<int>("MenuId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("MenuName");

                    b.HasKey("MenuId");

                    b.ToTable("Menus");
                });

            modelBuilder.Entity("MenuItem", b =>
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

            modelBuilder.Entity("Master_DB_Test.Models.DB.Article", b =>
                {
                    b.HasOne("Master_DB_Test.Models.DB.ArticleType", "ArticleType")
                        .WithMany()
                        .HasForeignKey("ArticleTypeID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master_DB_Test.Models.DB.Unit", "Unit")
                        .WithMany()
                        .HasForeignKey("UnitID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Master_DB_Test.Models.DB.ArticleBom", b =>
                {
                    b.HasOne("Master_DB_Test.Models.DB.Article", "Article")
                        .WithMany("ArticleBom")
                        .HasForeignKey("ArticleID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Master_DB_Test.Models.DB.ArticleBom", "ParrentArticleBom")
                        .WithMany("ChildArticleBom")
                        .HasForeignKey("ParrentArticleBomID");
                });

            modelBuilder.Entity("Master_DB_Test.Models.DB.Stock", b =>
                {
                    b.HasOne("Master_DB_Test.Models.DB.Article", "Article")
                        .WithOne("Stock")
                        .HasForeignKey("Master_DB_Test.Models.DB.Stock", "ArticleForeignKey")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MenuItem", b =>
                {
                    b.HasOne("Menu", "Menu")
                        .WithMany("MenuItems")
                        .HasForeignKey("MenuId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("MenuItem", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentMenuItemId");
                });
        }
    }
}
