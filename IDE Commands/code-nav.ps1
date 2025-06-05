# Code Navigation and Search - Advanced code exploration tools
# Run: .\code-nav.ps1 [action]

param(
    [string]$Action = "help",
    [string]$Term = "",
    [string]$FileType = "cs"
)

# Set the project root to the parent directory of the script's location
# This assumes the script is in a subdirectory of the project (e.g., a 'tools' folder)
# If the script is in the project root, $PSScriptRoot itself would be the ProjectRoot.
# Adjust if your project structure is different.
$ProjectRoot = Split-Path -Parent $PSScriptRoot
if (-not $ProjectRoot -or (-not (Test-Path $ProjectRoot))) {
    Write-Warning "Could not determine a valid project root from '$PSScriptRoot'. Using current location as project root."
    $ProjectRoot = Get-Location
}
Push-Location $ProjectRoot # Change current directory to project root

function Show-Help {
    Write-Host "🧭 Code Navigation Tools" -ForegroundColor Cyan
    Write-Host "========================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "USAGE: .\code-nav.ps1 [action] [term] [filetype]" -ForegroundColor Green
    Write-Host "  Example: .\code-nav.ps1 class PlayerController"
    Write-Host "  Example: .\code-nav.ps1 usage SpawnEnemy cs"
    Write-Host ""
    Write-Host "🔍 SEARCH COMMANDS:" -ForegroundColor Yellow
    Write-Host "  class [name]      - Find class, struct, or interface definitions (default FileType: cs)"
    Write-Host "  method [name]     - Find method definitions (default FileType: cs)"
    Write-Host "  usage [name]      - Find where a term is used (default FileType: cs)"
    Write-Host "  todo              - Find TODO, HACK, TEMP comments (default FileType: cs)"
    Write-Host "  fixme             - Find FIXME, BUG, FIX comments (default FileType: cs)"
    # Removed 'entity' from here as it's covered by Find-EntityCode and is more specific
    Write-Host ""
    Write-Host "📁 FILE COMMANDS:" -ForegroundColor Yellow
    Write-Host "  recent            - Recently modified files (cs, md, json - last 7 days)"
    Write-Host "  large             - Find large files (default >1MB, any type)"
    Write-Host "  duplicates        - Find duplicate file names (any type)"
    Write-Host "  scripts           - List all script files (default FileType: cs)"
    Write-Host ""
    Write-Host "🎯 DOMAIN-SPECIFIC COMMANDS (Example: Unity Entities):" -ForegroundColor Yellow
    Write-Host "  entity            - General entity-related code search (default FileType: cs)"
    Write-Host "  entity-deps       - Find Entity.cs dependencies and references (default FileType: cs)"
    Write-Host "  behavior-tree   - Behavior tree related code (default FileType: cs)"
    Write-Host "  stats-health    - Stats and health related code (default FileType: cs)"
    Write-Host ""
    Write-Host "Default FileType for most searches is '$FileType'."
    Write-Host "To search all files for 'usage', 'large', 'duplicates', pass an empty string or '*' for FileType if the command supports it,"
    Write-Host "or modify the command's Get-ChildItem -Include filter."
    Write-Host ""
}

# Helper function to get file filter based on $FileType
function Get-FileFilter {
    param([string]$DefaultExtension = "cs")
    if ($FileType -eq "*" -or [string]::IsNullOrEmpty($FileType)) {
        return "*.*" # Search all files if FileType is "*" or empty
    } elseif ($FileType.StartsWith("*.")) {
        return $FileType
    } else {
        return "*.$($FileType.TrimStart('.'))" # Ensure it's a wildcard pattern like "*.cs"
    }
}

function Find-Class {
    param([string]$ClassName)
    if (-not $ClassName) {
        $ClassName = Read-Host "Enter class, struct, or interface name"
        if (-not $ClassName) { Write-Warning "Class name cannot be empty."; return }
    }
    
    $currentFilter = Get-FileFilter -DefaultExtension "cs"
    Write-Host "🔍 Searching for class/struct/interface '$ClassName' in '$currentFilter' files..." -ForegroundColor Cyan
    Get-ChildItem -Path $ProjectRoot -Recurse -Include $currentFilter -File -ErrorAction SilentlyContinue | 
        Select-String -Pattern "(?i)(class|struct|interface)\s+$([regex]::Escape($ClassName))\b" |
        ForEach-Object {
            $relativePath = $_.Path.Replace($ProjectRoot, "").TrimStart("\/")
            Write-Host "$relativePath`:$($_.LineNumber)" -ForegroundColor Green -NoNewline
            Write-Host " - $($_.Line.Trim())" -ForegroundColor White
        }
}

