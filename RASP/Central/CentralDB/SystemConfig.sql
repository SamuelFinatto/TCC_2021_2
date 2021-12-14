CREATE TABLE [dbo].[SystemConfig]
(
	[Id] INT IDENTITY(1, 1) PRIMARY KEY, 
    [HostName] VARCHAR(50) NULL, 
    [IP] NCHAR(10) NULL, 
    [SSID] VARCHAR(50) NULL, 
    [Password] VARCHAR(50) NULL,

)
