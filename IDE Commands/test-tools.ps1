# Testing and Validation Tools - Code quality and validation
# Run: .\test-tools.ps1 [action]

param(
    [string]$Action = "help"
)

$ProjectRoot = Split-Path -Parent $PSScriptRoot
Push-Location $ProjectRoot

function Show-Help {
    Write-Host "🧪 Testing and Validation Tools" -ForegroundColor Cyan
    Write-Host "===============================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "USAGE: .\test-tools.ps1 [action]" -ForegroundColor Green
    Write-Host ""
    Write-Host "🔍 COMPILATION:" -ForegroundColor Yellow
    Write-Host "  compile          - Check compilation"
    Write-Host "  syntax           - Basic syntax validation"
    Write-Host "  missing-refs     - Find missing references"
    Write-Host "  unused           - Find unused variables/methods"
    Write-Host ""
    Write-Host "🎯 ENTITY SYSTEM:" -ForegroundColor Yellow
    Write-Host "  entity-validate  - Validate Entity system"
    Write-Host "  entity-stats     - Entity system statistics"
    Write-Host "  behavior-check   - Check behavior trees"
    Write-Host ""
    Write-Host "📊 ANALYSIS:" -ForegroundColor Yellow
    Write-Host "  complexity       - Analyze code complexity"
    Write-Host "  meta-check       - Check Unity meta files"
    Write-Host "  metrics          - Generate code metrics"
    Write-Host ""
}

function Test-Compilation {
    Write-Host "🔨 Testing compilation..." -ForegroundColor Green
    
    # Build each project separately to catch specific issues
    $projects = @("Assembly-CSharp.csproj", "Lineage.Logic.csproj")
    $success = $true
    
    foreach ($project in $projects) {
        if (Test-Path $project) {
            Write-Host "`n📁 Building $project..." -ForegroundColor Cyan
            $result = dotnet build $project --verbosity minimal --no-restore 2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ $project built successfully" -ForegroundColor Green
            } else {
                Write-Host "❌ $project build failed" -ForegroundColor Red
                $result | Where-Object { $_ -match "error|Error|ERROR" } | ForEach-Object {
                    Write-Host "  $_" -ForegroundColor Red
                }
                $success = $false
            }
        }
    }
    
    if ($success) {
        Write-Host "`n✅ All projects compiled successfully!" -ForegroundColor Green
    } else {
        Write-Host "`n❌ Compilation errors found!" -ForegroundColor Red
    }
}

function Test-Syntax {
    Write-Host "📝 Checking basic syntax..." -ForegroundColor Cyan
    
    $issues = @()
    Get-ChildItem -Recurse -Include "*.cs" | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        $fileName = $_.Name
        
        # Check for common syntax issues
        if ($content -match ";\s*;") {
            $issues += "$fileName - Double semicolons found"
        }
        
        if ($content -match "\{\s*\}") {
            # Empty blocks - might be intentional, so just note
        }
        
        # Check for unmatched braces (basic check)
        $openBraces = ($content.ToCharArray() | Where-Object { $_ -eq '{' }).Count
        $closeBraces = ($content.ToCharArray() | Where-Object { $_ -eq '}' }).Count
        
        if ($openBraces -ne $closeBraces) {
            $issues += "$fileName - Unmatched braces: $openBraces open, $closeBraces close"
        }
    }
    
    if ($issues.Count -eq 0) {
        Write-Host "✅ No syntax issues found!" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Found $($issues.Count) potential issues:" -ForegroundColor Yellow
        $issues | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    }
}

function Find-MissingReferences {
    Write-Host "🔍 Checking for missing references..." -ForegroundColor Cyan
    
    $issues = @()
    Get-ChildItem -Recurse -Include "*.cs" | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        $fileName = $_.Name
        
        # Look for common Unity types that might be missing references
        $unityTypes = @("GameObject", "Transform", "MonoBehaviour", "ScriptableObject")
        
        foreach ($type in $unityTypes) {
            if ($content -match "\b$type\b" -and $content -notmatch "using UnityEngine") {
                $issues += "$fileName - Uses $type but missing 'using UnityEngine'"
                break
            }
        }
    }
    
    if ($issues.Count -eq 0) {
        Write-Host "✅ No obvious missing references found!" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Found $($issues.Count) potential missing references:" -ForegroundColor Yellow
        $issues | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    }
}

