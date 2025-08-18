/*
=====================================================================================
DatabaseHelper.sql
Database Management and Utility Procedures for FractalDataWorks Sample Application
=====================================================================================

Purpose:
Provides utility stored procedures and functions for database management,
data reset operations, health checks, and maintenance tasks.

Procedures Included:
- sp_ResetSampleData: Clears and reinserts all sample data
- sp_DatabaseHealthCheck: Comprehensive database health assessment
- sp_GetTableStats: Detailed table statistics and information
- sp_CleanupOldActivity: Archive/cleanup old user activity logs
- fn_GetCustomerOrderSummary: Customer order statistics function

Usage:
Execute this script after all other setup scripts to install utility procedures.

Notes:
- All procedures include proper error handling
- Safe to run multiple times
- Includes performance monitoring capabilities
- Supports maintenance and troubleshooting
=====================================================================================
*/

USE [SampleDb];
GO

-- =============================================================================
-- UTILITY STORED PROCEDURES
-- =============================================================================

-- Drop existing procedures if they exist (for safe re-deployment)
IF OBJECT_ID('sp_ResetSampleData', 'P') IS NOT NULL
    DROP PROCEDURE sp_ResetSampleData;
GO

IF OBJECT_ID('sp_DatabaseHealthCheck', 'P') IS NOT NULL
    DROP PROCEDURE sp_DatabaseHealthCheck;
GO

IF OBJECT_ID('sp_GetTableStats', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetTableStats;
GO

IF OBJECT_ID('sp_CleanupOldActivity', 'P') IS NOT NULL
    DROP PROCEDURE sp_CleanupOldActivity;
GO

IF OBJECT_ID('fn_GetCustomerOrderSummary', 'FN') IS NOT NULL
    DROP FUNCTION fn_GetCustomerOrderSummary;
GO

-- =============================================================================
-- PROCEDURE: sp_ResetSampleData
-- Clears all data and reinserts sample data for testing purposes
-- =============================================================================

CREATE PROCEDURE sp_ResetSampleData
    @ConfirmReset BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Safety check to prevent accidental data loss
        IF @ConfirmReset = 0
        BEGIN
            PRINT 'WARNING: This procedure will delete ALL data in the sample database.';
            PRINT 'To proceed, call with @ConfirmReset = 1';
            PRINT 'Example: EXEC sp_ResetSampleData @ConfirmReset = 1';
            RETURN;
        END
        
        PRINT 'Starting sample data reset...';
        
        BEGIN TRANSACTION;
        
        -- Disable foreign key constraints temporarily
        PRINT 'Disabling foreign key constraints...';
        EXEC sp_msforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all";
        
        -- Clear all data in reverse dependency order
        PRINT 'Clearing existing data...';
        DELETE FROM [users].[UserActivity];
        DELETE FROM [sales].[Orders];
        DELETE FROM [sales].[Customers];
        DELETE FROM [inventory].[Products];
        DELETE FROM [inventory].[Categories];
        
        -- Reset identity seeds
        PRINT 'Resetting identity seeds...';
        DBCC CHECKIDENT('[users].[UserActivity]', RESEED, 0);
        DBCC CHECKIDENT('[sales].[Orders]', RESEED, 0);
        DBCC CHECKIDENT('[sales].[Customers]', RESEED, 0);
        DBCC CHECKIDENT('[inventory].[Products]', RESEED, 0);
        DBCC CHECKIDENT('[inventory].[Categories]', RESEED, 0);
        
        -- Re-enable foreign key constraints
        PRINT 'Re-enabling foreign key constraints...';
        EXEC sp_msforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all";
        
        COMMIT TRANSACTION;
        
        PRINT 'Data cleared successfully. Run the sample data scripts to repopulate.';
        PRINT 'Recommended order:';
        PRINT '1. 02_CreateSchemas.sql (if needed)';
        PRINT '2. 03_CreateTables.sql (if needed)';
        PRINT '3. 04_InsertSampleData.sql';
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        PRINT 'ERROR: Failed to reset sample data.';
        PRINT 'Error Message: ' + ERROR_MESSAGE();
        PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR(10));
        PRINT 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(10));
        
        THROW;
    END CATCH
END;
GO

-- =============================================================================
-- PROCEDURE: sp_DatabaseHealthCheck
-- Performs comprehensive database health assessment
-- =============================================================================

