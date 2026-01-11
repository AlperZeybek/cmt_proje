-- Fix HomeHeroContents table columns to allow unlimited length
USE [cmt_proje];
GO

ALTER TABLE [HomeHeroContents]
ALTER COLUMN [HeroTitle] nvarchar(max) NOT NULL;
GO

ALTER TABLE [HomeHeroContents]
ALTER COLUMN [HeroSubtitle] nvarchar(max) NOT NULL;
GO

-- Insert migration record if not exists
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260111140000_IncreaseHomeHeroContentLength')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260111140000_IncreaseHomeHeroContentLength', '8.0.22');
END
GO

