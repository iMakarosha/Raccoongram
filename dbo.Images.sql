CREATE TABLE [dbo].[Images] (
    [ImageId]  INT            IDENTITY (1, 1) NOT NULL,
    [UserId]   INT            NOT NULL,
    [Category] NVARCHAR (MAX) NULL,
    [KeyWords] NVARCHAR (MAX) NULL,
	[Url]   NVARCHAR (MAX) NULL,
    [Price]    FLOAT (53)     NOT NULL,
    [Date]     DATETIME       NOT NULL,
    CONSTRAINT [PK_dbo.Images] PRIMARY KEY CLUSTERED ([ImageId] ASC)
);

