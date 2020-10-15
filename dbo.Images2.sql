CREATE TABLE [dbo].[Images] (
    [ImageId]  INT            IDENTITY (1, 1) NOT NULL,
    [ApplicationUserId]   NVARCHAR (128) NOT NULL,
    [Category] NVARCHAR (MAX) NULL,
    [KeyWords] NVARCHAR (MAX) NULL,
    [Url]      NVARCHAR (MAX) NULL,
    [Price]    FLOAT (53)     NOT NULL,
    [Date]     DATETIME       NOT NULL,
    CONSTRAINT [FK_Images_ApplicationUser] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers]([Id]) 
	ON DELETE SET NULL
);

