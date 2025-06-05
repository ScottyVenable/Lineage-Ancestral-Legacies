# Unity Real-Time Performance Monitor
# Displays live Unity performance data with UI cache monitoring

param(
    [int]$RefreshInterval = 5,
    [switch]$ShowAll,
    [switch]$AutoFix
)

function Write-ColorLine {
    param($Text, $Color = "White")
    Write-Host $Text -ForegroundColor $Color
}

function Get-FormattedSize {
    param($Bytes)
    if ($Bytes -eq $null -or $Bytes -eq 0) { return "0 B" }
    $sizes = @("B", "KB", "MB", "GB")
    $order = [Math]::Floor([Math]::Log($Bytes, 1024))
    $size = [Math]::Round($Bytes / [Math]::Pow(1024, $order), 2)
    return "$size $($sizes[$order])"
}

function Get-UnityProcessInfo {
    $unityProcesses = Get-Process Unity* -ErrorAction SilentlyContinue
    $info = @{
        MainUnity = $null
        UnityHub = @()
        Other = @()
        TotalCount = 0
    }
    
    foreach ($process in $unityProcesses) {
        if ($process.ProcessName -eq "Unity") {
            $info.MainUnity = $process
        } elseif ($process.ProcessName -eq "Unity Hub") {
            $info.UnityHub += $process
        } else {
            $info.Other += $process
        }
        $info.TotalCount++
    }
    
    return $info
}

