CREATE TABLE [dbo].[Modules]
(
	[Id] INT IDENTITY(1, 1) PRIMARY KEY, 
    [MacAddress] VARCHAR(17) NOT NULL, 
    [IpAddress] NCHAR(10) NOT NULL, 
    [Type] SMALLINT NULL, 
    [Status] BIT NULL, 
    [Name] VARCHAR(50) NULL
)
