DECLARE @DropConstraints NVARCHAR(MAX) = '';
SELECT @DropConstraints += 'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(schema_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + ' DROP CONSTRAINT ' + QUOTENAME(name) + ';'
FROM sys.foreign_keys;
EXEC sp_executesql @DropConstraints;
GO

-- 2. Xóa toàn bộ các Bảng (Tables)
DECLARE @DropTables NVARCHAR(MAX) = '';
SELECT @DropTables += 'DROP TABLE ' + QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) + ';'
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE';
EXEC sp_executesql @DropTables;
GO