CREATE PROCEDURE sp_DatabaseHealthCheck
AS
BEGIN
    SET NOCOUNT ON;
    
    PRINT 'FractalDataWorks Sample Database Health Check';
    PRINT '=============================================';
    PRINT 'Timestamp: ' + CONVERT(VARCHAR(20), GETUTCDATE(), 120) + ' UTC';
    PRINT '';
    
    -- Check database basic information
    PRINT '1. Database Information:';
    SELECT 
        DB_NAME() AS DatabaseName,
        DATABASEPROPERTYEX(DB_NAME(), 'Status') AS DatabaseStatus,
        DATABASEPROPERTYEX(DB_NAME(), 'Collation') AS Collation,
        DATABASEPROPERTYEX(DB_NAME(), 'Recovery') AS RecoveryModel,
        DATABASEPROPERTYEX(DB_NAME(), 'Version') AS Version;
    
    PRINT '';
    PRINT '2. Schema Verification:';
    SELECT 
        name AS SchemaName,
        CASE 
            WHEN name IN ('sales', 'inventory', 'users', 'audit') THEN 'Custom Schema'
            ELSE 'System Schema'
        END AS SchemaType
    FROM sys.schemas 
    WHERE name IN ('sales', 'inventory', 'users', 'audit', 'dbo')
    ORDER BY name;
    
    PRINT '';
    PRINT '3. Table Status and Row Counts:';
    SELECT 
        s.name AS SchemaName,
        t.name AS TableName,
        p.rows AS RowCount,
        CASE 
            WHEN p.rows = 0 THEN 'Empty'
            WHEN p.rows < 10 THEN 'Low Data'
            WHEN p.rows < 100 THEN 'Moderate Data'
            ELSE 'High Data'
        END AS DataStatus
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    INNER JOIN sys.partitions p ON t.object_id = p.object_id
    WHERE p.index_id IN (0, 1) -- Heap or clustered index
        AND s.name IN ('sales', 'inventory', 'users')
    ORDER BY s.name, t.name;
    
    PRINT '';
    PRINT '4. Index Health:';
    SELECT 
        s.name AS SchemaName,
        t.name AS TableName,
        COUNT(i.index_id) AS IndexCount,
        SUM(CASE WHEN i.type = 1 THEN 1 ELSE 0 END) AS ClusteredIndexes,
        SUM(CASE WHEN i.type = 2 THEN 1 ELSE 0 END) AS NonClusteredIndexes
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    LEFT JOIN sys.indexes i ON t.object_id = i.object_id AND i.index_id > 0
    WHERE s.name IN ('sales', 'inventory', 'users')
    GROUP BY s.name, t.name
    ORDER BY s.name, t.name;
    
    PRINT '';
    PRINT '5. Foreign Key Relationships:';
    SELECT 
        s1.name AS ParentSchema,
        t1.name AS ParentTable,
        s2.name AS ChildSchema,
        t2.name AS ChildTable,
        fk.name AS ForeignKeyName,
        CASE WHEN fk.is_disabled = 1 THEN 'Disabled' ELSE 'Enabled' END AS Status
    FROM sys.foreign_keys fk
    INNER JOIN sys.tables t1 ON fk.referenced_object_id = t1.object_id
    INNER JOIN sys.schemas s1 ON t1.schema_id = s1.schema_id
    INNER JOIN sys.tables t2 ON fk.parent_object_id = t2.object_id
    INNER JOIN sys.schemas s2 ON t2.schema_id = s2.schema_id
    WHERE s1.name IN ('sales', 'inventory', 'users') 
       OR s2.name IN ('sales', 'inventory', 'users')
    ORDER BY s1.name, t1.name, s2.name, t2.name;
    
    PRINT '';
    PRINT '6. Data Integrity Checks:';
    
    -- Check for orphaned records
    DECLARE @orphanedOrders INT;
    SELECT @orphanedOrders = COUNT(*)
    FROM [sales].[Orders] o
    LEFT JOIN [sales].[Customers] c ON o.CustomerId = c.Id
    WHERE c.Id IS NULL;
    
    DECLARE @orphanedProducts INT;
    SELECT @orphanedProducts = COUNT(*)
    FROM [inventory].[Products] p
    LEFT JOIN [inventory].[Categories] c ON p.CategoryId = c.Id
    WHERE c.Id IS NULL;
    
    SELECT 
        'Orphaned Orders' AS CheckName,
        @orphanedOrders AS Count,
        CASE WHEN @orphanedOrders = 0 THEN 'PASS' ELSE 'FAIL' END AS Status
    UNION ALL
    SELECT 
        'Orphaned Products',
        @orphanedProducts,
        CASE WHEN @orphanedProducts = 0 THEN 'PASS' ELSE 'FAIL' END;
    
    PRINT '';
    PRINT '7. Recent Activity Summary:';
    SELECT 
        'Customers' AS EntityType,
        COUNT(*) AS TotalCount,
        COUNT(CASE WHEN IsActive = 1 THEN 1 END) AS ActiveCount,
        MAX(CreatedDate) AS LastCreated
    FROM [sales].[Customers]
    UNION ALL
    SELECT 
        'Orders',
        COUNT(*),
        COUNT(CASE WHEN Status NOT IN ('Cancelled') THEN 1 END),
        MAX(OrderDate)
    FROM [sales].[Orders]
    UNION ALL
    SELECT 
        'Products',
        COUNT(*),
        COUNT(CASE WHEN InStock = 1 THEN 1 END),
        MAX(CreatedDate)
    FROM [inventory].[Products]
    UNION ALL
    SELECT 
        'User Activities',
        COUNT(*),
        COUNT(CASE WHEN IsSuccessful = 1 THEN 1 END),
        MAX(Timestamp)
    FROM [users].[UserActivity];
    
    PRINT '';
    PRINT 'Health check completed successfully.';
    PRINT 'Review any FAIL statuses and address data integrity issues if found.';
