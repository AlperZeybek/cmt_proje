using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cmt_proje.Migrations
{
    /// <inheritdoc />
    public partial class AddPresentationTypeToSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PresentationType",
                table: "Submissions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PresentationType",
                table: "Submissions");
        }
    }
}