function Get-UICacheInfo {
    $cacheInfo = @{
        UIElements = 0
        BurstCache = 0
        ShaderCache = 0
        TempArtifacts = 0
        Total = 0
    }
    
    # Check UIElements cache
    $uiElementsPath = "Library\UIElements"
    if (Test-Path $uiElementsPath) {
        $cacheInfo.UIElements = (Get-ChildItem $uiElementsPath -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
    }
    
    # Check Burst cache
    $burstCachePath = "Library\BurstCache"
    if (Test-Path $burstCachePath) {
        $cacheInfo.BurstCache = (Get-ChildItem $burstCachePath -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
    }
    
    # Check Shader cache
    $shaderCachePath = "Library\ShaderCache"
    if (Test-Path $shaderCachePath) {
        $cacheInfo.ShaderCache = (Get-ChildItem $shaderCachePath -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
    }
    
    # Check Temp artifacts
    $tempPath = "Temp"
    if (Test-Path $tempPath) {
        $cacheInfo.TempArtifacts = (Get-ChildItem $tempPath -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
    }
    
    $cacheInfo.Total = $cacheInfo.UIElements + $cacheInfo.BurstCache + $cacheInfo.ShaderCache + $cacheInfo.TempArtifacts
    
    return $cacheInfo
}

function Show-Header {
    Clear-Host
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-ColorLine "═══════════════════════════════════════════════════════════════════" "Cyan"
    Write-ColorLine "         UNITY REAL-TIME PERFORMANCE MONITOR" "Cyan"
    Write-ColorLine "═══════════════════════════════════════════════════════════════════" "Cyan"
    Write-ColorLine "Last Update: $timestamp | Refresh Interval: ${RefreshInterval}s" "Gray"
    Write-Host ""
}

function Show-UnityStatus {
    param($ProcessInfo)
    
    Write-ColorLine "🎮 UNITY EDITOR STATUS" "Green"
    Write-ColorLine "━━━━━━━━━━━━━━━━━━━━━━" "Gray"
    
    if ($ProcessInfo.MainUnity) {
        $process = $ProcessInfo.MainUnity
        $memoryMB = [Math]::Round($process.WorkingSet64 / 1MB, 1)
        $cpuPercent = [Math]::Round($process.CPU, 1)
        
        # Memory status color coding
        $memoryColor = "Green"
        if ($memoryMB -gt 2048) { $memoryColor = "Red" }
        elseif ($memoryMB -gt 1024) { $memoryColor = "Yellow" }
        
        Write-Host "Status: " -NoNewline; Write-ColorLine "RUNNING" "Green"
        Write-Host "PID: " -NoNewline; Write-ColorLine $process.Id "White"
        Write-Host "Memory: " -NoNewline; Write-ColorLine "${memoryMB} MB" $memoryColor
        Write-Host "CPU Time: " -NoNewline; Write-ColorLine "${cpuPercent}s" "White"
        Write-Host "Start Time: " -NoNewline; Write-ColorLine $process.StartTime.ToString("HH:mm:ss") "White"
        
        # Performance warnings
        if ($memoryMB -gt 2048) {
            Write-ColorLine "⚠️  High memory usage detected! Consider restarting Unity." "Yellow"
        }
    } else {
        Write-ColorLine "Status: NOT RUNNING" "Red"
        Write-ColorLine "💡 Start Unity Editor to begin monitoring" "Yellow"
    }
    
    # Unity Hub status
    if ($ProcessInfo.UnityHub.Count -gt 0) {
        $hubMemory = [Math]::Round(($ProcessInfo.UnityHub | Measure-Object WorkingSet64 -Sum).Sum / 1MB, 1)
        Write-Host "Unity Hub: " -NoNewline; Write-ColorLine "Running (${hubMemory} MB)" "Green"
    } else {
        Write-Host "Unity Hub: " -NoNewline; Write-ColorLine "Not Running" "Yellow"
    }
    
    Write-Host ""
}

function Show-UICacheStatus {
    param($CacheInfo)
    
    Write-ColorLine "📁 UI CACHE STATUS" "Green"
    Write-ColorLine "━━━━━━━━━━━━━━━━━━" "Gray"
    
    Write-Host "UIElements: " -NoNewline
    $uiSize = Get-FormattedSize $CacheInfo.UIElements
    $uiColor = if ($CacheInfo.UIElements -gt 100MB) { "Red" } elseif ($CacheInfo.UIElements -gt 50MB) { "Yellow" } else { "Green" }
    Write-ColorLine $uiSize $uiColor
    
    Write-Host "BurstCache: " -NoNewline; Write-ColorLine (Get-FormattedSize $CacheInfo.BurstCache) "White"
    Write-Host "ShaderCache: " -NoNewline; Write-ColorLine (Get-FormattedSize $CacheInfo.ShaderCache) "White"
    Write-Host "TempArtifacts: " -NoNewline; Write-ColorLine (Get-FormattedSize $CacheInfo.TempArtifacts) "White"
    Write-Host "Total Cache: " -NoNewline; Write-ColorLine (Get-FormattedSize $CacheInfo.Total) "Cyan"
    
    # Cache warnings
    if ($CacheInfo.UIElements -gt 100MB) {
        Write-ColorLine "⚠️  Large UI cache may impact performance!" "Yellow"
    }
    Write-Host ""
}

function Show-OtherProcesses {
    param($OtherProcesses)
    
    if ($ShowAll -and $OtherProcesses.Count -gt 0) {
        Write-ColorLine "🔧 OTHER UNITY PROCESSES" "Blue"
        Write-ColorLine "━━━━━━━━━━━━━━━━━━━━━━━━━" "Gray"
        
        foreach ($process in $OtherProcesses) {
            $memoryMB = [Math]::Round($process.WorkingSet64 / 1MB, 1)
            $cpuTime = [Math]::Round($process.CPU, 1)
            Write-Host "$($process.ProcessName) (PID $($process.Id)): ${memoryMB}MB, ${cpuTime}s CPU" -ForegroundColor "Gray"
        }
        Write-Host ""
    }
}

function Show-QuickCommands {
    Write-ColorLine "💡 QUICK COMMANDS" "Cyan"
    Write-ColorLine "━━━━━━━━━━━━━━━━━" "Gray"
    Write-ColorLine "Clear UI Cache: Remove-Item 'Library\UIElements\*' -Recurse -Force" "White"
    Write-ColorLine "Clear All Caches: .\unity-health-check.ps1 -AutoFix" "White"
    Write-ColorLine "Emergency Stop: Get-Process Unity | Stop-Process -Force" "Red"
    Write-Host ""
}

function Show-Controls {
    Write-ColorLine "⌨️  CONTROLS" "Magenta"
    Write-ColorLine "━━━━━━━━━━━━" "Gray"
    Write-ColorLine "Press Ctrl+C to stop monitoring" "White"
    if (-not $ShowAll) {
        Write-ColorLine "Use -ShowAll to see other Unity processes" "Gray"
    }
    if (-not $AutoFix) {
        Write-ColorLine "Use -AutoFix to enable automatic cache clearing" "Gray"
    }
    Write-Host ""
}

function Invoke-AutoFix {
    param($CacheInfo)
    
    if ($AutoFix -and $CacheInfo.UIElements -gt 100MB) {
        Write-ColorLine "🔧 AUTO-FIX: Clearing large UI cache..." "Yellow"
        try {
            Remove-Item "Library\UIElements\*" -Recurse -Force -ErrorAction SilentlyContinue
            Write-ColorLine "✅ UI cache cleared successfully" "Green"
        } catch {
            Write-ColorLine "❌ Failed to clear UI cache: $($_.Exception.Message)" "Red"
        }
        Write-Host ""
    }
}

function Main {
    try {
        while ($true) {
            # Gather data
            $processInfo = Get-UnityProcessInfo
            $cacheInfo = Get-UICacheInfo
            
            # Display information
            Show-Header
            Show-UnityStatus $processInfo
            Show-UICacheStatus $cacheInfo
            Show-OtherProcesses $processInfo.Other
            Show-QuickCommands
            Show-Controls
            
            # Auto-fix if enabled
            Invoke-AutoFix $cacheInfo
            
            # Wait for next refresh
            Start-Sleep -Seconds $RefreshInterval
        }
    } catch [System.Management.Automation.PipelineStoppedException] {
        Write-ColorLine "🛑 Monitoring stopped by user" "Red"
    } catch {
        Write-Host ""
        Write-ColorLine "❌ Error: $($_.Exception.Message)" "Red"
    }
}

# Run the monitor
Main
