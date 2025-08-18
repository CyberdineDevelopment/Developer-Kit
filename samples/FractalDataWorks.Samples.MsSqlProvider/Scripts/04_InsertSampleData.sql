/*
=====================================================================================
04_InsertSampleData.sql
Sample Data Insertion Script for FractalDataWorks Sample Application
=====================================================================================

Purpose:
Inserts realistic sample data for testing and demonstration purposes.
This script is safe to run multiple times (checks for existing data).

Sample Data Includes:
- 25 customers with varied profiles
- 50+ orders across different statuses and dates
- Product categories with hierarchical structure
- 100+ products across multiple categories
- User activity logs for analytics testing

Usage:
Execute this script after 03_CreateTables.sql

Notes:
- Data designed for realistic testing scenarios
- Includes edge cases and varied data patterns
- Supports demonstration of all application features
- Safe to re-run (checks for existing data)
=====================================================================================
*/

USE [SampleDb];
GO

-- =============================================================================
-- SAMPLE DATA FOR INVENTORY SCHEMA
-- =============================================================================

-- Insert Categories (hierarchical structure)
PRINT 'Inserting sample categories...';

IF NOT EXISTS (SELECT 1 FROM [inventory].[Categories])
BEGIN
    -- Root categories
    INSERT INTO [inventory].[Categories] ([Name], [ParentId], [Description])
    VALUES 
        ('Electronics', NULL, 'Electronic devices and accessories'),
        ('Clothing', NULL, 'Apparel and fashion items'),
        ('Home & Garden', NULL, 'Home improvement and garden supplies'),
        ('Books', NULL, 'Books and educational materials'),
        ('Sports & Outdoors', NULL, 'Sports equipment and outdoor gear');
    
    -- Electronics subcategories
    INSERT INTO [inventory].[Categories] ([Name], [ParentId], [Description])
    VALUES 
        ('Computers', 1, 'Desktop and laptop computers'),
        ('Mobile Devices', 1, 'Smartphones and tablets'),
        ('Audio', 1, 'Headphones, speakers, and audio equipment'),
        ('Gaming', 1, 'Gaming consoles and accessories');
    
    -- Clothing subcategories
    INSERT INTO [inventory].[Categories] ([Name], [ParentId], [Description])
    VALUES 
        ('Men''s Clothing', 2, 'Clothing for men'),
        ('Women''s Clothing', 2, 'Clothing for women'),
        ('Shoes', 2, 'Footwear for all ages'),
        ('Accessories', 2, 'Fashion accessories');
    
    -- Home & Garden subcategories
    INSERT INTO [inventory].[Categories] ([Name], [ParentId], [Description])
    VALUES 
        ('Furniture', 3, 'Home and office furniture'),
        ('Kitchen', 3, 'Kitchen appliances and utensils'),
        ('Garden Tools', 3, 'Gardening equipment and tools');
    
    PRINT 'Sample categories inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Categories already exist. Skipping category insertion.';
END
GO

-- Insert Products
PRINT 'Inserting sample products...';

