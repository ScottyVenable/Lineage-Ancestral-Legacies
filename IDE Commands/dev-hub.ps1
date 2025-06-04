# Main Command Hub - Central entry point for all development tools
# Run: .\dev-hub.ps1 [command] or just .\dev-hub.ps1 for interactive menu

param(
    [string]$Command = "",
    [string]$Action = "",
    [string]$Parameter = ""
)

$ProjectRoot = Split-Path -Parent $PSScriptRoot
Push-Location $ProjectRoot

function Show-WelcomeBanner {
    Write-Host ""
    Write-Host "🚀 LINEAGE DEVELOPMENT HUB" -ForegroundColor Cyan
    Write-Host "============================" -ForegroundColor Cyan
    Write-Host "Unity Project: Lineage Ancestral Legacies" -ForegroundColor White
    Write-Host "Date: $(Get-Date -Format 'MMMM dd, yyyy - HH:mm')" -ForegroundColor Gray
    
    # Quick project status
    $unityRunning = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
    $vsCodeRunning = Get-Process -Name "Code" -ErrorAction SilentlyContinue
    
    Write-Host ""
    Write-Host "💻 Environment Status:" -ForegroundColor Yellow
    Write-Host "  Unity Editor: $(if($unityRunning){'🟢 Running'}else{'🔴 Stopped'})" -ForegroundColor White
    Write-Host "  VS Code: $(if($vsCodeRunning){'🟢 Running'}else{'🔴 Stopped'})" -ForegroundColor White
    
    # Git status
    $branch = git branch --show-current 2>$null
    if ($branch) {
        Write-Host "  Git Branch: 🌿 $branch" -ForegroundColor White
    }
    
    Write-Host ""
}

function Show-MainMenu {
    Write-Host "📋 AVAILABLE TOOLS:" -ForegroundColor Green
    Write-Host "===================" -ForegroundColor Green
    Write-Host ""
      Write-Host "1️⃣  DEV-QUICK" -ForegroundColor Cyan -NoNewline
    Write-Host "    - Quick development commands (build, clean, find, status)" -ForegroundColor White
    
    Write-Host "2️⃣  CODE-NAV" -ForegroundColor Cyan -NoNewline
    Write-Host "     - Code navigation and search tools" -ForegroundColor White
    
    Write-Host "3️⃣  GIT-FLOW" -ForegroundColor Cyan -NoNewline
    Write-Host "     - Git workflow optimization" -ForegroundColor White
    
    Write-Host "4️⃣  TEST-TOOLS" -ForegroundColor Cyan -NoNewline
    Write-Host "   - Testing and validation tools" -ForegroundColor White
    
    Write-Host "5️⃣  PERFORMANCE" -ForegroundColor Cyan -NoNewline
    Write-Host "  - Performance monitoring (existing tools)" -ForegroundColor White
    
    Write-Host ""
    Write-Host "⚡ QUICK ACTIONS:" -ForegroundColor Yellow
    Write-Host "================" -ForegroundColor Yellow
    Write-Host ""
    
    Write-Host "Q1" -ForegroundColor Green -NoNewline
    Write-Host " - Quick build all projects" -ForegroundColor White
    
    Write-Host "Q2" -ForegroundColor Green -NoNewline
    Write-Host " - Show project status" -ForegroundColor White
    
    Write-Host "Q3" -ForegroundColor Green -NoNewline
    Write-Host " - Open Entity.cs in VS Code" -ForegroundColor White
    
    Write-Host "Q4" -ForegroundColor Green -NoNewline
    Write-Host " - Git status and recent commits" -ForegroundColor White
    
    Write-Host "Q5" -ForegroundColor Green -NoNewline
    Write-Host " - Find TODO/FIXME comments" -ForegroundColor White
    
    Write-Host "Q6" -ForegroundColor Green -NoNewline
    Write-Host " - Validate Entity system" -ForegroundColor White
    
    Write-Host ""
    Write-Host "📚 DOCUMENTATION:" -ForegroundColor Magenta
    Write-Host "=================" -ForegroundColor Magenta
    Write-Host ""
    
    Write-Host "D1" -ForegroundColor Magenta -NoNewline
    Write-Host " - Open Documents folder" -ForegroundColor White
    
    Write-Host "D2" -ForegroundColor Magenta -NoNewline
    Write-Host " - View implementation guide" -ForegroundColor White
    
    Write-Host "D3" -ForegroundColor Magenta -NoNewline
    Write-Host " - Show all help menus" -ForegroundColor White
    
    Write-Host ""
    Write-Host "Type a command (1-5, Q1-Q6, D1-D3) or 'exit' to quit:" -ForegroundColor Cyan -NoNewline
    Write-Host " " -NoNewline
}