END;
GO

-- =============================================================================
-- PROCEDURE: sp_GetTableStats
-- Provides detailed statistics for all tables
-- =============================================================================

CREATE PROCEDURE sp_GetTableStats
    @SchemaName NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        s.name AS SchemaName,
        t.name AS TableName,
        p.rows AS RowCount,
        p.data_compression_desc AS CompressionType,
        au.total_pages * 8 AS TotalSpaceKB,
        au.used_pages * 8 AS UsedSpaceKB,
        (au.total_pages - au.used_pages) * 8 AS UnusedSpaceKB,
        CASE 
            WHEN p.rows = 0 THEN 0
            ELSE CAST((au.used_pages * 8.0) / p.rows AS DECIMAL(10,2))
        END AS AvgBytesPerRow,
        t.create_date AS CreatedDate,
        t.modify_date AS LastModified
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    INNER JOIN sys.partitions p ON t.object_id = p.object_id AND p.index_id IN (0, 1)
    INNER JOIN (
        SELECT 
            object_id,
            SUM(total_pages) as total_pages,
            SUM(used_pages) as used_pages
        FROM sys.allocation_units au
        INNER JOIN sys.partitions p ON au.container_id = p.partition_id
        GROUP BY object_id
    ) au ON t.object_id = au.object_id
    WHERE (@SchemaName IS NULL OR s.name = @SchemaName)
        AND s.name IN ('sales', 'inventory', 'users', 'audit')
    ORDER BY s.name, t.name;
END;
GO

-- =============================================================================
-- PROCEDURE: sp_CleanupOldActivity
-- Archives or deletes old user activity records
-- =============================================================================

