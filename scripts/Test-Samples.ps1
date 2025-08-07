# Test script to compare analyzer output from samples 2, 3, and 4
# These samples have the same registrations but differ in framework
# The output JSON MUST be equivalent

param(
    [switch]$Verbose = $false,
    [switch]$SkipBuild = $false,
    [switch]$OutputNormalizedJson = $false,
    [string]$OutputDirectory = ".\test-outputs"
)

$ErrorActionPreference = "Stop"

Write-Host "=== XrmSync Analyzer Test ===" -ForegroundColor Cyan
Write-Host "Testing analyzer output equivalence for samples 2, 3, and 4" -ForegroundColor Green

# Define paths
$rootPath = Join-Path $PSScriptRoot ..
$xrmSyncPath = Join-Path $rootPath "XrmSync"
$samplesPath = Join-Path $rootPath "Samples"

# Sample projects to test (2, 3, 4 have equivalent registrations)
$samples = @{
    "2-Hybrid" = @{
        ProjectPath = Join-Path $samplesPath "2-Hybrid"
        AssemblyName = "ILMerged.SamplePlugins.dll"
        Framework = "Hybrid (Custom + XrmPluginCore patterns)"
    }
    "3-XrmPluginCore" = @{
        ProjectPath = Join-Path $samplesPath "3-XrmPluginCore" 
        AssemblyName = "ILMerged.SamplePlugins.dll"
        Framework = "XrmPluginCore"
    }
    "4-Full-DAXIF" = @{
        ProjectPath = Join-Path $samplesPath "4-Full-DAXIF"
        AssemblyName = "ILMerged.SamplePlugins.dll" 
        Framework = "Custom Plugin base class (Extended)"
    }
}

function Write-VerboseOutput {
    param([string]$Message)
    if ($Verbose) {
        Write-Host $Message -ForegroundColor Gray
    }
}

function Build-Project {
    param([string]$ProjectPath, [string]$ProjectName)
    
    Write-Host "Building $ProjectName..." -ForegroundColor Yellow
    Write-VerboseOutput "  Project path: $ProjectPath"
    
    $buildOutput = dotnet build $ProjectPath --configuration Debug --verbosity quiet 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build $ProjectName. Output: $buildOutput"
    }
    Write-VerboseOutput "  Build completed successfully"
}

function Get-AssemblyPath {
    param([string]$ProjectPath, [string]$AssemblyName)
    
    return Join-Path $ProjectPath "bin" "Debug" "net462" $AssemblyName
}

function Run-Analyzer {
    param([string]$AssemblyPath, [string]$SampleName)
    
    Write-Host "Analyzing $SampleName..." -ForegroundColor Yellow
    Write-VerboseOutput "  Assembly: $AssemblyPath"
    
    if (-not (Test-Path $AssemblyPath)) {
        Write-Error "Assembly not found at: $AssemblyPath"
    }
    
    # Run the analyzer using dotnet run
    $analyzeOutput = dotnet run --project $xrmSyncPath -- analyze --assembly $AssemblyPath --pretty-print 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to analyze $SampleName. Output: $analyzeOutput"
    }
    
    # Filter out any non-JSON output (like build messages)
    $jsonLines = $analyzeOutput | Where-Object { 
        $_ -match '^\s*[{\[]' -or 
        $_ -match '^\s*[}\]]' -or 
        $_ -match '^\s*"' -or 
        $_ -match '^\s*,' 
    }
    $jsonOutput = $jsonLines -join "`n"
    
    Write-VerboseOutput "  Analysis completed"
    return $jsonOutput
}

function Save-NormalizedJsonToFile {
    param(
        [object]$NormalizedObject,
        [string]$SampleName,
        [string]$OutputDirectory
    )
    
    if (-not $OutputNormalizedJson) {
        return
    }
    
    # Ensure output directory exists
    if (-not (Test-Path $OutputDirectory)) {
        New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
        Write-VerboseOutput "Created output directory: $OutputDirectory"
    }
    
    # Generate filename with timestamp for uniqueness
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $filename = "$SampleName-normalized-$timestamp.json"
    $filePath = Join-Path $OutputDirectory $filename
    
    # Convert to pretty-printed JSON and save
    $prettyJson = $NormalizedObject | ConvertTo-Json -Depth 10
    $prettyJson | Out-File -FilePath $filePath -Encoding UTF8
    
    Write-Host "  ?? Saved normalized JSON: $filename" -ForegroundColor Cyan
    return $filePath
}

