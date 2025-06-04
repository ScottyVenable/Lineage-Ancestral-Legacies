# Development Quick Commands - Essential shortcuts for daily Unity development
# Run: .\dev-quick.ps1

param(
    [string]$Action = "help"
)

$ProjectRoot = Split-Path -Parent $PSScriptRoot
Push-Location $ProjectRoot

function Show-Help {
    Write-Host "🚀 Development Quick Commands" -ForegroundColor Cyan
    Write-Host "=============================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "USAGE: .\dev-quick.ps1 [action]" -ForegroundColor Green
    Write-Host ""
    Write-Host "📁 PROJECT ACTIONS:" -ForegroundColor Yellow
    Write-Host "  build        - Build all C# projects"
    Write-Host "  clean        - Clean build artifacts and temp files"
    Write-Host "  reset        - Full reset (clean + rebuild)"
    Write-Host "  errors       - Check for compilation errors"
    Write-Host ""
    Write-Host "🔍 CODE ACTIONS:" -ForegroundColor Yellow
    Write-Host "  find [term]  - Find text in all C# files"
    Write-Host "  entity       - Quick edit Entity.cs"
    Write-Host "  entitydata   - Quick edit EntityTypeData.cs"
    Write-Host "  docs         - Open documentation folder"
    Write-Host ""
    Write-Host "⚡ PERFORMANCE:" -ForegroundColor Yellow
    Write-Host "  monitor      - Start performance monitoring"
    Write-Host "  cache-clear  - Clear Unity caches"
    Write-Host "  mem          - Show memory usage"
    Write-Host ""
    Write-Host "📊 STATUS:" -ForegroundColor Yellow
    Write-Host "  status       - Show project status"
    Write-Host "  git          - Git status and recent commits"
    Write-Host ""
}

function Build-Project {
    Write-Host "🔨 Building C# Projects..." -ForegroundColor Green
    dotnet build "Assembly-CSharp.csproj" --verbosity minimal
    dotnet build "Lineage.Logic.csproj" --verbosity minimal
    Write-Host "✅ Build complete!" -ForegroundColor Green
}

function Clean-Project {
    Write-Host "🧹 Cleaning project..." -ForegroundColor Yellow
    
    # Clean build outputs
    if (Test-Path "bin") { Remove-Item "bin" -Recurse -Force }
    if (Test-Path "obj") { Remove-Item "obj" -Recurse -Force }
    
    # Clean Unity temp files
    if (Test-Path "Temp") { Remove-Item "Temp" -Recurse -Force }
    if (Test-Path "Library\ShaderCache") { Remove-Item "Library\ShaderCache" -Recurse -Force }
    if (Test-Path "Library\UIElements") { Remove-Item "Library\UIElements" -Recurse -Force }
    if (Test-Path "Library\BurstCache") { Remove-Item "Library\BurstCache" -Recurse -Force }
    
    Write-Host "✅ Cleanup complete!" -ForegroundColor Green
}

function Find-InCode {
    param([string]$SearchTerm)
    if (-not $SearchTerm) {
        $SearchTerm = Read-Host "Enter search term"
    }
    
    Write-Host "🔍 Searching for '$SearchTerm' in C# files..." -ForegroundColor Cyan
    Get-ChildItem -Recurse -Include "*.cs" | Select-String -Pattern $SearchTerm | 
        Select-Object -First 20 | ForEach-Object {
            Write-Host "$($_.Filename):$($_.LineNumber)" -ForegroundColor Green -NoNewline
            Write-Host " - $($_.Line.Trim())" -ForegroundColor White
        }
}

