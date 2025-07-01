# Sets the <Version> property in the given .csproj file to the latest version found in CHANGELOG.md (keepachangelog format)
param(
    [string]$ChangelogPath = "CHANGELOG.md",
    [string]$CsprojPath = "XrmPluginSync/XrmPluginSync.csproj"
)

$changelog = Get-Content $ChangelogPath
# Match lines like: ### v1.0.0-rc1 - xx xxxx 2025
$versionLine = $changelog | Where-Object { $_ -match '^### v([0-9]+\.[0-9]+\.[0-9]+(-[A-Za-z0-9]+)?)' } | Select-Object -First 1
if ($versionLine -match '^### v([0-9]+\.[0-9]+\.[0-9]+(-[A-Za-z0-9]+)?)') {
    $version = $matches[1]
    Write-Host "Detected version: $version"
    [xml]$xml = Get-Content $CsprojPath
    $propertyGroup = $xml.Project.PropertyGroup | Where-Object { $_.Version -or $_.OutputType }
    if (-not $propertyGroup.Version) {
        $versionElement = $xml.CreateElement('Version')
        $versionElement.InnerText = $version
        $propertyGroup.AppendChild($versionElement) | Out-Null
    } else {
        $propertyGroup.Version = $version
    }
    $xml.Save($CsprojPath)
    Write-Host "Updated $CsprojPath with version $version"
} else {
    Write-Error 'No version found in CHANGELOG.md'
    exit 1
}
