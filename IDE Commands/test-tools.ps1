# Testing and Validation Tools - Automated testing and code validation
# Run: .\test-tools.ps1 [action]

param(
    [string]$Action = "help",
    [string]$Target = ""
)

$ProjectRoot = Split-Path -Parent $PSScriptRoot
Push-Location $ProjectRoot

function Show-Help {
    Write-Host "🧪 Testing and Validation Tools" -ForegroundColor Cyan
    Write-Host "===============================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "USAGE: .\test-tools.ps1 [action] [target]" -ForegroundColor Green
    Write-Host ""
    Write-Host "🔍 VALIDATION COMMANDS:" -ForegroundColor Yellow
    Write-Host "  compile          - Check compilation status"
    Write-Host "  syntax           - Basic syntax validation"
    Write-Host "  missing-refs     - Find missing references"
    Write-Host "  unused           - Find unused using statements"
    Write-Host ""
    Write-Host "🎯 ENTITY TESTING:" -ForegroundColor Yellow
    Write-Host "  entity-validate  - Validate Entity system"
    Write-Host "  entity-stats     - Test stat system"
    Write-Host "  behavior-check   - Check behavior tree setup"
    Write-Host ""
    Write-Host "📊 CODE ANALYSIS:" -ForegroundColor Yellow
    Write-Host "  complexity       - Analyze code complexity"
    Write-Host "  dependencies     - Check dependencies"
    Write-Host "  metrics          - Code metrics report"
    Write-Host ""
    Write-Host "🛠️ UNITY SPECIFIC:" -ForegroundColor Yellow
    Write-Host "  meta-check       - Validate .meta files"
    Write-Host "  prefab-check     - Check prefab integrity"
    Write-Host "  scene-validate   - Validate scene files"
    Write-Host ""
}

function Check-Compilation {
    Write-Host "🔨 Checking compilation status..." -ForegroundColor Cyan
    Write-Host "==================================" -ForegroundColor Cyan
    
    # Check if Unity is running
    $unityProcess = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
    if ($unityProcess) {
        Write-Host "🎮 Unity Editor: Running" -ForegroundColor Green
    } else {
        Write-Host "🎮 Unity Editor: Not running" -ForegroundColor Yellow
    }
    
    # Try to compile C# projects
    Write-Host "`n📄 Assembly-CSharp.csproj:" -ForegroundColor Yellow
    if (Test-Path "Assembly-CSharp.csproj") {
        $result = dotnet build "Assembly-CSharp.csproj" --verbosity quiet --nologo 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✅ Compilation successful" -ForegroundColor Green
        } else {
            Write-Host "  ❌ Compilation failed" -ForegroundColor Red
            Write-Host "  Errors:" -ForegroundColor Red
            $result | Select-Object -First 10 | ForEach-Object {
                Write-Host "    $_" -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "  ⚠️ Project file not found" -ForegroundColor Yellow
    }
    
    # Check recent error logs
    Write-Host "`n📝 Recent Unity logs:" -ForegroundColor Yellow
    $logPath = "$env:USERPROFILE\AppData\Local\Unity\Editor\Editor.log"
    if (Test-Path $logPath) {
        $recentErrors = Get-Content $logPath | Select-String -Pattern "(error|Error|ERROR)" | Select-Object -Last 5
        if ($recentErrors) {
            Write-Host "  ⚠️ Recent errors found:" -ForegroundColor Red
            $recentErrors | ForEach-Object { Write-Host "    $($_.Line)" -ForegroundColor Yellow }
        } else {
            Write-Host "  ✅ No recent errors in Unity log" -ForegroundColor Green
        }
    }
}

function Check-Syntax {
    Write-Host "📝 Basic syntax validation..." -ForegroundColor Cyan
    Write-Host "=============================" -ForegroundColor Cyan
    
    $issues = @()
    
    Get-ChildItem -Recurse -Include "*.cs" | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        $fileName = $_.Name
        
        # Check for common syntax issues
        if ($content -match ";\s*;") {
            $issues += "$fileName`: Double semicolons found"
        }
        
        if ($content -match "\{\s*\}") {
            # Empty blocks - might be intentional, so just note
        }
        
        # Check for unmatched braces (basic check)
        $openBraces = ($content.ToCharArray() | Where-Object { $_ -eq '{' }).Count
        $closeBraces = ($content.ToCharArray() | Where-Object { $_ -eq '}' }).Count
        if ($openBraces -ne $closeBraces) {
            $issues += "$fileName`: Mismatched braces (Open: $openBraces, Close: $closeBraces)"
        }
        
        # Check for TODO/FIXME comments
        if ($content -match "//\s*(TODO|FIXME|HACK)") {
            $todoCount = ([regex]::Matches($content, "//\s*(TODO|FIXME|HACK)")).Count
            $issues += "$fileName`: $todoCount TODO/FIXME comments"
        }
    }
    
    if ($issues) {
        Write-Host "⚠️ Issues found:" -ForegroundColor Yellow
        $issues | ForEach-Object { Write-Host "  $_" -ForegroundColor White }
    } else {
        Write-Host "✅ No obvious syntax issues found" -ForegroundColor Green
    }
}