function Compare-JsonObjects {
    param(
        [string]$Json1,
        [string]$Json2,
        [string]$Sample1Name,
        [string]$Sample2Name
    )
    
    try {
        $obj1 = $Json1 | ConvertFrom-Json
        $obj2 = $Json2 | ConvertFrom-Json
        
        # Normalize objects for comparison (remove assembly-specific properties)
        $normalized1 = Normalize-AssemblyInfo $obj1
        $normalized2 = Normalize-AssemblyInfo $obj2
        
        # Save normalized JSON to files if requested
        $file1 = Save-NormalizedJsonToFile $normalized1 $Sample1Name $OutputDirectory
        $file2 = Save-NormalizedJsonToFile $normalized2 $Sample2Name $OutputDirectory
        
        $json1Normalized = $normalized1 | ConvertTo-Json -Depth 10 -Compress
        $json2Normalized = $normalized2 | ConvertTo-Json -Depth 10 -Compress
        
        if ($json1Normalized -eq $json2Normalized) {
            Write-Host "? $Sample1Name and $Sample2Name produce equivalent output" -ForegroundColor Green
            if ($OutputNormalizedJson -and $file1 -and $file2) {
                Write-Host "  ?? Files saved for comparison. Use: git diff --no-index `"$file1`" `"$file2`"" -ForegroundColor Gray
            }
            return $true
        } else {
            Write-Host "? $Sample1Name and $Sample2Name produce different output" -ForegroundColor Red
            if ($OutputNormalizedJson -and $file1 -and $file2) {
                Write-Host "  ?? Diff the saved files: git diff --no-index `"$file1`" `"$file2`"" -ForegroundColor Yellow
                Write-Host "  ?? Files: $file1" -ForegroundColor Gray
                Write-Host "  ??       $file2" -ForegroundColor Gray
            }
            Write-VerboseOutput "Normalized $Sample1Name JSON:"
            Write-VerboseOutput ($normalized1 | ConvertTo-Json -Depth 10)
            Write-VerboseOutput "Normalized $Sample2Name JSON:"
            Write-VerboseOutput ($normalized2 | ConvertTo-Json -Depth 10)
            return $false
        }
    }
    catch {
        Write-Host "? Error comparing JSON for $Sample1Name and $Sample2Name : $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Normalize-AssemblyInfo {
    param($AssemblyInfo)
    
    # Create a normalized copy excluding assembly-specific properties
    $normalized = @{
        Plugins = $AssemblyInfo.Plugins
        CustomApis = $AssemblyInfo.CustomApis
    }
    
    return $normalized
}

function Test-AnalyzerEquivalence {
    Write-Host "`n=== Building Projects ===" -ForegroundColor Cyan
    
    if (-not $SkipBuild) {
        # Build XrmSync tool first
        Write-Host "Building XrmSync tool..." -ForegroundColor Yellow
        $buildOutput = dotnet build $xrmSyncPath --configuration Debug --verbosity quiet 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to build XrmSync tool. Output: $buildOutput"
        }
        
        # Build sample projects
        foreach ($sampleName in $samples.Keys) {
            Build-Project $samples[$sampleName].ProjectPath $sampleName
        }
    } else {
        Write-Host "Skipping build (--SkipBuild specified)" -ForegroundColor Yellow
    }
    
    Write-Host "`n=== Running Analysis ===" -ForegroundColor Cyan
    
    if ($OutputNormalizedJson) {
        Write-Host "?? Normalized JSON files will be saved to: $OutputDirectory" -ForegroundColor Cyan
    }
    
    # Run analyzer on each sample
    $results = @{}
    foreach ($sampleName in $samples.Keys) {
        $assemblyPath = Get-AssemblyPath $samples[$sampleName].ProjectPath $samples[$sampleName].AssemblyName
        $results[$sampleName] = @{
            Json = Run-Analyzer $assemblyPath $sampleName
            Framework = $samples[$sampleName].Framework
        }
    }
    
    Write-Host "`n=== Comparing Results ===" -ForegroundColor Cyan
    
    # Compare each pair of samples
    $sampleNames = @($samples.Keys | Sort-Object)
    $allEqual = $true
    
    for ($i = 0; $i -lt $sampleNames.Count - 1; $i++) {
        for ($j = $i + 1; $j -lt $sampleNames.Count; $j++) {
            $sample1 = $sampleNames[$i]
            $sample2 = $sampleNames[$j]
            
            Write-Host "`nComparing $sample1 vs ${sample2}:" -ForegroundColor White
            Write-VerboseOutput "  $sample1 Framework: $($results[$sample1].Framework)"
            Write-VerboseOutput "  $sample2 Framework: $($results[$sample2].Framework)"
            
            $isEqual = Compare-JsonObjects $results[$sample1].Json $results[$sample2].Json $sample1 $sample2
            $allEqual = $allEqual -and $isEqual
        }
    }
    
    Write-Host "`n=== Test Results ===" -ForegroundColor Cyan
    
    if ($allEqual) {
        Write-Host "?? SUCCESS: All samples produce equivalent analyzer output!" -ForegroundColor Green
        Write-Host "This confirms that the analyzer correctly handles different plugin frameworks" -ForegroundColor Green
    } else {
        Write-Host "? FAILURE: Samples produce different analyzer output!" -ForegroundColor Red
        Write-Host "This indicates an issue with analyzer framework compatibility" -ForegroundColor Red
    }
    
    if ($OutputNormalizedJson) {
        Write-Host "`n?? Normalized JSON files saved in: $OutputDirectory" -ForegroundColor Cyan
        Write-Host "?? Use your favorite diff tool to compare the files for detailed analysis" -ForegroundColor Gray
    }
    
    if ($Verbose) {
        Write-Host "`n=== Raw JSON Outputs ===" -ForegroundColor Cyan
        foreach ($sampleName in $sampleNames) {
            Write-Host "`n$sampleName Output:" -ForegroundColor White
            Write-Host $results[$sampleName].Json -ForegroundColor Gray
        }
    }
    
    return $allEqual
}

# Show usage information
if ($OutputNormalizedJson) {
    Write-Host "?? Normalized JSON output enabled" -ForegroundColor Green
    Write-Host "   Output directory: $OutputDirectory" -ForegroundColor Gray
    Write-Host "   Files will be timestamped for uniqueness" -ForegroundColor Gray
}

# Run the test
$success = Test-AnalyzerEquivalence

# Exit with appropriate code
if ($success) {
    exit 0
} else {
    exit 1
}