function Find-UnusedCode {
    Write-Host "🗑️ Looking for potentially unused code..." -ForegroundColor Cyan
    
    # This is a basic check - a full analysis would require more sophisticated tools
    $privateFields = @()
    $publicMethods = @()
    
    Get-ChildItem -Recurse -Include "*.cs" | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        $fileName = $_.Name
        
        # Find private fields
        $privateMatches = [regex]::Matches($content, "private\s+\w+\s+(\w+)\s*;")
        foreach ($match in $privateMatches) {
            $fieldName = $match.Groups[1].Value
            if ($content.Split("`n").Count -gt 0) {
                $usageCount = ([regex]::Matches($content, "\b$fieldName\b")).Count
                if ($usageCount -le 1) {  # Only declared, never used
                    $privateFields += "$fileName - Potentially unused field: $fieldName"
                }
            }
        }
    }
    
    if ($privateFields.Count -eq 0) {
        Write-Host "✅ No obviously unused private fields found!" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Found $($privateFields.Count) potentially unused items:" -ForegroundColor Yellow
        $privateFields | Select-Object -First 10 | ForEach-Object { 
            Write-Host "  $_" -ForegroundColor Yellow 
        }
        if ($privateFields.Count -gt 10) {
            Write-Host "  ... and $($privateFields.Count - 10) more" -ForegroundColor Gray
        }
    }
}

function Test-EntitySystem {
    Write-Host "🎯 Validating Entity system..." -ForegroundColor Cyan
    
    $entityFiles = @(
        "Assets\Scripts\Entities\Entity.cs",
        "Assets\Scripts\Entities\EntityTypeData.cs"
    )
    
    $issues = @()
    
    foreach ($file in $entityFiles) {
        if (-not (Test-Path $file)) {
            $issues += "Missing core file: $file"
        } else {
            Write-Host "✅ Found $file" -ForegroundColor Green
            
            # Basic validation of Entity.cs
            if ($file -like "*Entity.cs") {
                $content = Get-Content $file -Raw
                
                $requiredMethods = @("Start", "Update", "OnDestroy")
                foreach ($method in $requiredMethods) {
                    if ($content -notmatch "\s+$method\s*\(") {
                        $issues += "Entity.cs may be missing $method method"
                    }
                }
                
                if ($content -match "public class Entity") {
                    Write-Host "✅ Entity class structure looks good" -ForegroundColor Green
                }
            }
        }
    }
    
    # Check for Entity references
    $entityRefs = Get-ChildItem -Recurse -Include "*.cs" | Select-String -Pattern "\bEntity\b" | Measure-Object
    Write-Host "📊 Found $($entityRefs.Count) references to Entity in codebase" -ForegroundColor Cyan
    
    if ($issues.Count -eq 0) {
        Write-Host "✅ Entity system validation passed!" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Entity system issues found:" -ForegroundColor Yellow
        $issues | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    }
}

