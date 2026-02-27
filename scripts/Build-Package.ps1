# Local build, test, and pack script mirroring the CI pipeline
param(
    [string]$Configuration = "Debug",
    [string]$Output = "XrmSync/nupkg",
    [switch]$BumpPreview
)

$ErrorActionPreference = "Stop"

$rootPath = Join-Path $PSScriptRoot ..
$csprojPath = Join-Path $rootPath "XrmSync/XrmSync.csproj"
$changelogPath = Join-Path $rootPath "CHANGELOG.md"
$testsPath = Join-Path $rootPath "Tests"

Write-Host "=== XrmSync Build & Pack ===" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Gray
Write-Host "Output: $Output" -ForegroundColor Gray

# 0. Bump preview version in CHANGELOG.md
if ($BumpPreview) {
    Write-Host "`nBumping preview version..." -ForegroundColor Yellow
    $content = Get-Content $changelogPath
    $regex = '^(### v?\d+\.\d+\.\d+-\w+\.)(\d+)( - ).+$'
    if ($content[0] -match $regex) {
        $oldVersion = $content[0]
        $newNumber = [int]$matches[2] + 1
        $today = Get-Date -Format "dd MMMM yyyy"
        $content[0] = "$($matches[1])$newNumber$($matches[3])$today"
        $content | Set-Content $changelogPath
        Write-Host "  $oldVersion" -ForegroundColor Gray
        Write-Host "  $($content[0])" -ForegroundColor Green
    } else {
        Write-Error "First line of CHANGELOG.md does not match preview version pattern: $($content[0])"
    }
}

# 1. Set version from CHANGELOG.md
Write-Host "`nSetting version from CHANGELOG.md..." -ForegroundColor Yellow
& "$PSScriptRoot/Set-VersionFromChangelog.ps1" -ChangelogPath $changelogPath -CsprojPath $csprojPath
if ($LASTEXITCODE -ne 0) { Write-Error "Failed to set version" }

# 2. Restore dependencies
Write-Host "`nRestoring dependencies..." -ForegroundColor Yellow
dotnet restore $csprojPath
if ($LASTEXITCODE -ne 0) { Write-Error "Failed to restore dependencies" }

# 3. Build
Write-Host "`nBuilding..." -ForegroundColor Yellow
dotnet build $csprojPath --no-restore --configuration $Configuration
if ($LASTEXITCODE -ne 0) { Write-Error "Build failed" }

# 4. Test
Write-Host "`nRunning tests..." -ForegroundColor Yellow
dotnet test $testsPath --no-build --configuration $Configuration --verbosity normal
if ($LASTEXITCODE -ne 0) { Write-Error "Tests failed" }

# 5. Pack
Write-Host "`nPacking NuGet package..." -ForegroundColor Yellow
dotnet pack $csprojPath --no-build --configuration $Configuration --output $Output
if ($LASTEXITCODE -ne 0) { Write-Error "Pack failed" }

Write-Host "`nDone! Package output: $Output" -ForegroundColor Green
