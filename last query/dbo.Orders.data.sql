SET IDENTITY_INSERT [dbo].[Orders] ON
INSERT INTO [dbo].[Orders] ([OrderId], [ImageId], [BuyerEmail], [ApplicationUserId], [Price], [Size], [BuyingDate]) VALUES (1, 69, N'my@mail.ru', N'5b5b05cb-df2b-4b5d-881e-fd8d05cac359', 3.38, 1920, N'2018-05-17 11:40:34')
INSERT INTO [dbo].[Orders] ([OrderId], [ImageId], [BuyerEmail], [ApplicationUserId], [Price], [Size], [BuyingDate]) VALUES (2, 18, N'my@mail.ru', N'cbdbd8dc-eeab-43f3-89a9-123a8c525c5b', 3, 1920, N'2018-05-17 11:40:55')
INSERT INTO [dbo].[Orders] ([OrderId], [ImageId], [BuyerEmail], [ApplicationUserId], [Price], [Size], [BuyingDate]) VALUES (3, 70, N'my@mail.ru', N'5b5b05cb-df2b-4b5d-881e-fd8d05cac359', 3.38, 1920, N'2018-05-17 11:41:06')
SET IDENTITY_INSERT [dbo].[Orders] OFF
