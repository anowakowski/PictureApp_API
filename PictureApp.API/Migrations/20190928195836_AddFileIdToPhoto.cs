using Microsoft.EntityFrameworkCore.Migrations;

namespace PictureApp.API.Migrations
{
    public partial class AddFileIdToPhoto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileId",
                table: "Photo",
                nullable: true);            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileId",
                table: "Photo");
        }
    }
}