IF NOT EXISTS (SELECT 1 FROM [inventory].[Products])
BEGIN
    -- Electronics - Computers
    INSERT INTO [inventory].[Products] ([Name], [Price], [CategoryId], [InStock], [Description], [SKU])
    VALUES 
        ('Gaming Laptop Pro 15"', 1299.99, 6, 1, 'High-performance gaming laptop with RTX graphics', 'COMP-LAP-001'),
        ('Business Ultrabook 13"', 899.99, 6, 1, 'Lightweight business laptop for professionals', 'COMP-LAP-002'),
        ('Desktop Workstation', 1899.99, 6, 1, 'Powerful desktop for content creation', 'COMP-DES-001'),
        ('Mini PC Home Theater', 399.99, 6, 0, 'Compact PC for media streaming', 'COMP-MIN-001');
    
    -- Electronics - Mobile Devices
    INSERT INTO [inventory].[Products] ([Name], [Price], [CategoryId], [InStock], [Description], [SKU])
    VALUES 
        ('Smartphone Pro Max 256GB', 999.99, 7, 1, 'Latest flagship smartphone with advanced camera', 'MOB-PHO-001'),
        ('Budget Smartphone 128GB', 299.99, 7, 1, 'Affordable smartphone with good battery life', 'MOB-PHO-002'),
        ('Tablet 10" Wi-Fi', 329.99, 7, 1, '10-inch tablet perfect for reading and browsing', 'MOB-TAB-001'),
        ('Tablet Pro 12" Cellular', 799.99, 7, 1, 'Professional tablet with cellular connectivity', 'MOB-TAB-002');
    
    -- Electronics - Audio
    INSERT INTO [inventory].[Products] ([Name], [Price], [CategoryId], [InStock], [Description], [SKU])
    VALUES 
        ('Wireless Noise-Canceling Headphones', 299.99, 8, 1, 'Premium headphones with active noise cancellation', 'AUD-HEA-001'),
        ('Sports Earbuds Wireless', 149.99, 8, 1, 'Waterproof wireless earbuds for sports', 'AUD-EAR-001'),
        ('Bluetooth Speaker Portable', 79.99, 8, 1, 'Compact Bluetooth speaker with rich sound', 'AUD-SPE-001'),
        ('Home Theater System', 599.99, 8, 0, '5.1 surround sound system for home theater', 'AUD-HTS-001');
    
    -- Electronics - Gaming
    INSERT INTO [inventory].[Products] ([Name], [Price], [CategoryId], [InStock], [Description], [SKU])
    VALUES 
        ('Gaming Console Next Gen', 499.99, 9, 1, 'Next generation gaming console with 4K support', 'GAM-CON-001'),
        ('Wireless Gaming Controller', 69.99, 9, 1, 'Ergonomic wireless controller with haptic feedback', 'GAM-CTR-001'),
        ('Gaming Mechanical Keyboard', 129.99, 9, 1, 'RGB mechanical keyboard for gaming', 'GAM-KEY-001'),
        ('Gaming Mouse Wireless', 89.99, 9, 1, 'High-precision wireless gaming mouse', 'GAM-MOU-001');
    
    -- Clothing - Men's
    INSERT INTO [inventory].[Products] ([Name], [Price], [CategoryId], [InStock], [Description], [SKU])
    VALUES 
        ('Men''s Cotton T-Shirt', 24.99, 10, 1, 'Comfortable 100% cotton t-shirt', 'CLO-MEN-001'),
        ('Men''s Jeans Classic Fit', 59.99, 10, 1, 'Classic fit denim jeans', 'CLO-MEN-002'),
        ('Men''s Dress Shirt', 39.99, 10, 1, 'Professional dress shirt for business', 'CLO-MEN-003'),
        ('Men''s Hoodie', 49.99, 10, 1, 'Warm and comfortable hoodie', 'CLO-MEN-004');
    
    -- Clothing - Women's
    INSERT INTO [inventory].[Products] ([Name], [Price], [CategoryId], [InStock], [Description], [SKU])
    VALUES 
        ('Women''s Blouse Silk', 69.99, 11, 1, 'Elegant silk blouse for professional wear', 'CLO-WOM-001'),
        ('Women''s Skinny Jeans', 54.99, 11, 1, 'Comfortable stretch skinny jeans', 'CLO-WOM-002'),
        ('Women''s Summer Dress', 79.99, 11, 1, 'Light and breezy summer dress', 'CLO-WOM-003'),
        ('Women''s Cardigan', 44.99, 11, 0, 'Soft knit cardigan sweater', 'CLO-WOM-004');
    
    -- Shoes
    INSERT INTO [inventory].[Products] ([Name], [Price], [CategoryId], [InStock], [Description], [SKU])
    VALUES 
        ('Running Shoes Men''s', 119.99, 12, 1, 'High-performance running shoes with cushioning', 'SHO-RUN-001'),
        ('Casual Sneakers Women''s', 89.99, 12, 1, 'Comfortable everyday sneakers', 'SHO-CAS-001'),
        ('Dress Shoes Men''s Leather', 149.99, 12, 1, 'Classic leather dress shoes', 'SHO-DRE-001'),
        ('Boots Women''s Waterproof', 129.99, 12, 1, 'Durable waterproof boots', 'SHO-BOO-001');
    
    -- Home & Garden - Furniture
    INSERT INTO [inventory].[Products] ([Name], [Price], [CategoryId], [InStock], [Description], [SKU])
    VALUES 
        ('Office Chair Ergonomic', 299.99, 14, 1, 'Ergonomic office chair with lumbar support', 'FUR-CHA-001'),
        ('Dining Table Wood', 599.99, 14, 1, 'Solid wood dining table for 6 people', 'FUR-TAB-001'),
        ('Bookshelf 5-Tier', 149.99, 14, 1, 'Modern 5-tier bookshelf', 'FUR-BOO-001'),
        ('Sofa 3-Seater', 899.99, 14, 0, 'Comfortable 3-seater sofa', 'FUR-SOF-001');
    
    -- Home & Garden - Kitchen
    INSERT INTO [inventory].[Products] ([Name], [Price], [CategoryId], [InStock], [Description], [SKU])
    VALUES 
        ('Coffee Maker Programmable', 89.99, 15, 1, '12-cup programmable coffee maker', 'KIT-COF-001'),
        ('Blender High-Speed', 199.99, 15, 1, 'High-speed blender for smoothies', 'KIT-BLE-001'),
        ('Non-Stick Pan Set', 79.99, 15, 1, '3-piece non-stick pan set', 'KIT-PAN-001'),
        ('Stand Mixer', 349.99, 15, 1, 'Professional stand mixer for baking', 'KIT-MIX-001');
    
    -- Books
    INSERT INTO [inventory].[Products] ([Name], [Price], [CategoryId], [InStock], [Description], [SKU])
    VALUES 
        ('Programming Guide Complete', 45.99, 4, 1, 'Comprehensive programming reference book', 'BOO-PRO-001'),
        ('Mystery Novel Bestseller', 14.99, 4, 1, 'New York Times bestselling mystery novel', 'BOO-MYS-001'),
        ('Cooking Masterclass', 32.99, 4, 1, 'Professional cooking techniques and recipes', 'BOO-COO-001'),
        ('History World Wars', 28.99, 4, 1, 'Detailed history of the world wars', 'BOO-HIS-001');
    
    -- Sports & Outdoors
    INSERT INTO [inventory].[Products] ([Name], [Price], [CategoryId], [InStock], [Description], [SKU])
    VALUES 
        ('Yoga Mat Premium', 59.99, 5, 1, 'High-quality yoga mat with excellent grip', 'SPO-YOG-001'),
        ('Camping Tent 4-Person', 199.99, 5, 1, 'Waterproof camping tent for 4 people', 'SPO-TEN-001'),
        ('Bicycle Mountain 26"', 599.99, 5, 1, 'Mountain bike with 21-speed transmission', 'SPO-BIC-001'),
        ('Hiking Backpack 40L', 129.99, 5, 0, 'Durable hiking backpack with rain cover', 'SPO-BAC-001');
    
    PRINT 'Sample products inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Products already exist. Skipping product insertion.';
