/*
=====================================================================================
03_CreateTables.sql
Table Creation Script for FractalDataWorks Sample Application
=====================================================================================

Purpose:
Creates all tables, constraints, indexes, and relationships for the sample application.
This script is safe to run multiple times.

Tables Created:
- sales.Customers: Customer master data with versioning
- sales.Orders: Order transactions linked to customers
- inventory.Categories: Product categorization hierarchy
- inventory.Products: Product catalog with pricing and stock
- users.UserActivity: User activity logging and analytics

Usage:
Execute this script after 02_CreateSchemas.sql and before 04_InsertSampleData.sql

Design Notes:
- All tables include optimistic concurrency control via Version columns
- Proper indexing for common query patterns
- Foreign key relationships with cascading rules
- Check constraints for data integrity
- Audit-friendly design with timestamps
=====================================================================================
*/

USE [SampleDb];
GO

-- =============================================================================
-- SALES SCHEMA TABLES
-- =============================================================================

-- Create Customers table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[sales].[Customers]') AND type in (N'U'))
BEGIN
    PRINT 'Creating sales.Customers table...';
    
    CREATE TABLE [sales].[Customers] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(255) NOT NULL,
        [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreditLimit] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        [Version] ROWVERSION NOT NULL,
        
        CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UK_Customers_Email] UNIQUE NONCLUSTERED ([Email] ASC),
        CONSTRAINT [CK_Customers_CreditLimit] CHECK ([CreditLimit] >= 0),
        CONSTRAINT [CK_Customers_Email_Format] CHECK ([Email] LIKE '%@%.%')
    );
    
    -- Index for active customers
    CREATE NONCLUSTERED INDEX [IX_Customers_IsActive_Name] 
    ON [sales].[Customers] ([IsActive] ASC, [Name] ASC);
    
    -- Index for created date queries
    CREATE NONCLUSTERED INDEX [IX_Customers_CreatedDate] 
    ON [sales].[Customers] ([CreatedDate] ASC);
    
    PRINT 'sales.Customers table created successfully.';
END
ELSE
BEGIN
    PRINT 'sales.Customers table already exists. Skipping creation.';
END
GO

-- Create Orders table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[sales].[Orders]') AND type in (N'U'))
BEGIN
    PRINT 'Creating sales.Orders table...';
    
    CREATE TABLE [sales].[Orders] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [CustomerId] INT NOT NULL,
        [OrderDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [Status] NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        [TotalAmount] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        [Version] ROWVERSION NOT NULL,
        
        CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Orders_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [sales].[Customers] ([Id]),
        CONSTRAINT [CK_Orders_TotalAmount] CHECK ([TotalAmount] >= 0),
        CONSTRAINT [CK_Orders_Status] CHECK ([Status] IN ('Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled'))
    );
    
    -- Index for customer orders
    CREATE NONCLUSTERED INDEX [IX_Orders_CustomerId_OrderDate] 
    ON [sales].[Orders] ([CustomerId] ASC, [OrderDate] DESC);
    
    -- Index for order status and date
    CREATE NONCLUSTERED INDEX [IX_Orders_Status_OrderDate] 
    ON [sales].[Orders] ([Status] ASC, [OrderDate] DESC);
    
    -- Index for date range queries
    CREATE NONCLUSTERED INDEX [IX_Orders_OrderDate] 
    ON [sales].[Orders] ([OrderDate] ASC);
    
    PRINT 'sales.Orders table created successfully.';
END
ELSE
BEGIN
    PRINT 'sales.Orders table already exists. Skipping creation.';
END
GO

-- =============================================================================
-- INVENTORY SCHEMA TABLES
-- =============================================================================

-- Create Categories table (self-referencing for hierarchy)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[inventory].[Categories]') AND type in (N'U'))
BEGIN
    PRINT 'Creating inventory.Categories table...';
    
    CREATE TABLE [inventory].[Categories] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(50) NOT NULL,
        [ParentId] INT NULL,
        [Description] NVARCHAR(255) NULL,
        [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [IsActive] BIT NOT NULL DEFAULT 1,
        
        CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Categories_Parent] FOREIGN KEY ([ParentId]) REFERENCES [inventory].[Categories] ([Id]),
        CONSTRAINT [UK_Categories_Name_Parent] UNIQUE NONCLUSTERED ([Name] ASC, [ParentId] ASC)
    );
    
    -- Index for hierarchy queries
    CREATE NONCLUSTERED INDEX [IX_Categories_ParentId_Name] 
    ON [inventory].[Categories] ([ParentId] ASC, [Name] ASC);
    
    -- Index for active categories
    CREATE NONCLUSTERED INDEX [IX_Categories_IsActive_Name] 
    ON [inventory].[Categories] ([IsActive] ASC, [Name] ASC);
    
    PRINT 'inventory.Categories table created successfully.';
END
ELSE
BEGIN
    PRINT 'inventory.Categories table already exists. Skipping creation.';