function Find-Method {
    param([string]$MethodName)
    if (-not $MethodName) {
        $MethodName = Read-Host "Enter method name"
        if (-not $MethodName) { Write-Warning "Method name cannot be empty."; return }
    }
    
    $currentFilter = Get-FileFilter -DefaultExtension "cs"
    Write-Host "🔍 Searching for method '$MethodName' in '$currentFilter' files..." -ForegroundColor Cyan
    # Regex: (public/private/etc.) (static/async/etc.) ReturnType MethodName ( or MethodName (
    # This is a simplified regex and might catch non-method lines or miss complex signatures.
    $pattern = "(?i)(?:(?:public|private|protected|internal)\s+)?(?:(?:static|async|virtual|override|abstract)\s+)?\w+(?:\[\])?\s+$([regex]::Escape($MethodName))\s*\(|^\s*$([regex]::Escape($MethodName))\s*\("
    Get-ChildItem -Path $ProjectRoot -Recurse -Include $currentFilter -File -ErrorAction SilentlyContinue | 
        Select-String -Pattern $pattern |
        Select-Object -First 15 | ForEach-Object {
            $relativePath = $_.Path.Replace($ProjectRoot, "").TrimStart("\/")
            Write-Host "$relativePath`:$($_.LineNumber)" -ForegroundColor Green -NoNewline
            Write-Host " - $($_.Line.Trim())" -ForegroundColor White
        }
}

function Find-Usage {
    param([string]$SearchTermParam) # Renamed to avoid conflict with outer $Term
    if (-not $SearchTermParam) {
        $SearchTermParam = Read-Host "Enter term to find usage for"
        if (-not $SearchTermParam) { Write-Warning "Search term cannot be empty."; return }
    }
    
    $currentFilter = Get-FileFilter -DefaultExtension "cs" # Default to cs, but can be overridden by global $FileType
    Write-Host "🔍 Finding usage of '$SearchTermParam' in '$currentFilter' files..." -ForegroundColor Cyan
    Get-ChildItem -Path $ProjectRoot -Recurse -Include $currentFilter -File -ErrorAction SilentlyContinue | 
        Select-String -Pattern "\b$([regex]::Escape($SearchTermParam))\b" -CaseSensitive:$false | # Case-insensitive search
        Select-Object -First 20 | ForEach-Object {
            $relativePath = $_.Path.Replace($ProjectRoot, "").TrimStart("\/")
            Write-Host "$relativePath`:$($_.LineNumber)" -ForegroundColor Green -NoNewline
            Write-Host " - $($_.Line.Trim())" -ForegroundColor White
        }
}

function Find-TODOs {
    $currentFilter = Get-FileFilter -DefaultExtension "cs"
    Write-Host "📝 Finding TODO/HACK/TEMP comments in '$currentFilter' files..." -ForegroundColor Cyan
    Get-ChildItem -Path $ProjectRoot -Recurse -Include $currentFilter -File -ErrorAction SilentlyContinue | 
        Select-String -Pattern "(?i)//\s*(TODO|HACK|TEMP)" | # Case-insensitive
        ForEach-Object {
            $relativePath = $_.Path.Replace($ProjectRoot, "").TrimStart("\/")
            Write-Host "$relativePath`:$($_.LineNumber)" -ForegroundColor Yellow -NoNewline
            Write-Host " - $($_.Line.Trim())" -ForegroundColor White
        }
}

function Find-FIXMEs {
    $currentFilter = Get-FileFilter -DefaultExtension "cs"
    Write-Host "🔧 Finding FIXME/BUG/FIX comments in '$currentFilter' files..." -ForegroundColor Cyan
    Get-ChildItem -Path $ProjectRoot -Recurse -Include $currentFilter -File -ErrorAction SilentlyContinue | 
        Select-String -Pattern "(?i)//\s*(FIXME|BUG|FIX)" | # Case-insensitive
        ForEach-Object {
            $relativePath = $_.Path.Replace($ProjectRoot, "").TrimStart("\/")
            Write-Host "$relativePath`:$($_.LineNumber)" -ForegroundColor Red -NoNewline
            Write-Host " - $($_.Line.Trim())" -ForegroundColor White
        }
}

function Show-RecentFiles {
    Write-Host "📅 Recently modified files (last 7 days)..." -ForegroundColor Cyan
    # FileType parameter is not directly used here, includes common text/code files by default
    Get-ChildItem -Path $ProjectRoot -Recurse -Include "*.cs", "*.js", "*.ts", "*.shader", "*.hlsl", "*.json", "*.xml", "*.txt", "*.md", "*.asmdef" -File -ErrorAction SilentlyContinue | 
        Where-Object { $_.LastWriteTime -gt (Get-Date).AddDays(-7) } |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 15 |
        ForEach-Object {
            $relPath = $_.FullName.Replace($ProjectRoot, "").TrimStart("\/")
            Write-Host "$($_.LastWriteTime.ToString('yyyy-MM-dd HH:mm'))" -ForegroundColor Green -NoNewline
            Write-Host " - $relPath ($([Math]::Round($_.Length/1KB,1)) KB)" -ForegroundColor White
        }
}

