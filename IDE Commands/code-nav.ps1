# Code Navigation and Search - Advanced code exploration tools
# Run: .\code-nav.ps1 [action]

param(
    [string]$Action = "help",
    [string]$Term = "",
    [string]$FileType = "cs"
)

$ProjectRoot = Split-Path -Parent $PSScriptRoot
Push-Location $ProjectRoot

function Show-Help {
    Write-Host "🧭 Code Navigation Tools" -ForegroundColor Cyan
    Write-Host "========================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "USAGE: .\code-nav.ps1 [action] [term] [filetype]" -ForegroundColor Green
    Write-Host ""
    Write-Host "🔍 SEARCH COMMANDS:" -ForegroundColor Yellow
    Write-Host "  class [name]     - Find class definitions"
    Write-Host "  method [name]    - Find method definitions"
    Write-Host "  usage [name]     - Find where something is used"
    Write-Host "  todo             - Find TODO comments"
    Write-Host "  fixme            - Find FIXME comments"
    Write-Host "  entity           - Entity-related code"
    Write-Host ""
    Write-Host "📁 FILE COMMANDS:" -ForegroundColor Yellow
    Write-Host "  recent           - Recently modified files"
    Write-Host "  large            - Find large files"
    Write-Host "  duplicates       - Find duplicate file names"
    Write-Host "  scripts          - List all script files"
    Write-Host ""
    Write-Host "🎯 ENTITY COMMANDS:" -ForegroundColor Yellow
    Write-Host "  entity-deps      - Find Entity dependencies"
    Write-Host "  behavior-tree    - Behavior tree related code"
    Write-Host "  stats            - Stats and health related code"
    Write-Host ""
}

function Find-Class {
    param([string]$ClassName)
    if (-not $ClassName) {
        $ClassName = Read-Host "Enter class name"
    }
    
    Write-Host "🔍 Searching for class '$ClassName'..." -ForegroundColor Cyan
    Get-ChildItem -Recurse -Include "*.cs" | Select-String -Pattern "class\s+$ClassName\b|struct\s+$ClassName\b|interface\s+$ClassName\b" |
        ForEach-Object {
            Write-Host "$($_.Filename):$($_.LineNumber)" -ForegroundColor Green -NoNewline
            Write-Host " - $($_.Line.Trim())" -ForegroundColor White
        }
}

function Find-Method {
    param([string]$MethodName)
    if (-not $MethodName) {
        $MethodName = Read-Host "Enter method name"
    }
    
    Write-Host "🔍 Searching for method '$MethodName'..." -ForegroundColor Cyan
    Get-ChildItem -Recurse -Include "*.cs" | Select-String -Pattern "\s+\w+\s+$MethodName\s*\(|\s+$MethodName\s*\(" |
        Select-Object -First 15 | ForEach-Object {
            Write-Host "$($_.Filename):$($_.LineNumber)" -ForegroundColor Green -NoNewline
            Write-Host " - $($_.Line.Trim())" -ForegroundColor White
        }
}

function Find-Usage {
    param([string]$Term)
    if (-not $Term) {
        $Term = Read-Host "Enter term to find usage"
    }
    
    Write-Host "🔍 Finding usage of '$Term'..." -ForegroundColor Cyan
    Get-ChildItem -Recurse -Include "*.cs" | Select-String -Pattern "\b$Term\b" |
        Select-Object -First 20 | ForEach-Object {
            Write-Host "$($_.Filename):$($_.LineNumber)" -ForegroundColor Green -NoNewline
            Write-Host " - $($_.Line.Trim())" -ForegroundColor White
        }
}

function Find-TODOs {
    Write-Host "📝 Finding TODO comments..." -ForegroundColor Cyan
    Get-ChildItem -Recurse -Include "*.cs" | Select-String -Pattern "//\s*TODO|//\s*HACK|//\s*TEMP" |
        ForEach-Object {
            Write-Host "$($_.Filename):$($_.LineNumber)" -ForegroundColor Yellow -NoNewline
            Write-Host " - $($_.Line.Trim())" -ForegroundColor White
        }
}

function Find-FIXMEs {
    Write-Host "🔧 Finding FIXME comments..." -ForegroundColor Cyan
    Get-ChildItem -Recurse -Include "*.cs" | Select-String -Pattern "//\s*FIXME|//\s*BUG|//\s*FIX" |
        ForEach-Object {
            Write-Host "$($_.Filename):$($_.LineNumber)" -ForegroundColor Red -NoNewline
            Write-Host " - $($_.Line.Trim())" -ForegroundColor White
        }
}

