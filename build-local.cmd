@echo off
echo Building with local packages...
echo.

REM Set the environment variable to use local packages
set UseLocalPackages=true

REM Copy the local nuget config
copy /Y nuget.config.local nuget.config > nul

REM Clean and restore with local packages
echo Cleaning previous build...
dotnet clean -v minimal

echo.
echo Restoring with local packages...
dotnet restore

echo.
echo Building solution...
dotnet build %*

REM Restore original nuget.config
echo.
echo Restoring original nuget.config...
git checkout nuget.config 2>nul || copy /Y nuget.config.local.backup nuget.config > nul

echo.
echo Build complete!