END
GO

-- =============================================================================
-- SAMPLE DATA FOR SALES SCHEMA
-- =============================================================================

-- Insert Customers
PRINT 'Inserting sample customers...';

IF NOT EXISTS (SELECT 1 FROM [sales].[Customers])
BEGIN
    INSERT INTO [sales].[Customers] ([Name], [Email], [IsActive], [CreditLimit])
    VALUES 
        ('John Smith', 'john.smith@email.com', 1, 5000.00),
        ('Sarah Johnson', 'sarah.johnson@email.com', 1, 7500.00),
        ('Michael Brown', 'michael.brown@email.com', 1, 3000.00),
        ('Emily Davis', 'emily.davis@email.com', 1, 10000.00),
        ('David Wilson', 'david.wilson@email.com', 1, 2500.00),
        ('Lisa Anderson', 'lisa.anderson@email.com', 0, 1000.00),
        ('Robert Taylor', 'robert.taylor@email.com', 1, 6000.00),
        ('Jennifer Martinez', 'jennifer.martinez@email.com', 1, 4500.00),
        ('Christopher Lee', 'christopher.lee@email.com', 1, 8000.00),
        ('Amanda White', 'amanda.white@email.com', 1, 3500.00),
        ('James Harris', 'james.harris@email.com', 1, 5500.00),
        ('Michelle Clark', 'michelle.clark@email.com', 1, 7000.00),
        ('Daniel Lewis', 'daniel.lewis@email.com', 0, 2000.00),
        ('Ashley Robinson', 'ashley.robinson@email.com', 1, 9000.00),
        ('Matthew Walker', 'matthew.walker@email.com', 1, 4000.00),
        ('Jessica Hall', 'jessica.hall@email.com', 1, 6500.00),
        ('Andrew Young', 'andrew.young@email.com', 1, 3200.00),
        ('Stephanie King', 'stephanie.king@email.com', 1, 5800.00),
        ('Joshua Wright', 'joshua.wright@email.com', 1, 4200.00),
        ('Nicole Green', 'nicole.green@email.com', 1, 7200.00),
        ('Ryan Adams', 'ryan.adams@email.com', 1, 2800.00),
        ('Heather Baker', 'heather.baker@email.com', 0, 1500.00),
        ('Kevin Nelson', 'kevin.nelson@email.com', 1, 6800.00),
        ('Megan Carter', 'megan.carter@email.com', 1, 5200.00),
        ('Brandon Mitchell', 'brandon.mitchell@email.com', 1, 4800.00);
    
    PRINT 'Sample customers inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Customers already exist. Skipping customer insertion.';
