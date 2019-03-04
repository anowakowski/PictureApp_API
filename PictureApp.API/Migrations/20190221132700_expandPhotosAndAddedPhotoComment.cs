using Microsoft.EntityFrameworkCore.Migrations;

namespace PictureApp.API.Migrations
{
    public partial class expandPhotosAndAddedPhotoComment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Subtitle",
                table: "Photo",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Photo",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PhotoComment",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: false),
                    PhotoId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotoComment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhotoComment_Photo_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "Photo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhotoComment_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhotoComment_PhotoId",
                table: "PhotoComment",
                column: "PhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_PhotoComment_UserId",
                table: "PhotoComment",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhotoComment");

            migrationBuilder.DropColumn(
                name: "Subtitle",
                table: "Photo");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Photo");
        }
    }
}
