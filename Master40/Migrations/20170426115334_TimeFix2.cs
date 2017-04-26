using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Master40.Migrations
{
    public partial class TimeFix2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryDateTime",
                table: "Purchases");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "Orders",
                newName: "DueTime");

            migrationBuilder.AddColumn<int>(
                name: "DueTime",
                table: "Purchases",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueTime",
                table: "Purchases");

            migrationBuilder.RenameColumn(
                name: "DueTime",
                table: "Orders",
                newName: "Time");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDateTime",
                table: "Purchases",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
