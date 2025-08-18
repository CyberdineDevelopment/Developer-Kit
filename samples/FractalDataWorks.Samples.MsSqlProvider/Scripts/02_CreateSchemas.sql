/*
=====================================================================================
02_CreateSchemas.sql
Schema Creation Script for FractalDataWorks Sample Application
=====================================================================================

Purpose:
Creates logical schemas to organize database objects by functional area.
This script is safe to run multiple times.

Schemas Created:
- sales: Customer and order related tables
- inventory: Product and category management
- users: User activity and logging
- audit: Audit trail and change tracking

Usage:
Execute this script after 01_CreateDatabase.sql and before 03_CreateTables.sql

Notes:
- Each schema represents a bounded context in the domain
- Provides logical separation for security and organization
- Enables fine-grained permission control
=====================================================================================
*/

USE [SampleDb];
GO

-- Create sales schema for customer and order management
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'sales')
BEGIN
    PRINT 'Creating sales schema...';
    EXEC('CREATE SCHEMA [sales] AUTHORIZATION [dbo]');
    PRINT 'Sales schema created successfully.';
END
ELSE
BEGIN
    PRINT 'Sales schema already exists. Skipping creation.';
END
GO

-- Create inventory schema for product and category management
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'inventory')
BEGIN
    PRINT 'Creating inventory schema...';
    EXEC('CREATE SCHEMA [inventory] AUTHORIZATION [dbo]');
    PRINT 'Inventory schema created successfully.';
END
ELSE
BEGIN
    PRINT 'Inventory schema already exists. Skipping creation.';
END
GO

-- Create users schema for user activity and session management
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'users')
BEGIN
    PRINT 'Creating users schema...';
    EXEC('CREATE SCHEMA [users] AUTHORIZATION [dbo]');
    PRINT 'Users schema created successfully.';
END
ELSE
BEGIN
    PRINT 'Users schema already exists. Skipping creation.';
END
GO

-- Create audit schema for change tracking and audit trails
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'audit')
BEGIN
    PRINT 'Creating audit schema...';
    EXEC('CREATE SCHEMA [audit] AUTHORIZATION [dbo]');
    PRINT 'Audit schema created successfully.';
END
ELSE
BEGIN
    PRINT 'Audit schema already exists. Skipping creation.';
END
GO

-- Verify schema creation
PRINT 'Verifying schema creation...';

DECLARE @schemaCount INT;
SELECT @schemaCount = COUNT(*)
FROM sys.schemas 
WHERE name IN ('sales', 'inventory', 'users', 'audit');

IF @schemaCount = 4
BEGIN
    PRINT 'All schemas created successfully.';
    PRINT 'Schema creation completed. Ready for table creation.';
END
ELSE
BEGIN
    PRINT 'ERROR: Not all schemas were created successfully.';
    PRINT 'Expected 4 schemas, found: ' + CAST(@schemaCount AS VARCHAR(10));
    RAISERROR('Schema creation incomplete', 16, 1);
END
GO

-- Display created schemas for verification
PRINT 'Created schemas:';
SELECT 
    s.name AS SchemaName,
    p.name AS SchemaOwner,
    s.schema_id AS SchemaId
FROM sys.schemas s
INNER JOIN sys.database_principals p ON s.principal_id = p.principal_id
WHERE s.name IN ('sales', 'inventory', 'users', 'audit')
ORDER BY s.name;
GO