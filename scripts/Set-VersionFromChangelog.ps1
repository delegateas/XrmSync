# Sets the <Version> property in the given .csproj file to the latest version found in CHANGELOG.md (keepachangelog format)
param(
    [string]$ChangelogPath,
    [string]$CsprojPath
)

$changelog = Get-Content $ChangelogPath
# Match lines like: ### v1.0.0-rc.1 - xx xxxx 2025

$regex = '^### v?([0-9]+\.[0-9]+\.[0-9]+(-[A-Za-z0-9\-\.]+)?)'
$resolved = Resolve-Path $CsprojPath

$versionLine = $changelog | Where-Object { $_ -match $regex } | Select-Object -First 1
if ($versionLine -match $regex) {
    $version = $matches[1]
    Write-Host "Detected version: $version"
    [xml]$xml = Get-Content $resolved

    $propertyGroup = $xml.Project.PropertyGroup | Where-Object { $_.Version -or $_.OutputType }
    if (-not $propertyGroup.Version) {
        $versionElement = $xml.CreateElement('Version')
        $versionElement.InnerText = $version
        $propertyGroup.AppendChild($versionElement) | Out-Null
    } else {
        $propertyGroup.Version = $version
    }

    $xml.Save($resolved)
    Write-Host "Updated $CsprojPath with version $version"
} else {
    Write-Error 'No version found in CHANGELOG.md'
    exit 1
}