function Show-ProjectStatus {
    Write-Host "📊 Project Status" -ForegroundColor Cyan
    Write-Host "=================" -ForegroundColor Cyan
    
    # File counts
    $csFiles = (Get-ChildItem -Recurse -Include "*.cs" | Measure-Object).Count
    $prefabs = (Get-ChildItem -Recurse -Include "*.prefab" | Measure-Object).Count
    $scenes = (Get-ChildItem -Recurse -Include "*.unity" | Measure-Object).Count
    
    Write-Host "📄 C# Files: $csFiles" -ForegroundColor White
    Write-Host "🎮 Prefabs: $prefabs" -ForegroundColor White
    Write-Host "🌍 Scenes: $scenes" -ForegroundColor White
    
    # Project size
    $size = (Get-ChildItem -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
    Write-Host "💾 Project Size: $([math]::Round($size, 2)) MB" -ForegroundColor White
    
    # Unity process
    $unityProcess = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
    if ($unityProcess) {
        $memMB = [math]::Round($unityProcess.WorkingSet64 / 1MB, 0)
        Write-Host "🎯 Unity Memory: $memMB MB" -ForegroundColor White
    } else {
        Write-Host "🎯 Unity: Not running" -ForegroundColor Red
    }
}

function Show-GitStatus {
    Write-Host "📋 Git Status" -ForegroundColor Cyan
    Write-Host "==============" -ForegroundColor Cyan
    
    git status --short
    
    Write-Host "`n📈 Recent Commits:" -ForegroundColor Cyan
    git log --oneline -5
}

function Open-EntityFile {
    $entityPath = "Assets\Scripts\Entities\Entity.cs"
    if (Test-Path $entityPath) {
        code $entityPath
        Write-Host "📝 Opening Entity.cs in VS Code..." -ForegroundColor Green
    } else {
        Write-Host "❌ Entity.cs not found!" -ForegroundColor Red
    }
}

function Open-EntityDataFile {
    $entityDataPath = "Assets\Scripts\Entities\EntityTypeData.cs"
    if (Test-Path $entityDataPath) {
        code $entityDataPath
        Write-Host "📝 Opening EntityTypeData.cs in VS Code..." -ForegroundColor Green
    } else {
        Write-Host "❌ EntityTypeData.cs not found!" -ForegroundColor Red
    }
}

function Open-Docs {
    $docsPath = "Documents"
    if (Test-Path $docsPath) {
        code $docsPath
        Write-Host "📚 Opening Documents folder..." -ForegroundColor Green
    } else {
        Write-Host "❌ Documents folder not found!" -ForegroundColor Red
    }
}

function Start-Monitor {
    Write-Host "📊 Starting performance monitor..." -ForegroundColor Green
    Start-Process powershell -ArgumentList "-File", "IDE Commands\unity-performance-monitor.ps1"
}

function Clear-Caches {
    Write-Host "🗑️ Clearing Unity caches..." -ForegroundColor Yellow
    if (Test-Path "Library\ShaderCache") { Remove-Item "Library\ShaderCache" -Recurse -Force }
    if (Test-Path "Library\UIElements") { Remove-Item "Library\UIElements" -Recurse -Force }
    if (Test-Path "Library\BurstCache") { Remove-Item "Library\BurstCache" -Recurse -Force }
    Write-Host "✅ Caches cleared!" -ForegroundColor Green
}

function Show-Memory {
    Write-Host "💾 Memory Usage" -ForegroundColor Cyan
    Write-Host "===============" -ForegroundColor Cyan
    
    $processes = @("Unity", "Code", "devenv")
    foreach ($proc in $processes) {
        $process = Get-Process -Name $proc -ErrorAction SilentlyContinue
        if ($process) {
            $memMB = [math]::Round($process.WorkingSet64 / 1MB, 0)
            Write-Host "$proc`: $memMB MB" -ForegroundColor White
        }
    }
}

function Check-Errors {
    Write-Host "🔍 Checking for compilation errors..." -ForegroundColor Cyan
    
    # Look for common error patterns in recent log files
    $logPath = "Logs"
    if (Test-Path $logPath) {
        $recentLog = Get-ChildItem $logPath -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        if ($recentLog) {
            $errors = Select-String -Path $recentLog.FullName -Pattern "(error|Error|ERROR)" | Select-Object -First 10
            if ($errors) {
                Write-Host "⚠️ Found potential errors:" -ForegroundColor Red                $errors | ForEach-Object { Write-Host "  $($_.Line)" -ForegroundColor Yellow }
            } else {
                Write-Host "✅ No errors found in recent logs!" -ForegroundColor Green
            }
        }
    }
}

# Main command dispatcher
switch ($Action.ToLower()) {
    "build" { Build-Project }
    "clean" { Clean-Project }
    "reset" { Clean-Project; Build-Project }
    "find" { Find-InCode $args[0] }
    "entity" { Open-EntityFile }
    "entitydata" { Open-EntityDataFile }
    "docs" { Open-Docs }
    "monitor" { Start-Monitor }
    "cache-clear" { Clear-Caches }
    "mem" { Show-Memory }
    "status" { Show-ProjectStatus }
    "git" { Show-GitStatus }
    "errors" { Check-Errors }
    default { Show-Help }
}

Pop-Location