function Find-MissingReferences {
    Write-Host "🔗 Checking for missing references..." -ForegroundColor Cyan
    Write-Host "=====================================" -ForegroundColor Cyan
    
    $missingRefs = @()
    
    Get-ChildItem -Recurse -Include "*.cs" | ForEach-Object {
        $content = Get-Content $_.FullName
        $fileName = $_.Name
        
        # Look for using statements that might not exist
        $usingStatements = $content | Select-String -Pattern "using\s+[\w\.]+;" | ForEach-Object { $_.Line.Trim() }
        
        foreach ($using in $usingStatements) {
            # Check for common typos or missing namespaces
            if ($using -match "using\s+UnityEngine\.UI;" -and -not (Test-Path "Packages\com.unity.ugui")) {
                $missingRefs += "$fileName`: UI namespace used but package may not be installed"
            }
        }
        
        # Check for common Unity component references
        $unityRefs = $content | Select-String -Pattern "\b(Rigidbody|Collider|Renderer|Camera|Light|AudioSource)\b"
        if ($unityRefs -and -not ($content | Select-String -Pattern "using UnityEngine")) {
            $missingRefs += "$fileName`: Unity components used without UnityEngine using statement"
        }
    }
    
    if ($missingRefs) {
        Write-Host "⚠️ Potential missing references:" -ForegroundColor Yellow
        $missingRefs | ForEach-Object { Write-Host "  $_" -ForegroundColor White }
    } else {
        Write-Host "✅ No obvious missing references found" -ForegroundColor Green
    }
}

function Find-UnusedUsings {
    Write-Host "🗑️ Finding unused using statements..." -ForegroundColor Cyan
    Write-Host "=====================================" -ForegroundColor Cyan
    
    $unusedUsings = @()
    
    Get-ChildItem -Recurse -Include "*.cs" | Select-Object -First 10 | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        $fileName = $_.Name
        
        $usingStatements = Get-Content $_.FullName | Select-String -Pattern "^using\s+[\w\.]+;" | ForEach-Object { $_.Line.Trim() }
        
        foreach ($using in $usingStatements) {
            if ($using -match "using\s+([\w\.]+);") {
                $namespace = $matches[1]
                $shortName = $namespace.Split('.')[-1]
                
                # Simple check - if namespace isn't referenced in code
                if ($content -notmatch "\b$shortName\b" -and $namespace -ne "System") {
                    $unusedUsings += "$fileName`: $using (possibly unused)"
                }
            }
        }
    }
    
    if ($unusedUsings) {
        Write-Host "⚠️ Potentially unused using statements:" -ForegroundColor Yellow
        $unusedUsings | Select-Object -First 15 | ForEach-Object { Write-Host "  $_" -ForegroundColor White }
        if ($unusedUsings.Count -gt 15) {
            Write-Host "  ... and $($unusedUsings.Count - 15) more" -ForegroundColor Gray
        }
    } else {
        Write-Host "✅ No obviously unused using statements found" -ForegroundColor Green
    }
}

