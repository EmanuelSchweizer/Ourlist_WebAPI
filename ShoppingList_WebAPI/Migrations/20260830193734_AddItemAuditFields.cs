using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingList_WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddItemAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BoughtAt",
                table: "ListItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BoughtByUserId",
                table: "ListItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "ListItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ListItems_BoughtByUserId",
                table: "ListItems",
                column: "BoughtByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ListItems_CreatedByUserId",
                table: "ListItems",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ListItems_Users_BoughtByUserId",
                table: "ListItems",
                column: "BoughtByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ListItems_Users_CreatedByUserId",
                table: "ListItems",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ListItems_Users_BoughtByUserId",
                table: "ListItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ListItems_Users_CreatedByUserId",
                table: "ListItems");

            migrationBuilder.DropIndex(
                name: "IX_ListItems_BoughtByUserId",
                table: "ListItems");

            migrationBuilder.DropIndex(
                name: "IX_ListItems_CreatedByUserId",
                table: "ListItems");

            migrationBuilder.DropColumn(
                name: "BoughtAt",
                table: "ListItems");

            migrationBuilder.DropColumn(
                name: "BoughtByUserId",
                table: "ListItems");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ListItems");
        }
    }
}
