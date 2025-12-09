using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cmt_proje.Migrations
{
    /// <inheritdoc />
    public partial class AddContactFieldsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecondaryEmail",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecondaryEmail",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WhatsAppNumber",
                table: "AspNetUsers");
        }
    }
}
