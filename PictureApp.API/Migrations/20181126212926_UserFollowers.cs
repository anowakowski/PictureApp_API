﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace PictureApp.API.Migrations
{
    public partial class UserFollowers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFollower",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FollowerId = table.Column<int>(nullable: false),
                    FolloweeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFollower", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFollower_Users_FolloweeId",
                        column: x => x.FolloweeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFollower_Users_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFollower_FolloweeId",
                table: "UserFollower",
                column: "FolloweeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFollower_FollowerId",
                table: "UserFollower",
                column: "FollowerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFollower");
        }
    }
}
