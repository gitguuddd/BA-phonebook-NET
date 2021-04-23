using Microsoft.EntityFrameworkCore.Migrations;

namespace PhonebookAPI_dotnet.Migrations
{
    public partial class added_userId_to_phonebook_entries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "PhonebookEntries",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhonebookEntries_UserId",
                table: "PhonebookEntries",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PhonebookEntries_AspNetUsers_UserId",
                table: "PhonebookEntries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhonebookEntries_AspNetUsers_UserId",
                table: "PhonebookEntries");

            migrationBuilder.DropIndex(
                name: "IX_PhonebookEntries_UserId",
                table: "PhonebookEntries");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PhonebookEntries");
        }
    }
}