END
GO

-- Create Products table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[inventory].[Products]') AND type in (N'U'))
BEGIN
    PRINT 'Creating inventory.Products table...';
    
    CREATE TABLE [inventory].[Products] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Price] DECIMAL(18,2) NOT NULL,
        [CategoryId] INT NOT NULL,
        [InStock] BIT NOT NULL DEFAULT 1,
        [CreatedDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [Description] NVARCHAR(500) NULL,
        [SKU] NVARCHAR(50) NOT NULL,
        
        CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Products_Categories] FOREIGN KEY ([CategoryId]) REFERENCES [inventory].[Categories] ([Id]),
        CONSTRAINT [UK_Products_SKU] UNIQUE NONCLUSTERED ([SKU] ASC),
        CONSTRAINT [CK_Products_Price] CHECK ([Price] >= 0)
    );
    
    -- Index for category and stock queries
    CREATE NONCLUSTERED INDEX [IX_Products_CategoryId_InStock] 
    ON [inventory].[Products] ([CategoryId] ASC, [InStock] ASC);
    
    -- Index for name searches
    CREATE NONCLUSTERED INDEX [IX_Products_Name] 
    ON [inventory].[Products] ([Name] ASC);
    
    -- Index for price range queries
    CREATE NONCLUSTERED INDEX [IX_Products_Price] 
    ON [inventory].[Products] ([Price] ASC);
    
    PRINT 'inventory.Products table created successfully.';
END
ELSE
BEGIN
    PRINT 'inventory.Products table already exists. Skipping creation.';
END
GO

-- =============================================================================
-- USERS SCHEMA TABLES
-- =============================================================================

-- Create UserActivity table for logging user actions
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[users].[UserActivity]') AND type in (N'U'))
BEGIN
    PRINT 'Creating users.UserActivity table...';
    
    CREATE TABLE [users].[UserActivity] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [UserId] NVARCHAR(50) NOT NULL,
        [ActivityType] NVARCHAR(50) NOT NULL,
        [Timestamp] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [IsSuccessful] BIT NOT NULL DEFAULT 1,
        [Details] NVARCHAR(MAX) NULL,
        [SessionId] NVARCHAR(100) NULL,
        [IPAddress] NVARCHAR(45) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        
        CONSTRAINT [PK_UserActivity] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [CK_UserActivity_IPAddress] CHECK ([IPAddress] IS NULL OR LEN([IPAddress]) > 0)
    );
    
    -- Index for user activity queries
    CREATE NONCLUSTERED INDEX [IX_UserActivity_UserId_Timestamp] 
    ON [users].[UserActivity] ([UserId] ASC, [Timestamp] DESC);
    
    -- Index for activity type analysis
    CREATE NONCLUSTERED INDEX [IX_UserActivity_ActivityType_Timestamp] 
    ON [users].[UserActivity] ([ActivityType] ASC, [Timestamp] DESC);
    
    -- Index for session tracking
    CREATE NONCLUSTERED INDEX [IX_UserActivity_SessionId_Timestamp] 
    ON [users].[UserActivity] ([SessionId] ASC, [Timestamp] DESC)
    WHERE [SessionId] IS NOT NULL;
    
    -- Index for failed activities
    CREATE NONCLUSTERED INDEX [IX_UserActivity_IsSuccessful_Timestamp] 
    ON [users].[UserActivity] ([IsSuccessful] ASC, [Timestamp] DESC)
    WHERE [IsSuccessful] = 0;
    
    PRINT 'users.UserActivity table created successfully.';
END
ELSE
BEGIN
    PRINT 'users.UserActivity table already exists. Skipping creation.';
END
GO

-- =============================================================================
-- TABLE VERIFICATION AND SUMMARY
-- =============================================================================

PRINT 'Verifying table creation...';

-- Count created tables by schema
DECLARE @tableCount INT;
SELECT @tableCount = COUNT(*)
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name IN ('sales', 'inventory', 'users');

PRINT 'Total tables created: ' + CAST(@tableCount AS VARCHAR(10));

-- Display table summary
PRINT 'Table creation summary:';
SELECT 
    s.name AS SchemaName,
    t.name AS TableName,
    c.column_count AS ColumnCount,
    i.index_count AS IndexCount
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
LEFT JOIN (
    SELECT 
        object_id, 
        COUNT(*) as column_count 
    FROM sys.columns 
    GROUP BY object_id
) c ON t.object_id = c.object_id
LEFT JOIN (
    SELECT 
        object_id, 
        COUNT(*) as index_count 
    FROM sys.indexes 
    WHERE type > 0 
    GROUP BY object_id
) i ON t.object_id = i.object_id
WHERE s.name IN ('sales', 'inventory', 'users')
ORDER BY s.name, t.name;

PRINT 'Table creation completed successfully.';
PRINT 'Ready for sample data insertion.';
GO