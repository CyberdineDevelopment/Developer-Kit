#Requires -Version 7.0
# Quick setup script - Just set up LocalDB and run SQL scripts

param(
    [switch]$Reset
)

$ErrorActionPreference = 'Stop'

Write-Host "`n=== LocalDB Quick Setup ===" -ForegroundColor Cyan

# Configuration
$instanceName = "MSSQLLocalDB"
$databaseName = "SampleDb"
$scriptsPath = Join-Path (Split-Path $PSScriptRoot) "Scripts"

# Check if LocalDB is installed
Write-Host "`nChecking LocalDB..." -ForegroundColor Yellow
try {
    $instances = sqllocaldb info 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "LocalDB not installed. Install SQL Server Express LocalDB first."
    }
    Write-Host "✓ LocalDB is installed" -ForegroundColor Green
} catch {
    Write-Host "✗ $_" -ForegroundColor Red
    exit 1
}

# Create/Start LocalDB instance
Write-Host "`nStarting LocalDB instance..." -ForegroundColor Yellow
$instanceInfo = sqllocaldb info $instanceName 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Creating instance: $instanceName"
    sqllocaldb create $instanceName
}

sqllocaldb start $instanceName
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ LocalDB instance '$instanceName' is running" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to start LocalDB" -ForegroundColor Red
    exit 1
}

# Reset database if requested
if ($Reset) {
    Write-Host "`nDropping existing database..." -ForegroundColor Yellow
    $dropSql = @"
USE master;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = '$databaseName')
BEGIN
    ALTER DATABASE [$databaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [$databaseName];
    PRINT 'Database dropped';
END
GO
"@
    
    $dropSql | sqlcmd -S "(localdb)\$instanceName" -E 2>&1 | Out-Null
    Write-Host "✓ Database reset" -ForegroundColor Green
}

# Get SQL scripts
Write-Host "`nFinding SQL scripts..." -ForegroundColor Yellow
$sqlFiles = Get-ChildItem -Path $scriptsPath -Filter "*.sql" | Sort-Object Name

if ($sqlFiles.Count -eq 0) {
    Write-Host "✗ No SQL scripts found in: $scriptsPath" -ForegroundColor Red
    exit 1
}

Write-Host "Found $($sqlFiles.Count) SQL scripts:" -ForegroundColor Green
$sqlFiles | ForEach-Object { Write-Host "  - $($_.Name)" }

# Execute each SQL script
Write-Host "`nExecuting SQL scripts..." -ForegroundColor Yellow

foreach ($sqlFile in $sqlFiles) {
    Write-Host "`n▶ Running: $($sqlFile.Name)" -ForegroundColor Cyan
    
    # Determine target database
    $targetDb = if ($sqlFile.Name -match "01.*Create.*Database") { "master" } else { $databaseName }
    
    # Execute script
    $output = sqlcmd -S "(localdb)\$instanceName" -d $targetDb -E -i $sqlFile.FullName 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Success" -ForegroundColor Green
        if ($output) {
            $output | Where-Object { $_ -and $_ -notmatch "^$" } | ForEach-Object { 
                Write-Host "    $_" -ForegroundColor DarkGray 
            }
        }
    } else {
        Write-Host "  ✗ Failed" -ForegroundColor Red
        if ($output) {
            $output | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
        }
        exit 1
    }
}

# Test connection
Write-Host "`nTesting database connection..." -ForegroundColor Yellow
$testQuery = "SELECT DB_NAME() as DbName, COUNT(*) as TableCount FROM sys.tables"
$result = sqlcmd -S "(localdb)\$instanceName" -d $databaseName -E -Q $testQuery -h -1 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Database is ready!" -ForegroundColor Green
    Write-Host "`nConnection info:" -ForegroundColor Cyan
    Write-Host "  Server: (localdb)\$instanceName" 
    Write-Host "  Database: $databaseName"
    Write-Host "  Connection String: Server=(localdb)\$instanceName;Database=$databaseName;Integrated Security=true;TrustServerCertificate=true;"
} else {
    Write-Host "✗ Connection test failed" -ForegroundColor Red
    exit 1
}

Write-Host "`n✅ Setup complete!" -ForegroundColor Green