function Execute-QuickAction {
    param([string]$ActionCode)
    
    switch ($ActionCode.ToUpper()) {
        "Q1" {
            Write-Host "🔨 Quick building all projects..." -ForegroundColor Green
            & ".\IDE Commands\dev-quick.ps1" build
        }
        "Q2" {
            Write-Host "📊 Showing project status..." -ForegroundColor Green
            & ".\IDE Commands\dev-quick.ps1" status
        }
        "Q3" {
            Write-Host "📝 Opening Entity.cs..." -ForegroundColor Green
            & ".\IDE Commands\dev-quick.ps1" entity
        }
        "Q4" {
            Write-Host "📋 Showing Git status..." -ForegroundColor Green
            & ".\IDE Commands\git-flow.ps1" status
        }
        "Q5" {
            Write-Host "📝 Finding TODO/FIXME comments..." -ForegroundColor Green
            & ".\IDE Commands\code-nav.ps1" todo
            & ".\IDE Commands\code-nav.ps1" fixme
        }
        "Q6" {
            Write-Host "🎯 Validating Entity system..." -ForegroundColor Green
            & ".\IDE Commands\test-tools.ps1" entity-validate
        }
        "D1" {
            Write-Host "📚 Opening Documents folder..." -ForegroundColor Magenta
            & ".\IDE Commands\dev-quick.ps1" docs
        }
        "D2" {
            Write-Host "📖 Opening implementation guide..." -ForegroundColor Magenta
            $implPath = "Assets\Scripts\Entities\Documentation\IMPLEMENTATION.md"
            if (Test-Path $implPath) {
                code $implPath
            } else {
                Write-Host "❌ Implementation guide not found!" -ForegroundColor Red
            }
        }
        "D3" {
            Write-Host "📋 Showing all help menus..." -ForegroundColor Magenta
            Write-Host "`n=== DEV-QUICK HELP ===" -ForegroundColor Cyan
            & ".\IDE Commands\dev-quick.ps1" help
            Write-Host "`n=== CODE-NAV HELP ===" -ForegroundColor Cyan
            & ".\IDE Commands\code-nav.ps1" help
            Write-Host "`n=== GIT-FLOW HELP ===" -ForegroundColor Cyan
            & ".\IDE Commands\git-flow.ps1" help
            Write-Host "`n=== TEST-TOOLS HELP ===" -ForegroundColor Cyan
            & ".\IDE Commands\test-tools.ps1" help
        }
        default {
            Write-Host "❌ Unknown quick action: $ActionCode" -ForegroundColor Red
        }
    }
}

function Execute-ToolCommand {
    param([string]$ToolNumber)
    
    switch ($ToolNumber) {
        "1" {
            Write-Host "🚀 Starting DEV-QUICK tool..." -ForegroundColor Green
            Write-Host "Available actions: build, clean, reset, find, entity, entitydata, docs, monitor, cache-clear, mem, status, git, errors" -ForegroundColor Yellow
            $action = Read-Host "Enter action"
            if ($action) {
                & ".\IDE Commands\dev-quick.ps1" $action
            }
        }
        "2" {
            Write-Host "🧭 Starting CODE-NAV tool..." -ForegroundColor Green
            Write-Host "Available actions: class, method, usage, todo, fixme, entity, recent, large, duplicates, scripts" -ForegroundColor Yellow
            $action = Read-Host "Enter action"
            if ($action) {
                & ".\IDE Commands\code-nav.ps1" $action
            }
        }
        "3" {
            Write-Host "🔀 Starting GIT-FLOW tool..." -ForegroundColor Green
            Write-Host "Available actions: status, log, diff, branches, quick, save, backup, checkpoint, feature, switch" -ForegroundColor Yellow
            $action = Read-Host "Enter action"
            if ($action) {
                & ".\IDE Commands\git-flow.ps1" $action
            }
        }
        "4" {
            Write-Host "🧪 Starting TEST-TOOLS..." -ForegroundColor Green
            Write-Host "Available actions: compile, syntax, missing-refs, unused, entity-validate, entity-stats, behavior-check" -ForegroundColor Yellow
            $action = Read-Host "Enter action"
            if ($action) {
                & ".\IDE Commands\test-tools.ps1" $action
            }
        }
        "5" {
            Write-Host "📊 Starting Performance Monitor..." -ForegroundColor Green
            Start-Process powershell -ArgumentList "-File", "IDE Commands\unity-performance-monitor.ps1"
        }
        default {
            Write-Host "❌ Unknown tool number: $ToolNumber" -ForegroundColor Red
        }
    }
}

