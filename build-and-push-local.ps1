# Build and push all packages to local NuGet repository
param(
    [string]$Configuration = "Local",
    [string]$LocalNuGetPath = "C:\development\LocalNuGet"
)

Write-Host "Building Developer-Kit packages in $Configuration configuration..." -ForegroundColor Green

# Clean previous build
Write-Host "Cleaning previous build..." -ForegroundColor Yellow
dotnet clean -c $Configuration

# Build the solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build -c $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Pack all projects
Write-Host "Packing projects..." -ForegroundColor Yellow
dotnet pack -c $Configuration --no-build -o .\nupkgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "Pack failed!" -ForegroundColor Red
    exit 1
}

# Push packages to local NuGet
Write-Host "Pushing packages to local NuGet at $LocalNuGetPath..." -ForegroundColor Yellow

# Create local NuGet directory if it doesn't exist
if (!(Test-Path $LocalNuGetPath)) {
    New-Item -ItemType Directory -Path $LocalNuGetPath | Out-Null
}

# Copy all packages to local NuGet
Get-ChildItem -Path ".\nupkgs\*.nupkg" | ForEach-Object {
    Write-Host "  Pushing $($_.Name)..." -ForegroundColor Gray
    Copy-Item $_.FullName -Destination $LocalNuGetPath -Force
}

# Clear NuGet cache for FractalDataWorks packages
Write-Host "Clearing NuGet cache for FractalDataWorks packages..." -ForegroundColor Yellow
dotnet nuget locals all --clear

Write-Host "Done! Packages pushed to $LocalNuGetPath" -ForegroundColor Green
Write-Host "You can now restore packages in Enhanced-Enums using the Local configuration." -ForegroundColor Cyan