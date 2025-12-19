using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cmt_proje.Migrations
{
    /// <inheritdoc />
    public partial class LinkCommitteeToConference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM ScientificCommitteeMembers");

            migrationBuilder.AddColumn<int>(
                name: "ConferenceId",
                table: "ScientificCommitteeMembers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ScientificCommitteeMembers_ConferenceId",
                table: "ScientificCommitteeMembers",
                column: "ConferenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScientificCommitteeMembers_Conferences_ConferenceId",
                table: "ScientificCommitteeMembers",
                column: "ConferenceId",
                principalTable: "Conferences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScientificCommitteeMembers_Conferences_ConferenceId",
                table: "ScientificCommitteeMembers");

            migrationBuilder.DropIndex(
                name: "IX_ScientificCommitteeMembers_ConferenceId",
                table: "ScientificCommitteeMembers");

            migrationBuilder.DropColumn(
                name: "ConferenceId",
                table: "ScientificCommitteeMembers");
        }
    }
}
