/*
=====================================================================================
01_CreateDatabase.sql
Database Creation Script for FractalDataWorks Sample Application
=====================================================================================

Purpose:
Creates the SampleDb database with appropriate settings for LocalDB development.
This script is safe to run multiple times.

Usage:
Execute this script first before running any other database scripts.

Notes:
- Designed for SQL Server LocalDB
- Uses default file locations suitable for LocalDB
- Sets appropriate collation for multi-language support
- Enables proper transaction isolation
=====================================================================================
*/

USE master;
GO

-- Check if database already exists
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SampleDb')
BEGIN
    PRINT 'Creating SampleDb database...';
    
    -- Create the database with appropriate settings for LocalDB
    CREATE DATABASE [SampleDb]
    ON 
    ( 
        NAME = 'SampleDb',
        FILENAME = 'SampleDb.mdf',
        SIZE = 50MB,
        MAXSIZE = 500MB,
        FILEGROWTH = 10MB
    )
    LOG ON 
    ( 
        NAME = 'SampleDb_Log',
        FILENAME = 'SampleDb_Log.ldf',
        SIZE = 10MB,
        MAXSIZE = 100MB,
        FILEGROWTH = 10%
    )
    COLLATE SQL_Latin1_General_CP1_CI_AS;
    
    PRINT 'SampleDb database created successfully.';
END
ELSE
BEGIN
    PRINT 'SampleDb database already exists. Skipping creation.';
END
GO

-- Set database options for optimal development experience
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'SampleDb')
BEGIN
    PRINT 'Configuring SampleDb database options...';
    
    ALTER DATABASE [SampleDb] SET RECOVERY SIMPLE;
    ALTER DATABASE [SampleDb] SET AUTO_CLOSE OFF;
    ALTER DATABASE [SampleDb] SET AUTO_SHRINK OFF;
    ALTER DATABASE [SampleDb] SET AUTO_CREATE_STATISTICS ON;
    ALTER DATABASE [SampleDb] SET AUTO_UPDATE_STATISTICS ON;
    ALTER DATABASE [SampleDb] SET READ_COMMITTED_SNAPSHOT ON;
    
    PRINT 'SampleDb database configuration completed.';
END
GO

-- Verify database creation
USE [SampleDb];
GO

IF DB_NAME() = 'SampleDb'
BEGIN
    PRINT 'Successfully connected to SampleDb database.';
    PRINT 'Database is ready for schema creation.';
END
ELSE
BEGIN
    PRINT 'ERROR: Failed to connect to SampleDb database.';
    RAISERROR('Database creation failed', 16, 1);
END
GO