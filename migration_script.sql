IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Cameras] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Location_Value] nvarchar(500) NOT NULL,
    [Location_Zone] nvarchar(100) NOT NULL,
    [Location_Latitude] float(10) NULL,
    [Location_Longitude] float(11) NULL,
    [Network_IpAddress] nvarchar(45) NOT NULL,
    [Network_Port] int NOT NULL,
    [Network_Username] nvarchar(100) NOT NULL,
    [Network_Password] nvarchar(255) NOT NULL,
    [Network_Type] int NOT NULL,
    [Type] int NOT NULL,
    [Status] int NOT NULL,
    [Connection_StreamUrl] nvarchar(1000) NOT NULL,
    [Connection_SnapshotUrl] nvarchar(1000) NULL,
    [Connection_BackupStreamUrl] nvarchar(1000) NULL,
    [Connection_IsConnected] bit NOT NULL,
    [Connection_ConnectedAt] datetime2 NOT NULL,
    [Connection_LastHeartbeat] datetime2 NULL,
    [Connection_Type] nvarchar(50) NOT NULL,
    [Connection_AdditionalInfo] nvarchar(max) NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [LastActiveAt] datetime2 NULL,
    [RowVersion] varbinary(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [UpdatedBy] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Cameras] PRIMARY KEY ([Id])
);

CREATE TABLE [Pages] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(100) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [Status] int NOT NULL,
    [StatusName] nvarchar(50) NOT NULL,
    [DisplayWidth] int NOT NULL,
    [DisplayHeight] int NOT NULL,
    [ThumbnailUrl] nvarchar(500) NULL,
    [DisplayOrientation] int NOT NULL,
    [DisplayOrientationName] nvarchar(50) NOT NULL,
    [BackgroundAssetUrl] nvarchar(500) NULL,
    [BackgroundAssetType] nvarchar(50) NULL,
    [BackgroundAssetAltText] nvarchar(200) NULL,
    [BackgroundAssetContent] TEXT NULL,
    [BackgroundAssetMetadata] TEXT NULL,
    [RowVersion] varbinary(max) NOT NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [UpdatedBy] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Pages] PRIMARY KEY ([Id])
);

CREATE TABLE [Tool] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [DefaultJs] nvarchar(max) NOT NULL,
    [ElementType] nvarchar(50) NOT NULL,
    [DefaultAssets] nvarchar(max) NOT NULL,
    [RowVersion] varbinary(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Tool] PRIMARY KEY ([Id])
);

CREATE TABLE [CameraCapabilities] (
    [Id] uniqueidentifier NOT NULL,
    [Type] int NOT NULL,
    [IsEnabled] bit NOT NULL,
    [Configuration] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CameraId] uniqueidentifier NOT NULL,
    [CameraId1] uniqueidentifier NULL,
    [RowVersion] varbinary(max) NOT NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [UpdatedBy] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_CameraCapabilities] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CameraCapabilities_Cameras_CameraId] FOREIGN KEY ([CameraId]) REFERENCES [Cameras] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CameraCapabilities_Cameras_CameraId1] FOREIGN KEY ([CameraId1]) REFERENCES [Cameras] ([Id])
);

CREATE TABLE [CameraConfigurations] (
    [Id] uniqueidentifier NOT NULL,
    [Resolution] nvarchar(20) NOT NULL,
    [FrameRate] int NOT NULL,
    [VideoCodec] nvarchar(50) NOT NULL,
    [Bitrate] int NOT NULL,
    [AudioEnabled] bit NOT NULL,
    [AudioCodec] nvarchar(50) NULL,
    [Brand] nvarchar(100) NULL,
    [AdditionalSettings] nvarchar(max) NULL,
    [MotionDetection_IsEnabled] bit NOT NULL,
    [MotionDetection_Sensitivity] int NOT NULL,
    [MotionDetection_Zone] nvarchar(1000) NULL,
    [Recording_IsEnabled] bit NOT NULL,
    [Recording_Quality] int NOT NULL,
    [Recording_Duration] float NOT NULL,
    [Recording_StoragePath] nvarchar(500) NULL,
    [CameraId] uniqueidentifier NOT NULL,
    [CameraId1] uniqueidentifier NULL,
    [RowVersion] varbinary(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_CameraConfigurations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CameraConfigurations_Cameras_CameraId] FOREIGN KEY ([CameraId]) REFERENCES [Cameras] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CameraConfigurations_Cameras_CameraId1] FOREIGN KEY ([CameraId1]) REFERENCES [Cameras] ([Id])
);

