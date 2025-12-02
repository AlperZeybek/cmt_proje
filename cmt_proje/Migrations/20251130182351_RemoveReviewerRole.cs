using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cmt_proje.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReviewerRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AspNetUserRoles tablosundan Reviewer rolüne atanmış tüm kullanıcıları kaldır
            migrationBuilder.Sql(@"
                DELETE FROM [AspNetUserRoles]
                WHERE [RoleId] IN (
                    SELECT [Id] FROM [AspNetRoles] WHERE [Name] = 'Reviewer'
                )
            ");

            // AspNetRoleClaims tablosundan Reviewer rolüne ait claim'leri kaldır
            migrationBuilder.Sql(@"
                DELETE FROM [AspNetRoleClaims]
                WHERE [RoleId] IN (
                    SELECT [Id] FROM [AspNetRoles] WHERE [Name] = 'Reviewer'
                )
            ");

            // Reviewer rolünü AspNetRoles tablosundan sil
            migrationBuilder.Sql(@"
                DELETE FROM [AspNetRoles]
                WHERE [Name] = 'Reviewer'
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alma işlemi: Reviewer rolünü yeniden oluştur
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = 'Reviewer')
                BEGIN
                    INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
                    VALUES (NEWID(), 'Reviewer', 'REVIEWER', NULL)
                END
            ");
        }
    }
}