function Validate-EntitySystem {
    Write-Host "🎯 Validating Entity system..." -ForegroundColor Cyan
    Write-Host "==============================" -ForegroundColor Cyan
    
    $entityFile = "Assets\Scripts\Entities\Entity.cs"
    $entityDataFile = "Assets\Scripts\Entities\EntityTypeData.cs"
    
    # Check if core files exist
    if (Test-Path $entityFile) {
        Write-Host "✅ Entity.cs found" -ForegroundColor Green
        
        $entityContent = Get-Content $entityFile -Raw
        
        # Check for key methods
        $keyMethods = @("GetStat", "ModifyStat", "Initialize", "UpdateNeeds")
        foreach ($method in $keyMethods) {
            if ($entityContent -match "\b$method\b") {
                Write-Host "  ✅ $method method found" -ForegroundColor Green
            } else {
                Write-Host "  ❌ $method method missing" -ForegroundColor Red
            }
        }
    } else {
        Write-Host "❌ Entity.cs not found!" -ForegroundColor Red
    }
    
    if (Test-Path $entityDataFile) {
        Write-Host "✅ EntityTypeData.cs found" -ForegroundColor Green
    } else {
        Write-Host "❌ EntityTypeData.cs not found!" -ForegroundColor Red
    }
    
    # Check for ScriptableObject assets
    $entityAssets = Get-ChildItem -Recurse -Include "*.asset" | Where-Object { 
        $content = Get-Content $_.FullName -Raw -ErrorAction SilentlyContinue
        $content -and $content -match "EntityTypeData"
    }
    
    Write-Host "📊 EntityTypeData assets found: $($entityAssets.Count)" -ForegroundColor Cyan
}

function Test-StatSystem {
    Write-Host "📊 Testing stat system..." -ForegroundColor Cyan
    Write-Host "==========================" -ForegroundColor Cyan
    
    # Find stat-related code
    $statFiles = Get-ChildItem -Recurse -Include "*.cs" | Where-Object {
        (Get-Content $_.FullName -Raw) -match "\b(Stat|Health|StatType)\b"
    }
    
    Write-Host "📄 Files with stat system code: $($statFiles.Count)" -ForegroundColor White
    $statFiles | Select-Object -First 5 | ForEach-Object {
        $relPath = $_.FullName.Replace($ProjectRoot, "").TrimStart("\")
        Write-Host "  $relPath" -ForegroundColor Gray
    }
    
    # Check for stat types
    $statTypes = @("Health", "Hunger", "Thirst", "Stamina", "Energy")
    foreach ($statType in $statTypes) {
        $references = Get-ChildItem -Recurse -Include "*.cs" | Select-String -Pattern "\b$statType\b" | Measure-Object
        Write-Host "🔍 $statType references: $($references.Count)" -ForegroundColor White
    }
}

function Check-BehaviorTrees {
    Write-Host "🌳 Checking behavior tree setup..." -ForegroundColor Cyan
    Write-Host "===================================" -ForegroundColor Cyan
    
    # Look for Behavior Designer assets
    $behaviorAssets = Get-ChildItem -Recurse -Include "*.asset" | Where-Object {
        $content = Get-Content $_.FullName -Raw -ErrorAction SilentlyContinue
        $content -and $content -match "BehaviorTree|Behavior Designer"
    }
    
    Write-Host "🌳 Behavior tree assets: $($behaviorAssets.Count)" -ForegroundColor White
    
    # Check for blackboard variables
    $blackboardRefs = Get-ChildItem -Recurse -Include "*.cs" | Select-String -Pattern "\bBlackboard\b" | Measure-Object
    Write-Host "🖤 Blackboard references: $($blackboardRefs.Count)" -ForegroundColor White
    
    # Check for behavior scripts
    $behaviorScripts = Get-ChildItem -Recurse -Include "*.cs" | Where-Object {
        (Get-Content $_.FullName -Raw) -match "BehaviorDesigner|Action|Conditional"
    }
    Write-Host "📜 Behavior scripts: $($behaviorScripts.Count)" -ForegroundColor White
}

function Analyze-Complexity {
    Write-Host "📊 Analyzing code complexity..." -ForegroundColor Cyan
    Write-Host "===============================" -ForegroundColor Cyan
    
    $complexFiles = @()
    
    Get-ChildItem -Recurse -Include "*.cs" | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        $lines = $content.Split("`n").Count
        $methods = ([regex]::Matches($content, "\s+(public|private|protected|internal)\s+\w+\s+\w+\s*\(")).Count
        $classes = ([regex]::Matches($content, "\bclass\s+\w+")).Count
        
        if ($lines -gt 500 -or $methods -gt 20) {
            $complexFiles += [PSCustomObject]@{
                File = $_.Name
                Lines = $lines
                Methods = $methods
                Classes = $classes
            }
        }
    }
    
    if ($complexFiles) {
        Write-Host "⚠️ Complex files (>500 lines or >20 methods):" -ForegroundColor Yellow
        $complexFiles | Sort-Object Lines -Descending | ForEach-Object {
            Write-Host "  $($_.File): $($_.Lines) lines, $($_.Methods) methods" -ForegroundColor White
        }
    } else {
        Write-Host "✅ No overly complex files found" -ForegroundColor Green
    }
}

