using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Master40.DB.Migrations
{
    public partial class TimeFixForOrderPurchase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryDateTime",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "Time",
                table: "Orders",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Time",
                table: "Orders");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDateTime",
                table: "Orders",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
