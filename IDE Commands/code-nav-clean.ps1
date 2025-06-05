# Unity Code Navigation Tool
# Advanced code search and navigation utilities for Unity projects

param(
    [string]$SearchTerm = "",
    [string]$FileType = "",
    [switch]$Classes,
    [switch]$Methods,
    [switch]$Properties,
    [switch]$Large,
    [switch]$Duplicates,
    [switch]$Dependencies,
    [switch]$Stats,
    [switch]$Help
)

# Define $ProjectRoot and $AssetsPath early, but check $AssetsPath validity later, before use.
$ProjectRoot = (Get-Location).Path # Ensure we get the string path
$AssetsPath = Join-Path $ProjectRoot "Assets"

# Function to write status messages
function Write-NavStatus {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

# Function to find all classes
function Find-Classes {
    Write-NavStatus "Finding all classes..." "Cyan"
    
    $classPattern = '^\s*(?:public\s+|private\s+|protected\s+|internal\s+)*(?:abstract\s+|sealed\s+|static\s+)*class\s+(\w+)'
    
    Get-ChildItem -Path $AssetsPath -Recurse -Include "*.cs" -File | ForEach-Object {
        $file = $_
        $contentLines = Get-Content $file.FullName -ErrorAction SilentlyContinue
        $lineNumber = 0
        
        foreach ($line in $contentLines) {
            $lineNumber++
            if ($line -match $classPattern) {
                $className = $matches[1]
                $relPath = $file.FullName.Replace($ProjectRoot, "").TrimStart("\/") # Trim both \ and /
                Write-Host "Class: $className" -ForegroundColor Green
                Write-Host "  File: ${relPath}:${lineNumber}" -ForegroundColor Gray
            }
        }
    }
}

# Function to find all methods
function Find-Methods {
    Write-NavStatus "Finding all methods..." "Cyan"
    
    $methodPattern = '^\s*(?:public\s+|private\s+|protected\s+|internal\s+)*(?:static\s+|virtual\s+|override\s+|abstract\s+|async\s+)*(?:[\w\.<>\[\]]+\s+)+(\w+)\s*\([^)]*\)\s*(?:where\s+\w+\s*:\s*[\w\.<>\[\]]+)?\s*{?'
    
    $filter = $(if ($FileType) { "*.$FileType" } else { "*.cs" })
    
    Get-ChildItem -Path $AssetsPath -Recurse -Include $filter -File | ForEach-Object {
        $file = $_
        $contentLines = Get-Content $file.FullName -ErrorAction SilentlyContinue
        $lineNumber = 0
        
        foreach ($line in $contentLines) {
            $lineNumber++
            if ($line -match $methodPattern -and $line -notmatch '^\s*//') {
                $methodName = $matches[1]
                $keywordsToSkip = @('if', 'while', 'for', 'foreach', 'switch', 'using', 'namespace', 'class', 'struct', 'enum', 'return', 'new', 'get', 'set', 'add', 'remove', 'yield', 'throw', 'catch', 'finally', 'lock', 'fixed', 'checked', 'unchecked', 'unsafe', 'volatile', 'explicit', 'implicit', 'operator')
                if ($methodName -notin $keywordsToSkip -and $methodName -notlike '*=*' -and $methodName -ne $null -and $methodName.Length -gt 0) {
                    $relPath = $file.FullName.Replace($ProjectRoot, "").TrimStart("\/")
                    Write-Host "Method: $methodName" -ForegroundColor Yellow
                    Write-Host "  File: ${relPath}:${lineNumber}" -ForegroundColor Gray
                    Write-Host "  Line: $($line.Trim())" -ForegroundColor DarkGray
                }
            }
        }
    }
}

# Function to find all properties
function Find-Properties {
    Write-NavStatus "Finding all properties..." "Cyan"
    
    $propertyPattern = '^\s*(?:public\s+|private\s+|protected\s+|internal\s+)*(?:static\s+|virtual\s+|override\s+|abstract\s+)*[\w\.<>\[\]]+\s+(\w+)\s*{\s*(?:get;|set;|init;|get\s*{.*}\s*(?:private\s+|protected\s+)?set\s*{.*}|set\s*{.*}\s*(?:private\s+|protected\s+)?get\s*{.*}|get\s*{.*}\s*(?:private\s+|protected\s+)?init\s*{.*}|init\s*{.*}\s*(?:private\s+|protected\s+)?get\s*{.*})'
    
    $filter = $(if ($FileType) { "*.$FileType" } else { "*.cs" })
    
    Get-ChildItem -Path $AssetsPath -Recurse -Include $filter -File | ForEach-Object {
        $file = $_
        $contentLines = Get-Content $file.FullName -ErrorAction SilentlyContinue
        $lineNumber = 0
        
        foreach ($line in $contentLines) {
            $lineNumber++
            if ($line -match $propertyPattern) {
                $propertyName = $matches[1]
                $relPath = $file.FullName.Replace($ProjectRoot, "").TrimStart("\/")
                Write-Host "Property: $propertyName" -ForegroundColor Magenta
                Write-Host "  File: ${relPath}:${lineNumber}" -ForegroundColor Gray
            }
        }
    }
}

# Function to search for a specific term in code
function Search-Code {
    param([string]$Term)
    
    Write-NavStatus "Searching for: $Term" "Cyan"
    
    $includePatterns = @("*.cs", "*.js", "*.ts", "*.shader", "*.hlsl", "*.cginc", "*.compute", "*.json", "*.xml", "*.txt", "*.md", "*.asmdef")
    
    if ($FileType) {
        $includePatterns = @("*.$FileType")
    }
    
    Get-ChildItem -Path $AssetsPath -Recurse -Include $includePatterns -File | ForEach-Object {
        $fileItem = $_ # Renamed to avoid conflict with Select-String's $_
        Select-String -Path $fileItem.FullName -Pattern $Term -CaseSensitive:$false -ErrorAction SilentlyContinue | ForEach-Object {
            # $_ here is the MatchInfo object from Select-String
            if ($null -ne $_.LineNumber) {
                $relPath = $fileItem.FullName.Replace($ProjectRoot, "").TrimStart("\/")
                Write-Host "Found in: ${relPath}:$($_.LineNumber)" -ForegroundColor Green
                Write-Host "  $($_.Line.Trim())" -ForegroundColor White
            } else {
                 Write-Warning "Match found in $($fileItem.FullName) but LineNumber is unavailable. Line: $($_.Line.Trim())"
            }
        }
    }
}

# Function to find large files
function Find-LargeFiles {
    Write-NavStatus "Finding large files (>100KB)..." "Cyan"
    
    Get-ChildItem -Path $AssetsPath -Recurse -File | 
        Where-Object { $_.Length -gt 100KB } |
        Sort-Object Length -Descending |
        ForEach-Object {
            $sizeFormatted = ""
            if ($_.Length -ge 1MB) {
                $sizeFormatted = "[System.Math]::Round($_.Length / 1MB, 2).ToString('F2') MB"
            } elseif ($_.Length -ge 1KB) {
                $sizeFormatted = "[System.Math]::Round($_.Length / 1KB, 2).ToString('F2') KB"
            } else {
                $sizeFormatted = "$($_.Length) Bytes"
            }
            $relPath = $_.FullName.Replace($ProjectRoot, "").TrimStart("\/")
            $color = $(if ($_.Length -gt 10MB) { "Red" } elseif ($_.Length -gt 5MB) { "Yellow" } else { "White" }) # Adjusted thresholds for MB
            Write-Host "Large File: $relPath ($sizeFormatted)" -ForegroundColor $color
        }
}

# Function to find duplicate file names
function Find-Duplicates {
    Write-NavStatus "Finding duplicate file names..." "Cyan"
    
    Get-ChildItem -Path $AssetsPath -Recurse -File |
        Group-Object Name |
        Where-Object { $_.Count -gt 1 } |
        ForEach-Object {
            Write-Host "Duplicate: $($_.Name)" -ForegroundColor Red
            $_.Group | ForEach-Object {
                $relPath = $_.FullName.Replace($ProjectRoot, "").TrimStart("\/")
                Write-Host "  $relPath" -ForegroundColor Yellow
            }
        }
}

# Function to show dependency analysis (using statements)
function Show-Dependencies {
    Write-NavStatus "Analyzing dependencies (used namespaces)..." "Cyan"
    
    $usingPattern = '^\s*using\s+([^;]+);'
    # Using a case-insensitive dictionary for namespaces
    $namespaceUsage = [System.Collections.Generic.Dictionary[string, int]]::new([System.StringComparer]::OrdinalIgnoreCase)
    
    Get-ChildItem -Path $AssetsPath -Recurse -Include "*.cs" -File | ForEach-Object {
        $contentLines = Get-Content $_.FullName -ErrorAction SilentlyContinue
        
        foreach ($line in $contentLines) {
            if ($line -match $usingPattern) {
                $namespace = $matches[1].Trim()
                if ($namespaceUsage.ContainsKey($namespace)) {
                    $namespaceUsage[$namespace]++
                } else {
                    $namespaceUsage[$namespace] = 1
                }
            }
        }
    }
    
    Write-Host "Most used namespaces:" -ForegroundColor Green
    if ($namespaceUsage.Count -eq 0) {
        Write-Host "  No 'using' statements found in *.cs files." -ForegroundColor Yellow
        return
    }

    $namespaceUsage.GetEnumerator() | 
        Sort-Object Value -Descending | 
        Select-Object -First 15 |
        ForEach-Object {
            Write-Host "  $($_.Key): $($_.Value) occurrences" -ForegroundColor White
        }
}

# Function to show project statistics
function Show-ProjectStats {
    Write-NavStatus "Generating project statistics..." "Cyan"
    
    $stats = @{
        TotalFiles = 0
        CSharpFiles = 0
        JavaScriptFiles = 0 
        TypeScriptFiles = 0 
        ShaderFiles = 0     
        TextAssetFiles = 0  
        MetaFiles = 0
        TotalLinesCSharp = 0
        TotalSize = [long]0 # Ensure TotalSize is long for large projects
    }
    
    $files = Get-ChildItem -Path $AssetsPath -Recurse -File -ErrorAction SilentlyContinue
    
    if ($null -eq $files) {
        Write-Warning "No files found in Assets path: $AssetsPath"
        return
    }

    foreach ($file in $files) {
        $stats.TotalFiles++
        $stats.TotalSize += $file.Length
        
        switch ($file.Extension.ToLower()) {
            ".cs" { 
                $stats.CSharpFiles++
                try {
                    # Efficiently count lines without loading entire file into memory for large files
                    # Get-Content -ReadCount 0 returns an array of lines. .Length is the line count.
                    $lineCount = (Get-Content $file.FullName -ReadCount 0 -ErrorAction SilentlyContinue).Length 
                    $stats.TotalLinesCSharp += $lineCount
                } catch {
                    Write-Warning "Could not read lines from $($file.FullName): $($_.Exception.Message)"
                }
            }
            ".js" { $stats.JavaScriptFiles++ }
            ".ts" { $stats.TypeScriptFiles++ }
            ".shader" { $stats.ShaderFiles++ }
            ".hlsl" { $stats.ShaderFiles++ }
            ".cginc" { $stats.ShaderFiles++ }
            ".compute" { $stats.ShaderFiles++ }
            ".json" { $stats.TextAssetFiles++ }
            ".xml" { $stats.TextAssetFiles++ }
            ".txt" { $stats.TextAssetFiles++ }
            ".md" { $stats.TextAssetFiles++ }
            ".asmdef" { $stats.TextAssetFiles++ } # Unity Assembly Definition files
            ".meta" { $stats.MetaFiles++ }
        }
    }
    
    $totalSizeMB = if ($stats.TotalSize -gt 0) { [Math]::Round($stats.TotalSize / 1MB, 2) } else { 0 }
    
    Write-Host "Project Statistics:" -ForegroundColor Green
    Write-Host "  Total Files: $($stats.TotalFiles)" -ForegroundColor White
    Write-Host "  C# Files (.cs): $($stats.CSharpFiles)" -ForegroundColor White
    Write-Host "  JavaScript Files (.js): $($stats.JavaScriptFiles)" -ForegroundColor White
    Write-Host "  TypeScript Files (.ts): $($stats.TypeScriptFiles)" -ForegroundColor White
    Write-Host "  Shader Files (various): $($stats.ShaderFiles)" -ForegroundColor White
    Write-Host "  Text Asset Files (json, xml, etc.): $($stats.TextAssetFiles)" -ForegroundColor White
    Write-Host "  Unity Meta Files (.meta): $($stats.MetaFiles)" -ForegroundColor White
    Write-Host "  Total Lines of C# Code: $($stats.TotalLinesCSharp)" -ForegroundColor White
    Write-Host "  Total Project Size: $($totalSizeMB) MB" -ForegroundColor White
}

# --- Main Execution Logic ---

# Handle -Help explicitly or if no parameters are provided
if ($Help -or ($PSBoundParameters.Count -eq 0 -and (-not $PSBoundParameters.ContainsKey('Help')) ) ) {
    Write-Host @"
Unity Code Navigation Tool
=========================

Advanced code search and navigation utilities for Unity projects.

Parameters:
  -SearchTerm     : Search for specific term in code (e.g., "PlayerHealth")
  -FileType       : Filter by file type for some operations (e.g., "cs", "shader")
  -Classes        : Find all class definitions in .cs files
  -Methods        : Find all method definitions (default .cs, respects -FileType)
  -Properties     : Find all property definitions (default .cs, respects -FileType)
  -Large          : Find large files (default >100KB, see function for details)
  -Duplicates     : Find duplicate file names within the Assets folder
  -Dependencies   : Show basic namespace dependency analysis from 'using' statements in .cs files
  -Stats          : Show project statistics (file counts, sizes, lines of code)
  -Help           : Show this help message

Examples:
  .\code-nav.ps1 -SearchTerm "PlayerController"     # Search for PlayerController in various code files
  .\code-nav.ps1 -Classes                           # List all classes in .cs files
  .\code-nav.ps1 -Methods -FileType "cs"            # Find methods in C# files
  .\code-nav.ps1 -Large                             # Find large files
  .\code-nav.ps1 -Stats                             # Show project statistics
  .\code-nav.ps1                                    # Show this help message and project statistics

"@
    
    # If no parameters were provided at all, also show stats after help.
    if ($PSBoundParameters.Count -eq 0) {
        if (-not (Test-Path $AssetsPath)) {
            Write-NavStatus "Assets folder not found at '$AssetsPath'. Cannot show stats. Make sure you're in a Unity project root." "Red"
            # Do not exit here, help is already shown. Stats part is optional if path fails.
        } else {
            Show-ProjectStats
        }
    }
    exit 0 # Exit after help (and potentially stats) is shown.
}

# If we've reached here, it means -Help was not the primary command,
# and some other parameters were likely given.
# Critical check: Ensure AssetsPath is valid for all subsequent operations.
if (-not (Test-Path $AssetsPath)) {
    Write-NavStatus "Assets folder not found at '$AssetsPath'. This script must be run from a Unity project root directory." "Red"
    exit 1
}

# Dispatch to the correct function based on parameters
if ($SearchTerm) {
    Search-Code -Term $SearchTerm # Explicitly name parameter for clarity
} elseif ($Classes) {
    Find-Classes
} elseif ($Methods) {
    Find-Methods
} elseif ($Properties) {
    Find-Properties
} elseif ($Large) {
    Find-LargeFiles
} elseif ($Duplicates) {
    Find-Duplicates
} elseif ($Dependencies) {
    Show-Dependencies
} elseif ($Stats) { # Handles explicit call to -Stats
    Show-ProjectStats
} else {
    # This 'else' block is for cases where parameters were given,
    # but they didn't match any of the known command switches.
    # For example, if only '-FileType cs' was passed without a command like '-Methods'.
    Write-NavStatus "No specific command recognized, or an invalid parameter combination was used." "Yellow"
    Write-NavStatus "Use -Help for available options." "Yellow"
    # Optionally, show project stats as a fallback informative action.
    Show-ProjectStats
}
