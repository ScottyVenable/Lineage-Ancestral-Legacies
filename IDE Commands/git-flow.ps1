# Git Workflow Optimization - Streamlined git operations for Unity development
# Run: .\git-flow.ps1 [action]

param(
    [string]$Action = "help",
    [string]$Message = "",
    [string]$Branch = ""
)

$ProjectRoot = Split-Path -Parent $PSScriptRoot
Push-Location $ProjectRoot

function Show-Help {
    Write-Host "🔀 Git Workflow Tools" -ForegroundColor Cyan
    Write-Host "=====================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "USAGE: .\git-flow.ps1 [action] [parameters]" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 STATUS COMMANDS:" -ForegroundColor Yellow
    Write-Host "  status           - Enhanced git status"
    Write-Host "  log              - Pretty commit history"
    Write-Host "  diff             - Show current changes"
    Write-Host "  branches         - List all branches"
    Write-Host ""
    Write-Host "💾 COMMIT COMMANDS:" -ForegroundColor Yellow
    Write-Host "  quick [msg]      - Add all & commit with message"
    Write-Host "  save [msg]       - Stage changes & commit"
    Write-Host "  backup           - Create backup commit"
    Write-Host "  checkpoint       - Quick checkpoint save"
    Write-Host ""
    Write-Host "🌿 BRANCH COMMANDS:" -ForegroundColor Yellow
    Write-Host "  feature [name]   - Create feature branch"
    Write-Host "  switch [branch]  - Switch to branch"
    Write-Host "  clean-branches   - Clean merged branches"
    Write-Host ""
    Write-Host "🛡️ SAFETY COMMANDS:" -ForegroundColor Yellow
    Write-Host "  unity-ignore     - Setup Unity .gitignore"
    Write-Host "  stash            - Stash current changes"
    Write-Host "  unstash          - Pop latest stash"
    Write-Host "  reset-soft       - Soft reset last commit"
    Write-Host ""
}

function Show-Status {
    Write-Host "📊 Enhanced Git Status" -ForegroundColor Cyan
    Write-Host "======================" -ForegroundColor Cyan
    
    # Current branch
    $branch = git branch --show-current
    Write-Host "🌿 Current Branch: $branch" -ForegroundColor Green
    
    # Uncommitted changes
    $status = git status --porcelain
    if ($status) {
        Write-Host "`n📝 Changes:" -ForegroundColor Yellow
        $status | ForEach-Object {
            $statusCode = $_.Substring(0, 2)
            $file = $_.Substring(3)
            
            $color = switch ($statusCode.Trim()) {
                "M" { "Yellow" }
                "A" { "Green" }
                "D" { "Red" }
                "??" { "Cyan" }
                default { "White" }
            }
            
            Write-Host "  $statusCode $file" -ForegroundColor $color
        }
    } else {
        Write-Host "`n✅ Working tree clean" -ForegroundColor Green
    }
    
    # Recent commits
    Write-Host "`n📈 Recent Commits:" -ForegroundColor Cyan
    git log --oneline -5 --decorate
    
    # Unity specific files check
    $unityFiles = @("*.meta", "*.asset", "*.prefab", "*.unity", "*.cs")
    $changedUnityFiles = git diff --name-only HEAD | Where-Object { 
        $file = $_
        $unityFiles | Where-Object { $file -like $_ }
    }
    
    if ($changedUnityFiles) {
        Write-Host "`n🎮 Unity Files Changed:" -ForegroundColor Magenta
        $changedUnityFiles | ForEach-Object { Write-Host "  $_" -ForegroundColor White }
    }
}

function Show-Log {
    Write-Host "📈 Pretty Commit History" -ForegroundColor Cyan
    Write-Host "========================" -ForegroundColor Cyan
    
    git log --oneline --graph --decorate --all -15
}

function Show-Diff {
    Write-Host "🔍 Current Changes" -ForegroundColor Cyan
    Write-Host "==================" -ForegroundColor Cyan
    
    # Show diff summary
    git diff --stat
    
    Write-Host "`n📄 Detailed changes (first 50 lines):" -ForegroundColor Yellow
    git diff HEAD | Select-Object -First 50
}

function Show-Branches {
    Write-Host "🌿 All Branches" -ForegroundColor Cyan
    Write-Host "===============" -ForegroundColor Cyan
    
    # Local branches
    Write-Host "`n📍 Local Branches:" -ForegroundColor Green
    git branch -v
    
    # Remote branches
    Write-Host "`n🌐 Remote Branches:" -ForegroundColor Yellow
    git branch -r -v
}

function Quick-Commit {
    param([string]$CommitMessage)
    
    if (-not $CommitMessage) {
        $CommitMessage = Read-Host "Enter commit message"
    }
    
    Write-Host "💾 Quick commit: '$CommitMessage'" -ForegroundColor Green
    
    git add .
    git commit -m $CommitMessage
    
    Write-Host "✅ Committed successfully!" -ForegroundColor Green
}

function Save-Changes {
    param([string]$CommitMessage)
    
    if (-not $CommitMessage) {
        $CommitMessage = Read-Host "Enter commit message"
    }
    
    Write-Host "💾 Saving changes: '$CommitMessage'" -ForegroundColor Green
    
    # Show what will be committed
    Write-Host "`n📝 Files to be committed:" -ForegroundColor Yellow
    git diff --cached --name-only
    git diff --name-only
    
    $confirm = Read-Host "`nProceed with commit? (y/N)"
    if ($confirm -eq 'y' -or $confirm -eq 'Y') {
        git add .
        git commit -m $CommitMessage
        Write-Host "✅ Changes saved!" -ForegroundColor Green
    } else {
        Write-Host "❌ Commit cancelled" -ForegroundColor Red
    }
}

