-- InitialCreate migration SQL for Invoices table
CREATE TABLE [dbo].[Invoices](
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Customer] NVARCHAR(200) NOT NULL,
    [Amount] DECIMAL(18,2) NOT NULL,
    [CreatedOn] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
