USE [Raccoonogram_db]
GO

INSERT INTO [dbo].[AspNetUsers]
           ([Id]
           ,[Status]
           ,[Additionally]
           ,[Site]
           ,[Email]
           ,[EmailConfirmed]
           ,[PasswordHash]
           ,[SecurityStamp]
           ,[PhoneNumber]
           ,[PhoneNumberConfirmed]
           ,[TwoFactorEnabled]
           ,[LockoutEndDateUtc]
           ,[LockoutEnabled]
           ,[AccessFailedCount]
           ,[UserName])
     VALUES
           (<Id, nvarchar(128),>
           ,<Status, nvarchar(max),>
           ,<Additionally, nvarchar(max),>
           ,<Site, nvarchar(max),>
           ,<Email, nvarchar(256),>
           ,<EmailConfirmed, bit,>
           ,<PasswordHash, nvarchar(max),>
           ,<SecurityStamp, nvarchar(max),>
           ,<PhoneNumber, nvarchar(max),>
           ,<PhoneNumberConfirmed, bit,>
           ,<TwoFactorEnabled, bit,>
           ,<LockoutEndDateUtc, datetime,>
           ,<LockoutEnabled, bit,>
           ,<AccessFailedCount, int,>
           ,<UserName, nvarchar(256),>)
GO