function Start-InteractiveMode {
    Show-WelcomeBanner
    
    while ($true) {
        Show-MainMenu
        $input = Read-Host
        
        if ($input -eq "exit" -or $input -eq "quit" -or $input -eq "q") {
            Write-Host "👋 Goodbye! Happy coding!" -ForegroundColor Green
            break
        }
        
        Write-Host ""
        
        if ($input -match "^Q\d+$" -or $input -match "^D\d+$") {
            Execute-QuickAction $input
        }
        elseif ($input -match "^\d+$") {
            Execute-ToolCommand $input
        }
        else {
            Write-Host "❌ Invalid command. Please use 1-5 for tools, Q1-Q6 for quick actions, or D1-D3 for docs." -ForegroundColor Red
        }
        
        Write-Host ""
        Write-Host "Press Enter to continue..." -ForegroundColor Gray
        Read-Host
        Clear-Host
    }
}

# Command-line mode
if ($Command) {
    switch ($Command.ToLower()) {
        "quick" { & ".\IDE Commands\dev-quick.ps1" $Action $Parameter }
        "nav" { & ".\IDE Commands\code-nav.ps1" $Action $Parameter }
        "git" { & ".\IDE Commands\git-flow.ps1" $Action $Parameter }
        "test" { & ".\IDE Commands\test-tools.ps1" $Action $Parameter }
        "status" {
            Show-WelcomeBanner
            & ".\IDE Commands\dev-quick.ps1" status
        }
        "build" {
            Write-Host "🔨 Building project..." -ForegroundColor Green
            & ".\IDE Commands\dev-quick.ps1" build
        }
        "help" {
            Show-WelcomeBanner
            Write-Host "📋 Command Line Usage:" -ForegroundColor Cyan
            Write-Host "======================" -ForegroundColor Cyan
            Write-Host ""
            Write-Host ".\dev-hub.ps1                     # Interactive mode" -ForegroundColor White
            Write-Host ".\dev-hub.ps1 quick [action]      # Run dev-quick command" -ForegroundColor White
            Write-Host ".\dev-hub.ps1 nav [action]        # Run code-nav command" -ForegroundColor White
            Write-Host ".\dev-hub.ps1 git [action]        # Run git-flow command" -ForegroundColor White
            Write-Host ".\dev-hub.ps1 test [action]       # Run test-tools command" -ForegroundColor White
            Write-Host ".\dev-hub.ps1 status              # Quick project status" -ForegroundColor White
            Write-Host ".\dev-hub.ps1 build               # Quick build" -ForegroundColor White
            Write-Host ""
            Write-Host "Examples:" -ForegroundColor Yellow
            Write-Host ".\dev-hub.ps1 quick entity         # Open Entity.cs" -ForegroundColor Gray
            Write-Host ".\dev-hub.ps1 git status           # Git status" -ForegroundColor Gray
            Write-Host ".\dev-hub.ps1 nav todo             # Find TODOs" -ForegroundColor Gray
            Write-Host ".\dev-hub.ps1 test compile         # Check compilation" -ForegroundColor Gray
        }
        default {
            Write-Host "❌ Unknown command: $Command" -ForegroundColor Red
            Write-Host "Use '.\dev-hub.ps1 help' for usage information" -ForegroundColor Yellow
        }
    }
} else {
    # Interactive mode
    Start-InteractiveMode
}

Pop-Location