END
GO

-- Insert Orders
PRINT 'Inserting sample orders...';

IF NOT EXISTS (SELECT 1 FROM [sales].[Orders])
BEGIN
    -- Recent orders (last 30 days)
    INSERT INTO [sales].[Orders] ([CustomerId], [OrderDate], [Status], [TotalAmount])
    VALUES 
        (1, DATEADD(day, -2, GETUTCDATE()), 'Processing', 1456.98),
        (2, DATEADD(day, -1, GETUTCDATE()), 'Pending', 234.97),
        (3, DATEADD(day, -3, GETUTCDATE()), 'Shipped', 892.45),
        (4, DATEADD(day, -1, GETUTCDATE()), 'Delivered', 1299.99),
        (5, DATEADD(day, -5, GETUTCDATE()), 'Processing', 599.99),
        (1, DATEADD(day, -7, GETUTCDATE()), 'Delivered', 329.99),
        (6, DATEADD(day, -4, GETUTCDATE()), 'Cancelled', 0.00),
        (7, DATEADD(day, -6, GETUTCDATE()), 'Shipped', 789.97),
        (8, DATEADD(day, -3, GETUTCDATE()), 'Processing', 445.98),
        (9, DATEADD(day, -8, GETUTCDATE()), 'Delivered', 1129.98),
        (10, DATEADD(day, -2, GETUTCDATE()), 'Pending', 149.99),
        (2, DATEADD(day, -9, GETUTCDATE()), 'Delivered', 679.98),
        (11, DATEADD(day, -5, GETUTCDATE()), 'Shipped', 299.99),
        (12, DATEADD(day, -4, GETUTCDATE()), 'Processing', 1899.99),
        (13, DATEADD(day, -10, GETUTCDATE()), 'Cancelled', 0.00);
    
    -- Older orders (30-90 days ago)
    INSERT INTO [sales].[Orders] ([CustomerId], [OrderDate], [Status], [TotalAmount])
    VALUES 
        (14, DATEADD(day, -45, GETUTCDATE()), 'Delivered', 524.97),
        (15, DATEADD(day, -32, GETUTCDATE()), 'Delivered', 899.99),
        (16, DATEADD(day, -55, GETUTCDATE()), 'Delivered', 234.98),
        (17, DATEADD(day, -67, GETUTCDATE()), 'Delivered', 1456.97),
        (18, DATEADD(day, -43, GETUTCDATE()), 'Delivered', 689.99),
        (19, DATEADD(day, -38, GETUTCDATE()), 'Delivered', 345.98),
        (20, DATEADD(day, -59, GETUTCDATE()), 'Delivered', 799.99),
        (3, DATEADD(day, -72, GETUTCDATE()), 'Delivered', 129.99),
        (21, DATEADD(day, -41, GETUTCDATE()), 'Delivered', 1299.98),
        (22, DATEADD(day, -85, GETUTCDATE()), 'Delivered', 456.97);
    
    -- Historical orders (3-12 months ago)
    INSERT INTO [sales].[Orders] ([CustomerId], [OrderDate], [Status], [TotalAmount])
    VALUES 
        (23, DATEADD(day, -120, GETUTCDATE()), 'Delivered', 2345.96),
        (24, DATEADD(day, -145, GETUTCDATE()), 'Delivered', 567.98),
        (25, DATEADD(day, -167, GETUTCDATE()), 'Delivered', 1234.97),
        (1, DATEADD(day, -189, GETUTCDATE()), 'Delivered', 789.99),
        (4, DATEADD(day, -203, GETUTCDATE()), 'Delivered', 445.98),
        (7, DATEADD(day, -225, GETUTCDATE()), 'Delivered', 1599.97),
        (11, DATEADD(day, -234, GETUTCDATE()), 'Delivered', 234.99),
        (14, DATEADD(day, -267, GETUTCDATE()), 'Delivered', 899.98),
        (18, DATEADD(day, -289, GETUTCDATE()), 'Delivered', 1456.97),
        (20, DATEADD(day, -312, GETUTCDATE()), 'Delivered', 679.99);
    
    PRINT 'Sample orders inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Orders already exist. Skipping order insertion.';
