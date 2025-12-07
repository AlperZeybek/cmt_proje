using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cmt_proje.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlAndLinkFieldsToAboutContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if columns exist before adding them
            var sql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[AboutContents]') AND name = 'ImageUrl')
                BEGIN
                    ALTER TABLE [AboutContents] ADD [ImageUrl] nvarchar(500) NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[AboutContents]') AND name = 'LinkText')
                BEGIN
                    ALTER TABLE [AboutContents] ADD [LinkText] nvarchar(200) NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[AboutContents]') AND name = 'LinkUrl')
                BEGIN
                    ALTER TABLE [AboutContents] ADD [LinkUrl] nvarchar(500) NULL;
                END
            ";

            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "AboutContents");

            migrationBuilder.DropColumn(
                name: "LinkText",
                table: "AboutContents");

            migrationBuilder.DropColumn(
                name: "LinkUrl",
                table: "AboutContents");
        }
    }
}