CREATE PROCEDURE sp_CleanupOldActivity
    @DaysToKeep INT = 90,
    @DeleteOldRecords BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        DECLARE @CutoffDate DATETIME2(7) = DATEADD(day, -@DaysToKeep, GETUTCDATE());
        DECLARE @RecordsToProcess INT;
        
        -- Count records that would be affected
        SELECT @RecordsToProcess = COUNT(*)
        FROM [users].[UserActivity]
        WHERE [Timestamp] < @CutoffDate;
        
        PRINT 'User Activity Cleanup Process';
        PRINT '============================';
        PRINT 'Cutoff Date: ' + CONVERT(VARCHAR(20), @CutoffDate, 120) + ' UTC';
        PRINT 'Days to Keep: ' + CAST(@DaysToKeep AS VARCHAR(10));
        PRINT 'Records older than cutoff: ' + CAST(@RecordsToProcess AS VARCHAR(10));
        
        IF @RecordsToProcess = 0
        BEGIN
            PRINT 'No old records found. Cleanup not needed.';
            RETURN;
        END
        
        IF @DeleteOldRecords = 0
        BEGIN
            PRINT '';
            PRINT 'WARNING: This is a preview run. No records will be deleted.';
            PRINT 'To actually delete old records, call with @DeleteOldRecords = 1';
            PRINT '';
            
            -- Show sample of records that would be deleted
            SELECT TOP 10
                Id,
                UserId,
                ActivityType,
                Timestamp,
                IsSuccessful
            FROM [users].[UserActivity]
            WHERE [Timestamp] < @CutoffDate
            ORDER BY [Timestamp];
            
            RETURN;
        END
        
        -- Perform the actual cleanup
        BEGIN TRANSACTION;
        
        DELETE FROM [users].[UserActivity]
        WHERE [Timestamp] < @CutoffDate;
        
        DECLARE @DeletedCount INT = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        PRINT '';
        PRINT 'Cleanup completed successfully.';
        PRINT 'Records deleted: ' + CAST(@DeletedCount AS VARCHAR(10));
        
        -- Show current activity summary
        SELECT 
            COUNT(*) AS RemainingRecords,
            MIN([Timestamp]) AS OldestRecord,
            MAX([Timestamp]) AS NewestRecord
        FROM [users].[UserActivity];
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        PRINT 'ERROR: Failed to cleanup old activity records.';
        PRINT 'Error Message: ' + ERROR_MESSAGE();
        
        THROW;
    END CATCH
END;
GO

-- =============================================================================
-- FUNCTION: fn_GetCustomerOrderSummary
-- Returns order summary statistics for a customer
-- =============================================================================

CREATE FUNCTION fn_GetCustomerOrderSummary(@CustomerId INT)
RETURNS TABLE
AS
RETURN (
    SELECT 
        c.Id AS CustomerId,
        c.Name AS CustomerName,
        c.Email AS CustomerEmail,
        c.IsActive AS IsActiveCustomer,
        COUNT(o.Id) AS TotalOrders,
        COALESCE(SUM(o.TotalAmount), 0) AS TotalSpent,
        COALESCE(AVG(o.TotalAmount), 0) AS AverageOrderValue,
        MIN(o.OrderDate) AS FirstOrderDate,
        MAX(o.OrderDate) AS LastOrderDate,
        COUNT(CASE WHEN o.Status = 'Pending' THEN 1 END) AS PendingOrders,
        COUNT(CASE WHEN o.Status = 'Processing' THEN 1 END) AS ProcessingOrders,
        COUNT(CASE WHEN o.Status = 'Shipped' THEN 1 END) AS ShippedOrders,
        COUNT(CASE WHEN o.Status = 'Delivered' THEN 1 END) AS DeliveredOrders,
        COUNT(CASE WHEN o.Status = 'Cancelled' THEN 1 END) AS CancelledOrders
    FROM [sales].[Customers] c
    LEFT JOIN [sales].[Orders] o ON c.Id = o.CustomerId
    WHERE c.Id = @CustomerId
    GROUP BY c.Id, c.Name, c.Email, c.IsActive
);
GO

-- =============================================================================
-- INSTALLATION VERIFICATION
-- =============================================================================

PRINT 'Database Helper Procedures Installation Complete';
PRINT '===============================================';
PRINT '';
PRINT 'Installed Procedures and Functions:';
PRINT '- sp_ResetSampleData: Clear and reset all sample data';
PRINT '- sp_DatabaseHealthCheck: Comprehensive database health assessment';  
PRINT '- sp_GetTableStats: Detailed table statistics';
PRINT '- sp_CleanupOldActivity: Archive/cleanup old user activity logs';
PRINT '- fn_GetCustomerOrderSummary: Customer order statistics function';
PRINT '';
PRINT 'Usage Examples:';
PRINT '- EXEC sp_DatabaseHealthCheck;';
PRINT '- EXEC sp_GetTableStats;';
PRINT '- EXEC sp_GetTableStats @SchemaName = ''sales'';';
PRINT '- EXEC sp_CleanupOldActivity @DaysToKeep = 30;';
PRINT '- SELECT * FROM fn_GetCustomerOrderSummary(1);';
PRINT '- EXEC sp_ResetSampleData @ConfirmReset = 1;';
PRINT '';
PRINT 'All utility procedures are ready for use.';
GO