CREATE TABLE [CameraStreams] (
    [Id] uniqueidentifier NOT NULL,
    [Quality] int NOT NULL,
    [Url] nvarchar(1000) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CameraId] uniqueidentifier NOT NULL,
    [CameraId1] uniqueidentifier NULL,
    [RowVersion] varbinary(max) NOT NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [UpdatedBy] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_CameraStreams] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CameraStreams_Cameras_CameraId] FOREIGN KEY ([CameraId]) REFERENCES [Cameras] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CameraStreams_Cameras_CameraId1] FOREIGN KEY ([CameraId1]) REFERENCES [Cameras] ([Id])
);

CREATE TABLE [BaseElement] (
    [Id] uniqueidentifier NOT NULL,
    [ToolId] uniqueidentifier NOT NULL,
    [Order] int NOT NULL,
    [TemplateBody_HtmlTemplate] nvarchar(max) NOT NULL,
    [TemplateBody_DefaultCssClasses] TEXT NOT NULL,
    [TemplateBody_CustomCss] nvarchar(max) NOT NULL,
    [TemplateBody_CustomJs] nvarchar(max) NOT NULL,
    [TemplateBody_IsFloating] bit NOT NULL,
    [Asset_Url] nvarchar(max) NOT NULL,
    [Asset_Type] nvarchar(max) NOT NULL,
    [Asset_AltText] nvarchar(max) NOT NULL,
    [Asset_Content] nvarchar(max) NOT NULL,
    [Asset_Metadata] TEXT NOT NULL,
    [PageId] uniqueidentifier NOT NULL,
    [RowVersion] varbinary(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_BaseElement] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BaseElement_Pages_PageId] FOREIGN KEY ([PageId]) REFERENCES [Pages] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Template] (
    [Id] int NOT NULL IDENTITY,
    [HtmlStructure] nvarchar(max) NOT NULL,
    [DefaultCssClasses] nvarchar(max) NOT NULL,
    [DefaultCss] nvarchar(max) NOT NULL,
    [ToolId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_Template] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Template_Tool_ToolId] FOREIGN KEY ([ToolId]) REFERENCES [Tool] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_BaseElement_PageId] ON [BaseElement] ([PageId]);

CREATE INDEX [IX_CameraCapabilities_CameraId] ON [CameraCapabilities] ([CameraId]);

CREATE INDEX [IX_CameraCapabilities_CameraId1] ON [CameraCapabilities] ([CameraId1]);

CREATE INDEX [IX_CameraCapabilities_Type] ON [CameraCapabilities] ([Type]);

CREATE UNIQUE INDEX [IX_CameraConfigurations_CameraId] ON [CameraConfigurations] ([CameraId]);

CREATE UNIQUE INDEX [IX_CameraConfigurations_CameraId1] ON [CameraConfigurations] ([CameraId1]) WHERE [CameraId1] IS NOT NULL;

CREATE INDEX [IX_CameraConfigurations_Resolution] ON [CameraConfigurations] ([Resolution]);

CREATE UNIQUE INDEX [IX_Cameras_Name] ON [Cameras] ([Name]);

CREATE INDEX [IX_Cameras_Status] ON [Cameras] ([Status]);

CREATE INDEX [IX_Cameras_Type] ON [Cameras] ([Type]);

CREATE INDEX [IX_CameraStreams_CameraId] ON [CameraStreams] ([CameraId]);

CREATE INDEX [IX_CameraStreams_CameraId1] ON [CameraStreams] ([CameraId1]);

CREATE INDEX [IX_CameraStreams_Quality] ON [CameraStreams] ([Quality]);

CREATE INDEX [IX_Template_ToolId] ON [Template] ([ToolId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250818193031_init', N'9.0.8');

ALTER TABLE [Template] DROP CONSTRAINT [FK_Template_Tool_ToolId];

ALTER TABLE [Tool] DROP CONSTRAINT [PK_Tool];

EXEC sp_rename N'[Tool]', N'Tools', 'OBJECT';

ALTER TABLE [Tools] ADD CONSTRAINT [PK_Tools] PRIMARY KEY ([Id]);

ALTER TABLE [Template] ADD CONSTRAINT [FK_Template_Tools_ToolId] FOREIGN KEY ([ToolId]) REFERENCES [Tools] ([Id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250818194241_CreatePageAndToolTables', N'9.0.8');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250818194741_init1', N'9.0.8');

COMMIT;
GO

