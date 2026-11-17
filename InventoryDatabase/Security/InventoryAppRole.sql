-- OWASP A01/A05: apply least-privilege access instead of granting db_owner to the application.
-- Creates a dedicated application role scoped to CRUD operations on Products only.
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'InventoryAppRole' AND type = 'R')
BEGIN
    CREATE ROLE [InventoryAppRole];
END
GO

GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Products] TO [InventoryAppRole];
GO