function Check-MetaFiles {
    Write-Host "📄 Validating .meta files..." -ForegroundColor Cyan
    Write-Host "=============================" -ForegroundColor Cyan
    
    $assetsWithoutMeta = @()
    $metaWithoutAssets = @()
    
    # Find assets without .meta files
    Get-ChildItem -Recurse -Path "Assets" | Where-Object { 
        $_.Name -notlike "*.meta" -and -not (Test-Path "$($_.FullName).meta")
    } | ForEach-Object {
        $assetsWithoutMeta += $_.FullName.Replace($ProjectRoot, "").TrimStart("\")
    }
    
    # Find .meta files without corresponding assets
    Get-ChildItem -Recurse -Path "Assets" -Include "*.meta" | Where-Object {
        $assetPath = $_.FullName.Replace(".meta", "")
        -not (Test-Path $assetPath)
    } | ForEach-Object {
        $metaWithoutAssets += $_.FullName.Replace($ProjectRoot, "").TrimStart("\")
    }
    
    if ($assetsWithoutMeta) {
        Write-Host "⚠️ Assets without .meta files:" -ForegroundColor Yellow
        $assetsWithoutMeta | Select-Object -First 10 | ForEach-Object {
            Write-Host "  $_" -ForegroundColor White
        }
    }
    
    if ($metaWithoutAssets) {
        Write-Host "⚠️ Orphaned .meta files:" -ForegroundColor Yellow
        $metaWithoutAssets | Select-Object -First 10 | ForEach-Object {
            Write-Host "  $_" -ForegroundColor White
        }
    }
    
    if (-not $assetsWithoutMeta -and -not $metaWithoutAssets) {
        Write-Host "✅ All .meta files are properly paired" -ForegroundColor Green
    }
}

function Generate-Metrics {
    Write-Host "📈 Code metrics report..." -ForegroundColor Cyan
    Write-Host "=========================" -ForegroundColor Cyan
    
    $allFiles = Get-ChildItem -Recurse -Include "*.cs"
    $totalLines = 0
    $totalMethods = 0
    $totalClasses = 0
    
    $allFiles | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        $totalLines += $content.Split("`n").Count
        $totalMethods += ([regex]::Matches($content, "\s+(public|private|protected|internal)\s+\w+\s+\w+\s*\(")).Count
        $totalClasses += ([regex]::Matches($content, "\bclass\s+\w+")).Count
    }
    
    Write-Host "📄 Total C# files: $($allFiles.Count)" -ForegroundColor White
    Write-Host "📝 Total lines of code: $totalLines" -ForegroundColor White
    Write-Host "🔧 Total methods: $totalMethods" -ForegroundColor White
    Write-Host "🏗️ Total classes: $totalClasses" -ForegroundColor White
    
    $avgLinesPerFile = [math]::Round($totalLines / $allFiles.Count, 1)
    $avgMethodsPerFile = [math]::Round($totalMethods / $allFiles.Count, 1)
    
    Write-Host "`n📊 Averages:" -ForegroundColor Cyan
    Write-Host "  Lines per file: $avgLinesPerFile" -ForegroundColor White
    Write-Host "  Methods per file: $avgMethodsPerFile" -ForegroundColor White
}

# Main command dispatcher
switch ($Action.ToLower()) {
    "compile" { Check-Compilation }
    "syntax" { Check-Syntax }
    "missing-refs" { Find-MissingReferences }
    "unused" { Find-UnusedUsings }
    "entity-validate" { Validate-EntitySystem }
    "entity-stats" { Test-StatSystem }
    "behavior-check" { Check-BehaviorTrees }
    "complexity" { Analyze-Complexity }
    "meta-check" { Check-MetaFiles }
    "metrics" { Generate-Metrics }
    default { Show-Help }
}

Pop-Location
