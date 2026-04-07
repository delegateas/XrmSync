# Test script to compare analyzer output from samples 2, 3, and 4
# These samples have the same registrations but differ in framework
# The output JSON MUST be equivalent
#
# Assembly resolution order:
#   1. ILMerged.SamplePlugins.dll  (preferred — tests against the merged platform assembly)
#   2. SamplePlugins.dll           (fallback — requires -AllowNonMerged)
#
# Baseline regression check:
#   Sample 2 (Hybrid) output is compared against a committed baseline file.
#   Use -UpdateBaseline to regenerate the baseline from the current output.

param(
    [switch]$Verbose = $false,
    [switch]$SkipBuild = $false,
    [switch]$OutputNormalizedJson = $false,
    [switch]$AllowNonMerged = $false,
    [switch]$UpdateBaseline = $false,
    [string]$OutputDirectory = ".\test-outputs",
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

Write-Host "=== XrmSync Analyzer Test ===" -ForegroundColor Cyan
Write-Host "Testing analyzer output equivalence for samples 2, 3, and 4" -ForegroundColor Green

# Define paths
$rootPath = Join-Path $PSScriptRoot ..
$xrmSyncPath = Join-Path $rootPath "XrmSync"
$samplesPath = Join-Path $rootPath "Samples"
$baselineFile = Join-Path $PSScriptRoot "baseline-2-Hybrid.json"
$baselineSample = "2-Hybrid"

# Sample projects to test (2, 3, 4 have equivalent registrations)
$samples = @{
    "2-Hybrid" = @{
        ProjectPath = Join-Path $samplesPath "2-Hybrid"
        Framework = "Hybrid (Custom + XrmPluginCore patterns)"
    }
    "3-XrmPluginCore" = @{
        ProjectPath = Join-Path $samplesPath "3-XrmPluginCore"
        Framework = "XrmPluginCore"
    }
    "4-Full-DAXIF" = @{
        ProjectPath = Join-Path $samplesPath "4-Full-DAXIF"
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

    $buildOutput = dotnet build $ProjectPath --configuration $Configuration --verbosity quiet 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build $ProjectName. Output: $buildOutput"
    }
    Write-VerboseOutput "  Build completed successfully"
}

function Get-AssemblyPath {
    param([string]$ProjectPath)

    $ilMergedPath = Join-Path $ProjectPath "bin" $Configuration "net462" "ILMerged.SamplePlugins.dll"
    $nonMergedPath = Join-Path $ProjectPath "bin" $Configuration "net462" "SamplePlugins.dll"

    if (Test-Path $ilMergedPath) {
        Write-VerboseOutput "  Using ILMerged assembly: $ilMergedPath"
        return $ilMergedPath
    }

    if ($AllowNonMerged) {
        Write-Host "  Warning: ILMerged assembly not found, falling back to non-merged assembly" -ForegroundColor Yellow
        Write-VerboseOutput "  Non-merged assembly: $nonMergedPath"
        return $nonMergedPath
    }

    Write-Error "ILMerged assembly not found at: $ilMergedPath`nBuild the project with ILMerge enabled, or pass -AllowNonMerged to fall back to the non-merged assembly."
}

function Run-Analyzer {
    param([string]$AssemblyPath, [string]$SampleName)

    Write-Host "Analyzing $SampleName..." -ForegroundColor Yellow
    Write-VerboseOutput "  Assembly: $AssemblyPath"

    if (-not (Test-Path $AssemblyPath)) {
        Write-Error "Assembly not found at: $AssemblyPath"
    }

    # Run the analyzer using dotnet run
    $analyzeOutput = dotnet run --project $xrmSyncPath -- analyze --assembly $AssemblyPath --publisher-prefix new --pretty-print 2>&1
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

    Write-Host "  Saved normalized JSON: $filename" -ForegroundColor Cyan
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
            Write-Host "  OK: $Sample1Name and $Sample2Name produce equivalent output" -ForegroundColor Green
            if ($OutputNormalizedJson -and $file1 -and $file2) {
                Write-Host "  Files saved. Use: git diff --no-index `"$file1`" `"$file2`"" -ForegroundColor Gray
            }
            return $true
        } else {
            Write-Host "  FAIL: $Sample1Name and $Sample2Name produce different output" -ForegroundColor Red
            if ($OutputNormalizedJson -and $file1 -and $file2) {
                Write-Host "  Diff the saved files: git diff --no-index `"$file1`" `"$file2`"" -ForegroundColor Yellow
                Write-Host "  Files: $file1" -ForegroundColor Gray
                Write-Host "         $file2" -ForegroundColor Gray
            }
            Write-VerboseOutput "Normalized $Sample1Name JSON:"
            Write-VerboseOutput ($normalized1 | ConvertTo-Json -Depth 10)
            Write-VerboseOutput "Normalized $Sample2Name JSON:"
            Write-VerboseOutput ($normalized2 | ConvertTo-Json -Depth 10)
            return $false
        }
    }
    catch {
        Write-Host "  ERROR comparing JSON for $Sample1Name and $Sample2Name : $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Normalize-AssemblyInfo {
    param($AssemblyInfo)

    # Create a normalized copy excluding assembly-specific properties
    $normalized = @{
        Plugins    = $AssemblyInfo.Plugins
        CustomApis = $AssemblyInfo.CustomApis
    }

    return $normalized
}