function Backup-Commit {
    $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm"
    $message = "BACKUP: Work in progress - $timestamp"
    
    Write-Host "💾 Creating backup commit..." -ForegroundColor Green
    git add .
    git commit -m $message
    Write-Host "✅ Backup created: $message" -ForegroundColor Green
}

function Checkpoint-Save {
    $timestamp = Get-Date -Format "HH:mm"
    $message = "CHECKPOINT: Save point at $timestamp"
    
    Write-Host "🎯 Creating checkpoint..." -ForegroundColor Green
    git add .
    git commit -m $message
    Write-Host "✅ Checkpoint created!" -ForegroundColor Green
}

function Create-FeatureBranch {
    param([string]$BranchName)
    
    if (-not $BranchName) {
        $BranchName = Read-Host "Enter feature branch name"
    }
    
    $fullBranchName = "feature/$BranchName"
    
    Write-Host "🌿 Creating feature branch: $fullBranchName" -ForegroundColor Green
    git checkout -b $fullBranchName
    Write-Host "✅ Branch created and switched!" -ForegroundColor Green
}

function Switch-Branch {
    param([string]$BranchName)
    
    if (-not $BranchName) {
        Write-Host "Available branches:" -ForegroundColor Yellow
        git branch
        $BranchName = Read-Host "`nEnter branch name to switch to"
    }
    
    Write-Host "🔄 Switching to branch: $BranchName" -ForegroundColor Green
    git checkout $BranchName
}

function Clean-Branches {
    Write-Host "🧹 Cleaning merged branches..." -ForegroundColor Yellow
    
    # Show branches that will be deleted
    $mergedBranches = git branch --merged | Where-Object { $_ -notmatch "\*|main|master|develop" }
    
    if ($mergedBranches) {
        Write-Host "`n🗑️ Branches to be deleted:" -ForegroundColor Red
        $mergedBranches | ForEach-Object { Write-Host "  $($_.Trim())" -ForegroundColor Yellow }
        
        $confirm = Read-Host "`nDelete these branches? (y/N)"
        if ($confirm -eq 'y' -or $confirm -eq 'Y') {
            $mergedBranches | ForEach-Object { git branch -d $_.Trim() }
            Write-Host "✅ Branches cleaned!" -ForegroundColor Green
        }
    } else {
        Write-Host "✅ No merged branches to clean" -ForegroundColor Green
    }
}

function Setup-UnityIgnore {
    Write-Host "🛡️ Setting up Unity .gitignore..." -ForegroundColor Green
    
    $gitignoreLines = @(
        "# Unity generated files",
        "Library/",
        "Temp/",
        "Obj/",
        "Build/",
        "Builds/",
        "Logs/",
        "UserSettings/",
        "",
        "# Unity Package Manager",
        "Packages/packages-lock.json",
        "",
        "# Visual Studio cache/options",
        ".vs/",
        "*.sln.docstates",
        "*.userprefs",
        "*.suo",
        "*.user",
        "*.userosscache",
        "*.sln.docstates.bak",
        "",
        "# Unity specific",
        "*.pidb",
        "*.booproj",
        "*.tmp",
        "*.unityproj",
        "*.unity.meta",
        "*.DS_Store",
        "*.app",
        "",
        "# Unity3D generated meta files",
        "*.pidb.meta",
        "*.pdb.meta",
        "*.mdb.meta",
        "",
        "# Unity3D generated file on crash reports",
        "sysinfo.txt",
        "",
        "# Builds",
        "*.apk",
        "*.aab",
        "*.unitypackage",
        "*.app",
        "",
        "# Crashlytics generated file",
        "crashlytics-build.properties"
    )
    
    $gitignoreLines | Out-File -FilePath ".gitignore" -Encoding UTF8
    Write-Host "✅ Unity .gitignore created!" -ForegroundColor Green
}

function Stash-Changes {
    $message = "WIP: Stashed at $(Get-Date -Format 'yyyy-MM-dd HH:mm')"
    Write-Host "📦 Stashing changes..." -ForegroundColor Yellow
    git stash push -m $message
    Write-Host "✅ Changes stashed!" -ForegroundColor Green
}

function Unstash-Changes {
    Write-Host "📦 Restoring stashed changes..." -ForegroundColor Yellow
    git stash pop
    Write-Host "✅ Changes restored!" -ForegroundColor Green
}

function Reset-SoftCommit {
    Write-Host "⚠️ Soft reset last commit..." -ForegroundColor Yellow
    
    # Show last commit
    Write-Host "`nLast commit:" -ForegroundColor Cyan
    git log -1 --oneline
    
    $confirm = Read-Host "`nUndo this commit but keep changes? (y/N)"
    if ($confirm -eq 'y' -or $confirm -eq 'Y') {
        git reset --soft HEAD~1
        Write-Host "✅ Commit undone, changes preserved!" -ForegroundColor Green
    } else {
        Write-Host "❌ Reset cancelled" -ForegroundColor Red
    }
}

# Main command dispatcher
switch ($Action.ToLower()) {
    "status" { Show-Status }
    "log" { Show-Log }
    "diff" { Show-Diff }
    "branches" { Show-Branches }
    "quick" { Quick-Commit $Message }
    "save" { Save-Changes $Message }
    "backup" { Backup-Commit }
    "checkpoint" { Checkpoint-Save }
    "feature" { Create-FeatureBranch $Branch }
    "switch" { Switch-Branch $Branch }
    "clean-branches" { Clean-Branches }
    "unity-ignore" { Setup-UnityIgnore }
    "stash" { Stash-Changes }
    "unstash" { Unstash-Changes }
    "reset-soft" { Reset-SoftCommit }
    default { Show-Help }
}

Pop-Location
