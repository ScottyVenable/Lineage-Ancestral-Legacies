# Unity Real-Time Performance Monitor
# Displays live Unity performance data with UI cache monitoring

param(
    [int]$RefreshInterval = 5,
    [switch]$ShowAll,
    [switch]$AutoFix
)

function Write-ColorLine {
    param($Text, $C        Write-ColorLine "❌ Error: $($_.Exception.Message)" "Red"lor = "White")
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
        $info.TotalCount++
        
        if ($process.ProcessName -eq "Unity") {
            $info.MainUnity = $process
        } elseif ($process.ProcessName -eq "Unity Hub") {
            $info.UnityHub += $process
        } else {
            $info.Other += $process
        }
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
    
    $cachePaths = @{
        UIElements = "Library\UIElements"
        BurstCache = "Library\BurstCache"
        ShaderCache = "Library\ShaderCache"
        TempArtifacts = "Library\TempArtifacts"
    }
    
    foreach ($cache in $cachePaths.Keys) {
        $path = $cachePaths[$cache]
        if (Test-Path $path) {
            try {
                $size = (Get-ChildItem $path -Recurse -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
                $cacheInfo[$cache] = $size
                $cacheInfo.Total += $size
            } catch {
                # Ignore errors for locked files
            }
        }
    }
    
    return $cacheInfo
}

function Show-Header {
    param($Timestamp)
    Clear-Host
    Write-ColorLine "╔══════════════════════════════════════════════════════════════════════════════╗" "Cyan"
    Write-ColorLine "║                         UNITY REAL-TIME PERFORMANCE MONITOR                  ║" "Cyan"
    Write-ColorLine "╠══════════════════════════════════════════════════════════════════════════════╣" "Cyan"
    Write-ColorLine "║ Refresh: ${RefreshInterval}s | Last Update: $Timestamp | Press Ctrl+C to stop                ║" "White"
    Write-ColorLine "╚══════════════════════════════════════════════════════════════════════════════╝" "Cyan"
    Write-Host ""
}

function Show-UnityStatus {
    param($UnityInfo)
    
    Write-ColorLine "🎮 UNITY EDITOR STATUS" "Yellow"
    Write-ColorLine "━━━━━━━━━━━━━━━━━━━━━━━━" "Gray"
    
    if ($UnityInfo.MainUnity) {
        $process = $UnityInfo.MainUnity
        $memoryMB = [Math]::Round($process.WorkingSet64 / 1MB, 1)
        $cpuTime = [Math]::Round($process.CPU, 1)
        
        # Color coding based on performance
        $cpuColor = "Green"
        $memoryColor = "Green"
        
        if ($cpuTime -gt 200) { $cpuColor = "Yellow" }
        if ($cpuTime -gt 400) { $cpuColor = "Red" }
        if ($memoryMB -gt 2000) { $memoryColor = "Yellow" }
        if ($memoryMB -gt 4000) { $memoryColor = "Red" }
        
        Write-Host "Status: " -NoNewline; Write-ColorLine "RUNNING ✅" "Green"
        Write-Host "PID: " -NoNewline; Write-ColorLine $process.Id "White"
        Write-Host "CPU Time: " -NoNewline; Write-ColorLine "${cpuTime}s" $cpuColor
        Write-Host "Memory: " -NoNewline; Write-ColorLine "${memoryMB} MB" $memoryColor
        Write-Host "Threads: " -NoNewline; Write-ColorLine $process.Threads.Count "White"
        
        # Performance warnings
        if ($cpuTime -gt 300) {
            Write-ColorLine "⚠️  HIGH CPU USAGE DETECTED!" "Red"
        }
        if ($memoryMB -gt 3000) {
            Write-ColorLine "⚠️  HIGH MEMORY USAGE DETECTED!" "Red"
        }
    } else {
        Write-ColorLine "Status: NOT RUNNING ❌" "Red"
    }
    Write-Host ""
}

function Show-UnityHubStatus {
    param($HubProcesses)
    
    Write-ColorLine "🏢 UNITY HUB PROCESSES" "Magenta"
    Write-ColorLine "━━━━━━━━━━━━━━━━━━━━━━━" "Gray"
    
    if ($HubProcesses.Count -eq 0) {
        Write-ColorLine "No Unity Hub processes running" "Gray"
    } else {
        Write-ColorLine "Count: $($HubProcesses.Count)" "White"
        
        if ($HubProcesses.Count -gt 3) {
            Write-ColorLine "⚠️  Too many Unity Hub processes!" "Yellow"
        }
        
        if ($ShowAll) {
            foreach ($hub in $HubProcesses) {
                $memoryMB = [Math]::Round($hub.WorkingSet64 / 1MB, 1)
                $cpuTime = [Math]::Round($hub.CPU, 1)
                Write-Host "  PID $($hub.Id): ${memoryMB}MB, ${cpuTime}s CPU" -ForegroundColor "Gray"
            }
        }
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

function Main {
    Write-ColorLine "Starting Unity Real-Time Monitor..." "Cyan"
    Write-ColorLine "Press Ctrl+C to stop monitoring" "Yellow"
    Start-Sleep 2
    
    try {
        while ($true) {
            $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
            
            # Gather data
            $unityInfo = Get-UnityProcessInfo
            $cacheInfo = Get-UICacheInfo
            
            # Display everything
            Show-Header $timestamp
            Show-UnityStatus $unityInfo
            Show-UnityHubStatus $unityInfo.UnityHub
            Show-UICacheStatus $cacheInfo
            Show-OtherProcesses $unityInfo.Other
            Show-QuickCommands
            
            # Auto-fix if enabled and issues detected
            if ($AutoFix) {
                if ($unityInfo.MainUnity -and $unityInfo.MainUnity.CPU -gt 300) {
                    Write-ColorLine "🔧 AUTO-FIX: High CPU detected, clearing caches..." "Yellow"
                    & ".\unity-health-check.ps1" -AutoFix | Out-Host
                }
                if ($cacheInfo.UIElements -gt 100MB) {
                    Write-ColorLine "🔧 AUTO-FIX: Large UI cache detected, clearing..." "Yellow"
                    Remove-Item 'Library\UIElements\*' -Recurse -Force -ErrorAction SilentlyContinue
                }
            }
            
            Start-Sleep $RefreshInterval
        }
    } catch [System.Management.Automation.PipelineStoppedException] {
        Write-Host ""
        Write-ColorLine "🛑 Monitoring stopped by user" "Red"
    } catch {
        Write-Host ""
        Write-ColorLine "❌ Error: $($_.Exception.Message)" "Red"
    }
}

# Run the monitor
Main
