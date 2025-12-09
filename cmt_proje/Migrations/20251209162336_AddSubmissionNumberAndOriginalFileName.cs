using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cmt_proje.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionNumberAndOriginalFileName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmissionNumber",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "SubmissionNumber",
                table: "Submissions");
        }
    }
}