function Show-RecentFiles {
    Write-Host "📅 Recently modified files (last 7 days)..." -ForegroundColor Cyan
    Get-ChildItem -Recurse -Include "*.cs", "*.md", "*.json" | 
        Where-Object { $_.LastWriteTime -gt (Get-Date).AddDays(-7) } |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 15 |
        ForEach-Object {
            $relPath = $_.FullName.Replace($ProjectRoot, "").TrimStart("\")
            Write-Host "$($_.LastWriteTime.ToString('MM/dd HH:mm'))" -ForegroundColor Green -NoNewline
            Write-Host " - $relPath" -ForegroundColor White
        }
}

function Find-LargeFiles {
    Write-Host "📊 Finding large files (>1MB)..." -ForegroundColor Cyan
    Get-ChildItem -Recurse | 
        Where-Object { $_.Length -gt 1MB } |
        Sort-Object Length -Descending |
        Select-Object -First 10 |
        ForEach-Object {
            $sizeMB = [math]::Round($_.Length / 1MB, 2)
            $relPath = $_.FullName.Replace($ProjectRoot, "").TrimStart("\")
            Write-Host "$sizeMB MB" -ForegroundColor Yellow -NoNewline
            Write-Host " - $relPath" -ForegroundColor White
        }
}

function Find-Duplicates {
    Write-Host "👥 Finding duplicate file names..." -ForegroundColor Cyan
    Get-ChildItem -Recurse -File |
        Group-Object Name |
        Where-Object { $_.Count -gt 1 } |
        ForEach-Object {
            Write-Host "Duplicate: $($_.Name)" -ForegroundColor Red
            $_.Group | ForEach-Object {
                $relPath = $_.FullName.Replace($ProjectRoot, "").TrimStart("\")
                Write-Host "  $relPath" -ForegroundColor Yellow
            }
        }
}

function List-Scripts {
    Write-Host "📜 All script files..." -ForegroundColor Cyan
    $scripts = Get-ChildItem -Recurse -Include "*.cs" | Sort-Object Directory, Name
    
    $currentDir = ""
    foreach ($script in $scripts) {
        $dir = $script.Directory.Name
        if ($dir -ne $currentDir) {
            Write-Host "`n📁 $dir/" -ForegroundColor Green
            $currentDir = $dir
        }
        Write-Host "  $($script.Name)" -ForegroundColor White
    }
    
    Write-Host "`n📊 Total: $($scripts.Count) script files" -ForegroundColor Cyan
}

function Find-EntityCode {
    Write-Host "🎯 Entity-related code..." -ForegroundColor Cyan
    $patterns = @("Entity", "EntityType", "EntityData", "Health", "Stat")
    
    foreach ($pattern in $patterns) {
        Write-Host "`n🔍 $pattern related:" -ForegroundColor Yellow
        Get-ChildItem -Recurse -Include "*.cs" | Select-String -Pattern $pattern |
            Select-Object -First 5 | ForEach-Object {
                Write-Host "  $($_.Filename):$($_.LineNumber)" -ForegroundColor Green -NoNewline
                Write-Host " - $($_.Line.Trim())" -ForegroundColor White
            }
    }
}

function Find-EntityDependencies {
    Write-Host "🔗 Entity dependencies..." -ForegroundColor Cyan
    
    $entityFile = "Assets\Scripts\Entities\Entity.cs"
    if (Test-Path $entityFile) {
        Write-Host "`n📄 Entity.cs imports:" -ForegroundColor Yellow
        Get-Content $entityFile | Select-String -Pattern "using\s+" | ForEach-Object {
            Write-Host "  $($_.Line.Trim())" -ForegroundColor White
        }
        
        Write-Host "`n🔍 Files that reference Entity:" -ForegroundColor Yellow
        Get-ChildItem -Recurse -Include "*.cs" | Select-String -Pattern "\bEntity\b" |
            Where-Object { -not $_.Filename.Contains("Entity") } |
            Select-Object -First 10 | ForEach-Object {
                Write-Host "  $($_.Filename)" -ForegroundColor White
            }
    }
}

function Find-BehaviorTree {
    Write-Host "🌳 Behavior tree related code..." -ForegroundColor Cyan
    $patterns = @("BehaviorTree", "Blackboard", "BehaviorDesigner", "Behavior")
    
    foreach ($pattern in $patterns) {
        $results = Get-ChildItem -Recurse -Include "*.cs" | Select-String -Pattern $pattern
        if ($results) {
            Write-Host "`n🔍 $pattern:" -ForegroundColor Yellow
            $results | Select-Object -First 3 | ForEach-Object {
                Write-Host "  $($_.Filename):$($_.LineNumber)" -ForegroundColor Green
            }
        }
    }
}

function Find-Stats {
    Write-Host "📊 Stats and health related code..." -ForegroundColor Cyan
    $patterns = @("Health", "Stat", "StatType", "Hunger", "Thirst", "Stamina")
    
    foreach ($pattern in $patterns) {
        $results = Get-ChildItem -Recurse -Include "*.cs" | Select-String -Pattern "\b$pattern\b"
        if ($results) {
            Write-Host "`n🔍 $pattern (found $($results.Count)):" -ForegroundColor Yellow
            $results | Select-Object -First 3 | ForEach-Object {
                Write-Host "  $($_.Filename):$($_.LineNumber)" -ForegroundColor Green -NoNewline
                Write-Host " - $($_.Line.Trim())" -ForegroundColor White
            }
        }
    }
}

# Main command dispatcher
switch ($Action.ToLower()) {
    "class" { Find-Class $Term }
    "method" { Find-Method $Term }
    "usage" { Find-Usage $Term }
    "todo" { Find-TODOs }
    "fixme" { Find-FIXMEs }
    "entity" { Find-EntityCode }
    "recent" { Show-RecentFiles }
    "large" { Find-LargeFiles }
    "duplicates" { Find-Duplicates }
    "scripts" { List-Scripts }
    "entity-deps" { Find-EntityDependencies }
    "behavior-tree" { Find-BehaviorTree }
    "stats" { Find-Stats }
    default { Show-Help }
}

Pop-Location