function Find-LargeFiles {
    Write-Host "📊 Finding large files (>1MB)..." -ForegroundColor Cyan
    # FileType parameter is not used here, searches all files by default
    Get-ChildItem -Path $ProjectRoot -Recurse -File -ErrorAction SilentlyContinue | 
        Where-Object { $_.PSIsContainer -eq $false -and $_.Length -gt 1MB } | # Ensure it's a file
        Sort-Object Length -Descending |
        Select-Object -First 10 |
        ForEach-Object {
            $sizeMB = [math]::Round($_.Length / 1MB, 2)
            $relPath = $_.FullName.Replace($ProjectRoot, "").TrimStart("\/")
            Write-Host "$($sizeMB) MB" -ForegroundColor Yellow -NoNewline
            Write-Host " - $relPath" -ForegroundColor White
        }
}

function Find-Duplicates {
    Write-Host "👥 Finding duplicate file names..." -ForegroundColor Cyan
    # FileType parameter is not used here, searches all files by default
    Get-ChildItem -Path $ProjectRoot -Recurse -File -ErrorAction SilentlyContinue |
        Group-Object Name |
        Where-Object { $_.Count -gt 1 } |
        ForEach-Object {
            Write-Host "Duplicate: $($_.Name) ($($_.Count) occurrences)" -ForegroundColor Red
            $_.Group | ForEach-Object {
                $relPath = $_.FullName.Replace($ProjectRoot, "").TrimStart("\/")
                Write-Host "  $relPath" -ForegroundColor Yellow
            }
        }
}

function List-Scripts {
    $currentFilter = Get-FileFilter -DefaultExtension "cs"
    Write-Host "📜 Listing all '$currentFilter' script files..." -ForegroundColor Cyan
    $scripts = Get-ChildItem -Path $ProjectRoot -Recurse -Include $currentFilter -File -ErrorAction SilentlyContinue | Sort-Object DirectoryName, Name
    
    if (-not $scripts) {
        Write-Host "No '$currentFilter' files found in $ProjectRoot." -ForegroundColor Yellow
        return
    }

    $currentDir = ""
    foreach ($script in $scripts) {
        $dirRelativePath = $script.DirectoryName.Replace($ProjectRoot, "").TrimStart("\/")
        if ($dirRelativePath -ne $currentDir) {
            Write-Host "`n📁 $dirRelativePath/" -ForegroundColor Green
            $currentDir = $dirRelativePath
        }
        Write-Host "  $($script.Name)" -ForegroundColor White
    }
    
    Write-Host "`n📊 Total: $($scripts.Count) '$currentFilter' files" -ForegroundColor Cyan
}

function Find-EntityCode {
    $currentFilter = Get-FileFilter -DefaultExtension "cs"
    Write-Host "🎯 Entity-related code search in '$currentFilter' files..." -ForegroundColor Cyan
    $patterns = @("Entity", "EntityType", "EntityData", "Health", "Stat", "ComponentData", "IComponentData", "ISystem", "SystemBase", "Aspect", "EnableableAspect", "Authoring") # Common Unity ECS/DOTS terms
    
    foreach ($pattern in $patterns) {
        Write-Host "`n🔍 Searching for '$pattern' related terms:" -ForegroundColor Yellow
        Get-ChildItem -Path $ProjectRoot -Recurse -Include $currentFilter -File -ErrorAction SilentlyContinue | 
            Select-String -Pattern "\b$([regex]::Escape($pattern))\b" -CaseSensitive:$false |
            Select-Object -First 5 | ForEach-Object {
                $relativePath = $_.Path.Replace($ProjectRoot, "").TrimStart("\/")
                Write-Host "  $relativePath`:$($_.LineNumber)" -ForegroundColor Green -NoNewline
                Write-Host " - $($_.Line.Trim())" -ForegroundColor White
            }
    }
}