function Get-EntityStats {
    Write-Host "📊 Entity system statistics..." -ForegroundColor Cyan
    
    # Count entity-related files
    $entityFiles = Get-ChildItem -Recurse -Include "*.cs" | Where-Object { $_.Name -match "Entity|Health|Stat" }
    Write-Host "📁 Entity-related files: $($entityFiles.Count)" -ForegroundColor White
    
    # Count prefabs
    $prefabs = Get-ChildItem -Recurse -Include "*.prefab" | Measure-Object
    Write-Host "🎮 Prefabs: $($prefabs.Count)" -ForegroundColor White
    
    # Count scenes
    $scenes = Get-ChildItem -Recurse -Include "*.unity" | Measure-Object
    Write-Host "🌍 Scenes: $($scenes.Count)" -ForegroundColor White
    
    # Analyze Entity usage
    $entityUsage = Get-ChildItem -Recurse -Include "*.cs" | Select-String -Pattern "\bEntity\b"
    Write-Host "🔗 Entity references: $($entityUsage.Count)" -ForegroundColor White
    
    if ($entityFiles.Count -gt 0) {
        Write-Host "`n📋 Entity-related files:" -ForegroundColor Yellow
        $entityFiles | Select-Object -First 10 | ForEach-Object {
            $relPath = $_.FullName.Replace($ProjectRoot, "").TrimStart("\")
            Write-Host "  $relPath" -ForegroundColor White
        }
    }
}

function Test-BehaviorTrees {
    Write-Host "🌳 Checking behavior trees..." -ForegroundColor Cyan
    
    $behaviorFiles = Get-ChildItem -Recurse -Include "*Behavior*.cs", "*Tree*.cs"
    
    if ($behaviorFiles.Count -eq 0) {
        Write-Host "ℹ️ No behavior tree files found" -ForegroundColor Gray
    } else {
        Write-Host "📁 Found $($behaviorFiles.Count) behavior-related files:" -ForegroundColor Green
        $behaviorFiles | ForEach-Object {
            $relPath = $_.FullName.Replace($ProjectRoot, "").TrimStart("\")
            Write-Host "  $relPath" -ForegroundColor White
        }
        
        # Check for behavior tree assets
        $behaviorAssets = Get-ChildItem -Recurse -Include "*.asset" | Where-Object { $_.Name -match "Behavior|Tree" }
        if ($behaviorAssets.Count -gt 0) {
            Write-Host "`n🎯 Behavior assets found: $($behaviorAssets.Count)" -ForegroundColor Green
        }
    }
}

function Get-CodeComplexity {
    Write-Host "📈 Analyzing code complexity..." -ForegroundColor Cyan
    
    $complexFiles = @()
    
    Get-ChildItem -Recurse -Include "*.cs" | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        $lines = $content.Split("`n").Count
        $methods = ([regex]::Matches($content, "\s+(public|private|protected|internal)\s+\w+\s+\w+\s*\(")).Count
        $classes = ([regex]::Matches($content, "\\bclass\\s+\\w+")).Count
        
        if ($lines -gt 500 -or $methods -gt 20) {
            $complexFiles += [PSCustomObject]@{
                File = $_.Name
                Lines = $lines
                Methods = $methods
                Classes = $classes
            }
        }
    }
    
    if ($complexFiles.Count -eq 0) {
        Write-Host "✅ No overly complex files found!" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Complex files found:" -ForegroundColor Yellow
        $complexFiles | Sort-Object Lines -Descending | ForEach-Object {
            Write-Host "  $($_.File): $($_.Lines) lines, $($_.Methods) methods, $($_.Classes) classes" -ForegroundColor Red
        }
    }
}

function Test-MetaFiles {
    Write-Host "🔍 Checking Unity meta files..." -ForegroundColor Cyan
    
    $csharpFiles = Get-ChildItem -Recurse -Include "*.cs"
    $metaFiles = Get-ChildItem -Recurse -Include "*.meta"
    $missingMeta = @()
    
    foreach ($file in $csharpFiles) {
        $expectedMeta = "$($file.FullName).meta"
        if (-not (Test-Path $expectedMeta)) {
            $missingMeta += $file.FullName.Replace($ProjectRoot, "").TrimStart("\")
        }
    }
    
    Write-Host "📊 Total .cs files: $($csharpFiles.Count)" -ForegroundColor White
    Write-Host "📊 Total .meta files: $($metaFiles.Count)" -ForegroundColor White
    
    if ($missingMeta.Count -eq 0) {
        Write-Host "✅ All C# files have corresponding .meta files!" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Missing .meta files for $($missingMeta.Count) files:" -ForegroundColor Yellow
        $missingMeta | Select-Object -First 10 | ForEach-Object {
            Write-Host "  $_" -ForegroundColor Red
        }
        if ($missingMeta.Count -gt 10) {
            Write-Host "  ... and $($missingMeta.Count - 10) more" -ForegroundColor Gray
        }
    }
}

function Get-CodeMetrics {
    Write-Host "📊 Generating code metrics..." -ForegroundColor Cyan
    
    $csharpFiles = Get-ChildItem -Recurse -Include "*.cs"
    $totalLines = 0
    $totalMethods = 0
    $totalClasses = 0
    
    foreach ($file in $csharpFiles) {
        $content = Get-Content $file.FullName -Raw
        $lines = $content.Split("`n").Count
        $methods = ([regex]::Matches($content, "\s+(public|private|protected|internal)\s+\w+\s+\w+\s*\(")).Count
        $classes = ([regex]::Matches($content, "\\bclass\\s+\\w+")).Count
        
        $totalLines += $lines
        $totalMethods += $methods
        $totalClasses += $classes
    }
    
    Write-Host "📈 Code Metrics Summary:" -ForegroundColor Green
    Write-Host "  📄 Total C# files: $($csharpFiles.Count)" -ForegroundColor White
    Write-Host "  📝 Total lines of code: $totalLines" -ForegroundColor White
    Write-Host "  🔧 Total methods: $totalMethods" -ForegroundColor White
    Write-Host "  🏗️ Total classes: $totalClasses" -ForegroundColor White
    
    if ($csharpFiles.Count -gt 0) {
        $avgLines = [math]::Round($totalLines / $csharpFiles.Count, 1)
        $avgMethods = [math]::Round($totalMethods / $csharpFiles.Count, 1)
        
        Write-Host "  📊 Average lines per file: $avgLines" -ForegroundColor White
        Write-Host "  📊 Average methods per file: $avgMethods" -ForegroundColor White
    }
}

# Main command dispatcher
switch ($Action.ToLower()) {
    "compile" { Test-Compilation }
    "syntax" { Test-Syntax }
    "missing-refs" { Find-MissingReferences }
    "unused" { Find-UnusedCode }
    "entity-validate" { Test-EntitySystem }
    "entity-stats" { Get-EntityStats }
    "behavior-check" { Test-BehaviorTrees }
    "complexity" { Get-CodeComplexity }
    "meta-check" { Test-MetaFiles }
    "metrics" { Get-CodeMetrics }
    default { Show-Help }
}
Pop-Location
