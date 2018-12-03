﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace PictureApp.API.Migrations
{
    public partial class AccountActivationTokenAndExtendUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAccountActivated",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AccountActivationToken",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(nullable: false),
                    Token = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountActivationToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountActivationToken_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountActivationToken_UserId",
                table: "AccountActivationToken",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountActivationToken");

            migrationBuilder.DropColumn(
                name: "IsAccountActivated",
                table: "Users");
        }
    }
}
