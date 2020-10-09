CREATE TABLE [dbo].[Images] (
    [ImageId]           INT            IDENTITY (1, 1) NOT NULL,
    [ApplicationUserId] NVARCHAR (128) NULL,
    [Category]          NVARCHAR (MAX) NULL,
    [KeyWords]          NVARCHAR (MAX) NULL,
    [Description]       NVARCHAR (MAX) NULL,
    [Url]               NVARCHAR (MAX) NULL,
	[Price]             FLOAT (53)     NOT NULL,
    [Date]              DATETIME       NOT NULL,
    CONSTRAINT [PK_dbo.Images] PRIMARY KEY CLUSTERED ([ImageId] ASC),
    CONSTRAINT [FK_dbo.Images_dbo.AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_ApplicationUserId]
    ON [dbo].[Images]([ApplicationUserId] ASC);