function Find-EntityDependencies {
    $currentFilter = Get-FileFilter -DefaultExtension "cs"
    Write-Host "🔗 Entity dependencies analysis in '$currentFilter' files..." -ForegroundColor Cyan
    
    # This is a very specific example; adapt to your project's main "Entity" file or concept
    $mainEntityConcept = $Term # Use the provided term as the central concept if specified
    if (-not $mainEntityConcept) {
        $mainEntityConcept = "Entity" # Default concept
        Write-Host "No specific entity term provided, using '$mainEntityConcept' as a general concept." -ForegroundColor DarkYellow
    }

    Write-Host "`n🔍 Files that reference '$mainEntityConcept':" -ForegroundColor Yellow
    Get-ChildItem -Path $ProjectRoot -Recurse -Include $currentFilter -File -ErrorAction SilentlyContinue | 
        Select-String -Pattern "\b$([regex]::Escape($mainEntityConcept))\b" -CaseSensitive:$false |
        # Optional: Exclude files that define the concept itself if it's too noisy
        # Where-Object { $_.Path -notmatch "[/\\]$([regex]::Escape($mainEntityConcept))\.(cs|ts|js)$" } |
        Group-Object Path | # Group by file path to list each file once
        Select-Object -First 10 | ForEach-Object {
            $relativePath = $_.Name.Replace($ProjectRoot, "").TrimStart("\/") # $_.Name here is the Path
            Write-Host "  $relativePath (references found: $($_.Group.Count))" -ForegroundColor White
        }
}

function Find-BehaviorTree {
    $currentFilter = Get-FileFilter -DefaultExtension "cs"
    Write-Host "🌳 Behavior tree related code search in '$currentFilter' files..." -ForegroundColor Cyan
    $patterns = @("BehaviorTree", "Blackboard", "BehaviorDesigner", "NodeCanvas", "BTNode", "Task", "Condition", "ActionNode") # Added NodeCanvas and common BT terms
    
    foreach ($pattern in $patterns) {
        $results = Get-ChildItem -Path $ProjectRoot -Recurse -Include $currentFilter -File -ErrorAction SilentlyContinue | 
            Select-String -Pattern "\b$([regex]::Escape($pattern))\b" -CaseSensitive:$false
        if ($results) {
            Write-Host "`n🔍 '$pattern' (found in $($results.Group | Select-Object -ExpandProperty Path -Unique).Count files):" -ForegroundColor Yellow
            # Show unique files first, then details for a few
            $results | Select-Object -First 5 | ForEach-Object {
                $relativePath = $_.Path.Replace($ProjectRoot, "").TrimStart("\/")
                Write-Host "  $relativePath`:$($_.LineNumber) - $($_.Line.Trim())" -ForegroundColor Green
            }
        } else {
            Write-Host "`n🔍 No results for '$pattern'." -ForegroundColor DarkGray
        }
    }
}

function Find-StatsHealthCode { # Renamed to avoid conflict with global $Stats parameter
    $currentFilter = Get-FileFilter -DefaultExtension "cs"
    Write-Host "📊 Stats and health related code search in '$currentFilter' files..." -ForegroundColor Cyan
    $patterns = @("Health", "Stat", "Attribute", "Damage", "Healing", "Mana", "Stamina", "Hunger", "Thirst", "XP", "Level")
    
    foreach ($pattern in $patterns) {
        $results = Get-ChildItem -Path $ProjectRoot -Recurse -Include $currentFilter -File -ErrorAction SilentlyContinue | 
            Select-String -Pattern "\b$([regex]::Escape($pattern))\b" -CaseSensitive:$false
        if ($results) {
            Write-Host "`n🔍 '$pattern' (found in $($results.Group | Select-Object -ExpandProperty Path -Unique).Count files, showing up to 3 matches):" -ForegroundColor Yellow
            $results | Select-Object -First 3 | ForEach-Object {
                $relativePath = $_.Path.Replace($ProjectRoot, "").TrimStart("\/")
                Write-Host "  $relativePath`:$($_.LineNumber)" -ForegroundColor Green -NoNewline
                Write-Host " - $($_.Line.Trim())" -ForegroundColor White
            }
        } else {
            Write-Host "`n🔍 No results for '$pattern'." -ForegroundColor DarkGray
        }
    }
}

# --- Main command dispatcher ---
try {
    switch ($Action.ToLower()) {
        "class"         { Find-Class $Term }
        "method"        { Find-Method $Term }
        "usage"         { Find-Usage $Term }
        "todo"          { Find-TODOs }
        "fixme"         { Find-FIXMEs }
        "entity"        { Find-EntityCode } # General entity keyword search
        "recent"        { Show-RecentFiles }
        "large"         { Find-LargeFiles }
        "duplicates"    { Find-Duplicates }
        "scripts"       { List-Scripts }
        "entity-deps"   { Find-EntityDependencies } # More specific dependency idea
        "behavior-tree" { Find-BehaviorTree }
        "stats-health"  { Find-StatsHealthCode } # Renamed from "stats"
        "help"          { Show-Help }
        default         { 
            Write-Warning "Unknown action: '$Action'"
            Show-Help 
        }
    }
}
finally {
    Pop-Location # Always restore original location
}

