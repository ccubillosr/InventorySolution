CREATE TABLE [dbo].[Products]
(
    [Id]            INT IDENTITY(1,1) NOT NULL,
    [Name]          NVARCHAR(120)   NOT NULL,
    [Description]   NVARCHAR(500)   NULL,
    [Sku]           NVARCHAR(60)    NOT NULL,
    [Quantity]      INT             NOT NULL CONSTRAINT [DF_Products_Quantity] DEFAULT (0),
    [UnitPrice]     DECIMAL(18,2)   NOT NULL CONSTRAINT [DF_Products_UnitPrice] DEFAULT (0),
    [RowVersion]    ROWVERSION      NOT NULL,
    [CreatedAtUtc]  DATETIME2(3)    NOT NULL CONSTRAINT [DF_Products_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAtUtc]  DATETIME2(3)    NULL,

    CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_Products_Sku] UNIQUE ([Sku]),
    CONSTRAINT [CK_Products_Quantity] CHECK ([Quantity] >= 0),
    CONSTRAINT [CK_Products_UnitPrice] CHECK ([UnitPrice] >= 0)
);