function Test-BaselineRegression {
    param(
        [string]$CurrentJson,
        [string]$SampleName
    )

    Write-Host "`n=== Baseline Regression Check ($SampleName) ===" -ForegroundColor Cyan

    $currentNormalized = Normalize-AssemblyInfo ($CurrentJson | ConvertFrom-Json)
    $currentCompressed = $currentNormalized | ConvertTo-Json -Depth 10 -Compress

    if ($UpdateBaseline) {
        $currentNormalized | ConvertTo-Json -Depth 10 | Out-File -FilePath $baselineFile -Encoding UTF8
        Write-Host "  Baseline updated: $baselineFile" -ForegroundColor Green
        return $true
    }

    if (-not (Test-Path $baselineFile)) {
        Write-Host "  WARNING: No baseline file found at $baselineFile" -ForegroundColor Yellow
        Write-Host "  Run with -UpdateBaseline to create it." -ForegroundColor Yellow
        return $true  # Don't fail the run — baseline is optional until first created
    }

    $baselineContent = Get-Content $baselineFile -Raw
    $baselineCompressed = ($baselineContent | ConvertFrom-Json) | ConvertTo-Json -Depth 10 -Compress

    if ($currentCompressed -eq $baselineCompressed) {
        Write-Host "  OK: $SampleName output matches baseline" -ForegroundColor Green
        return $true
    } else {
        Write-Host "  FAIL: $SampleName output differs from baseline ($baselineFile)" -ForegroundColor Red
        Write-Host "  If this change is intentional, run with -UpdateBaseline to update the baseline." -ForegroundColor Yellow
        return $false
    }
}

function Test-AnalyzerEquivalence {
    Write-Host "`n=== Building Projects ===" -ForegroundColor Cyan

    if (-not $SkipBuild) {
        # Build XrmSync tool first
        Write-Host "Building XrmSync tool..." -ForegroundColor Yellow
        $buildOutput = dotnet build $xrmSyncPath --configuration $Configuration --verbosity quiet 2>&1
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

    if ($AllowNonMerged) {
        Write-Host "Note: -AllowNonMerged enabled — will fall back to SamplePlugins.dll if ILMerged not present" -ForegroundColor Yellow
    }

    if ($OutputNormalizedJson) {
        Write-Host "Normalized JSON files will be saved to: $OutputDirectory" -ForegroundColor Cyan
    }

    # Run analyzer on each sample
    $results = @{}
    foreach ($sampleName in $samples.Keys) {
        $assemblyPath = Get-AssemblyPath $samples[$sampleName].ProjectPath
        $results[$sampleName] = @{
            Json      = Run-Analyzer $assemblyPath $sampleName
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

    # Baseline regression check against sample 2
    $baselineOk = Test-BaselineRegression $results[$baselineSample].Json $baselineSample
    $allEqual = $allEqual -and $baselineOk

    Write-Host "`n=== Test Results ===" -ForegroundColor Cyan

    if ($allEqual) {
        Write-Host "SUCCESS: All samples produce equivalent analyzer output and match the baseline!" -ForegroundColor Green
        Write-Host "This confirms that the analyzer correctly handles different plugin frameworks" -ForegroundColor Green
    } else {
        Write-Host "FAILURE: One or more checks failed — see above for details." -ForegroundColor Red
    }

    if ($OutputNormalizedJson) {
        Write-Host "`nNormalized JSON files saved in: $OutputDirectory" -ForegroundColor Cyan
        Write-Host "Use your favorite diff tool to compare the files for detailed analysis" -ForegroundColor Gray
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

# Run the test
$success = Test-AnalyzerEquivalence

# Exit with appropriate code
if ($success) {
    exit 0
} else {
    exit 1
}