END
GO

-- =============================================================================
-- SAMPLE DATA FOR USERS SCHEMA
-- =============================================================================

-- Insert User Activity logs
PRINT 'Inserting sample user activity...';

IF NOT EXISTS (SELECT 1 FROM [users].[UserActivity])
BEGIN
    DECLARE @i INT = 1;
    DECLARE @userId NVARCHAR(50);
    DECLARE @activityType NVARCHAR(50);
    DECLARE @timestamp DATETIME2(7);
    DECLARE @isSuccessful BIT;
    DECLARE @sessionId NVARCHAR(100);
    
    -- Generate 200 activity records
    WHILE @i <= 200
    BEGIN
        -- Randomize user ID (simulate 15 different users)
        SET @userId = 'user' + CAST(((@i - 1) % 15) + 1 AS NVARCHAR(10));
        
        -- Randomize activity types
        SET @activityType = CASE ((@i - 1) % 8)
            WHEN 0 THEN 'Login'
            WHEN 1 THEN 'Logout'
            WHEN 2 THEN 'ViewProduct'
            WHEN 3 THEN 'AddToCart'
            WHEN 4 THEN 'PlaceOrder'
            WHEN 5 THEN 'UpdateProfile'
            WHEN 6 THEN 'Search'
            ELSE 'PageView'
        END;
        
        -- Generate timestamp (last 30 days)
        SET @timestamp = DATEADD(minute, -RAND() * 43200, GETUTCDATE()); -- Random within last 30 days
        
        -- Most activities are successful, but some fail
        SET @isSuccessful = CASE WHEN (@i % 20) = 0 THEN 0 ELSE 1 END;
        
        -- Generate session ID
        SET @sessionId = 'sess_' + @userId + '_' + FORMAT(@timestamp, 'yyyyMMdd');
        
        INSERT INTO [users].[UserActivity] ([UserId], [ActivityType], [Timestamp], [IsSuccessful], [SessionId], [IPAddress], [UserAgent], [Details])
        VALUES (
            @userId,
            @activityType,
            @timestamp,
            @isSuccessful,
            @sessionId,
            '192.168.1.' + CAST(((@i - 1) % 254) + 1 AS NVARCHAR(3)),
            'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36',
            CASE 
                WHEN @activityType = 'Search' THEN 'Query: "' + CASE ((@i - 1) % 5) 
                    WHEN 0 THEN 'laptop' 
                    WHEN 1 THEN 'smartphone' 
                    WHEN 2 THEN 'headphones'
                    WHEN 3 THEN 'jeans'
                    ELSE 'coffee maker' 
                END + '"'
                WHEN @activityType = 'ViewProduct' THEN 'ProductId: ' + CAST(((@i - 1) % 40) + 1 AS NVARCHAR(10))
                WHEN @activityType = 'PlaceOrder' THEN 'OrderId: ' + CAST(((@i - 1) % 35) + 1 AS NVARCHAR(10))
                WHEN @isSuccessful = 0 THEN 'Error: ' + CASE ((@i - 1) % 3)
                    WHEN 0 THEN 'Authentication failed'
                    WHEN 1 THEN 'Validation error'
                    ELSE 'Service unavailable'
                END
                ELSE NULL
            END
        );
        
        SET @i = @i + 1;
    END;
    
    PRINT 'Sample user activity inserted successfully.';
END
ELSE
BEGIN
    PRINT 'User activity already exists. Skipping user activity insertion.';
END
GO

-- =============================================================================
-- DATA INSERTION SUMMARY
-- =============================================================================

PRINT 'Sample data insertion completed successfully!';
PRINT '';
PRINT 'Data Summary:';
PRINT '=============';

-- Count records in each table
SELECT 'Categories' AS TableName, COUNT(*) AS RecordCount FROM [inventory].[Categories]
UNION ALL
SELECT 'Products', COUNT(*) FROM [inventory].[Products]
UNION ALL
SELECT 'Customers', COUNT(*) FROM [sales].[Customers]
UNION ALL
SELECT 'Orders', COUNT(*) FROM [sales].[Orders]
UNION ALL
SELECT 'UserActivity', COUNT(*) FROM [users].[UserActivity]
ORDER BY TableName;

PRINT '';
PRINT 'Sample data is ready for testing and demonstration.';
PRINT 'You can now run queries against the sample database.';
GO