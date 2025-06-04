# Unity Health Check & Cleanup Script
# Prevents and fixes common Unity performance issues

param(
    [switch]$AutoFix,
    [switch]$MonitorMode,
    [int]$MonitorInterval = 30
)

function Write-Status {
    param($Message, $Color = "White", $Status = "INFO")
    $timestamp = Get-Date -Format "HH:mm:ss"
    Write-Host "[$timestamp] [$Status] $Message" -ForegroundColor $Color
}

function Check-MultipleUnityInstances {
    $unityProcesses = Get-Process Unity -ErrorAction SilentlyContinue | Where-Object {$_.ProcessName -eq "Unity"}
    
    if ($unityProcesses.Count -gt 1) {
        Write-Status "⚠️  Multiple Unity Editor instances detected ($($unityProcesses.Count))" "Yellow" "WARN"
        
        foreach ($process in $unityProcesses) {
            $memoryMB = [math]::Round($process.WorkingSet/1MB, 2)
            Write-Status "   PID $($process.Id): Memory=${memoryMB}MB, CPU=$($process.CPU)s" "White"
        }
        
        if ($AutoFix) {
            Write-Status "🔧 Auto-fixing: Closing extra Unity instances..." "Yellow" "FIX"
            $sortedProcesses = $unityProcesses | Sort-Object CPU -Descending
            $sortedProcesses | Select-Object -First ($sortedProcesses.Count - 1) | ForEach-Object {
                Write-Status "   Stopping PID $($_.Id)" "Red" "FIX"
                Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
            }
            Start-Sleep -Seconds 2
            return $true
        }
        return $false
    } else {
        Write-Status "✅ Single Unity instance running" "Green" "OK"
        return $true
    }
}

function Check-UnityPerformance {
    $unityProcess = Get-Process Unity -ErrorAction SilentlyContinue | Where-Object {$_.ProcessName -eq "Unity"} | Select-Object -First 1
    
    if ($unityProcess) {
        $memoryMB = [math]::Round($unityProcess.WorkingSet/1MB, 2)
        $cpuTime = $unityProcess.CPU
        
        # Performance thresholds
        $memoryThreshold = 2000  # MB
        $cpuThreshold = 300      # seconds
        
        $memoryOk = $memoryMB -lt $memoryThreshold
        $cpuOk = $cpuTime -lt $cpuThreshold
        
        if ($memoryOk -and $cpuOk) {
            Write-Status "✅ Unity performance OK (Memory: ${memoryMB}MB, CPU: ${cpuTime}s)" "Green" "OK"
        } else {
            if (-not $memoryOk) {
                Write-Status "⚠️  High memory usage: ${memoryMB}MB (threshold: ${memoryThreshold}MB)" "Yellow" "WARN"
            }
            if (-not $cpuOk) {
                Write-Status "⚠️  High CPU usage: ${cpuTime}s (threshold: ${cpuThreshold}s)" "Yellow" "WARN"
            }
            
            if ($AutoFix) {
                Write-Status "🔧 Performance issue detected - clearing caches..." "Yellow" "FIX"
                Clear-UnityCaches
                return $true
            }
        }
        return $memoryOk -and $cpuOk
    } else {
        Write-Status "ℹ️  No Unity Editor running" "Gray" "INFO"
        return $true
    }
}

function Clear-UnityCaches {
    Write-Status "🧹 Clearing Unity caches..." "Cyan" "CLEAN"
    
    $cacheTypes = @("UIElements", "BurstCache", "ShaderCache", "TempArtifacts")
    $totalCleared = 0
    
    foreach ($cache in $cacheTypes) {
        $cachePath = "Library\$cache"
        if (Test-Path $cachePath) {
            try {
                $size = (Get-ChildItem $cachePath -Recurse -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
                $sizeMB = [math]::Round($size/1MB, 2)
                Remove-Item $cachePath -Recurse -Force -ErrorAction SilentlyContinue
                Write-Status "   ✓ Cleared $cache (${sizeMB}MB)" "Green" "CLEAN"
                $totalCleared += $sizeMB
            } catch {
                Write-Status "   ✗ Failed to clear $cache" "Red" "ERROR"
            }
        }
    }
    
    Write-Status "🎯 Total cache cleared: ${totalCleared}MB" "Green" "CLEAN"
}

function Check-UnityHubProcesses {
    $hubProcesses = Get-Process "Unity Hub" -ErrorAction SilentlyContinue
    
    if ($hubProcesses.Count -gt 3) {
        Write-Status "⚠️  Many Unity Hub processes running ($($hubProcesses.Count))" "Yellow" "WARN"
        
        if ($AutoFix) {
            Write-Status "🔧 Cleaning up excess Unity Hub processes..." "Yellow" "FIX"
            $hubProcesses | Sort-Object CPU -Descending | Select-Object -First ($hubProcesses.Count - 2) | ForEach-Object {
                Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
                Write-Status "   Stopped Unity Hub PID $($_.Id)" "Yellow" "FIX"
            }
            return $true
        }
        return $false
    } else {
        Write-Status "✅ Unity Hub processes normal ($($hubProcesses.Count))" "Green" "OK"
        return $true
    }
}

function Run-HealthCheck {
    Write-Status "🏥 Starting Unity Health Check..." "Cyan" "START"
    Write-Status "=" * 50 "Gray"
    
    $issues = 0
    
    # Check for multiple Unity instances
    if (-not (Check-MultipleUnityInstances)) { $issues++ }
    
    # Check Unity performance
    if (-not (Check-UnityPerformance)) { $issues++ }
    
    # Check Unity Hub processes
    if (-not (Check-UnityHubProcesses)) { $issues++ }
    
    Write-Status "=" * 50 "Gray"
    
    if ($issues -eq 0) {
        Write-Status "🎉 Unity health check complete - no issues found!" "Green" "DONE"
    } else {
        Write-Status "⚠️  Health check found $issues issue(s)" "Yellow" "DONE"
        if (-not $AutoFix) {
            Write-Status "💡 Run with -AutoFix to automatically resolve issues" "Cyan" "TIP"
        }
    }
    
    return $issues -eq 0
}

# Main execution
if ($MonitorMode) {
    Write-Status "👁️  Starting Unity monitor mode (interval: ${MonitorInterval}s)" "Cyan" "MONITOR"
    Write-Status "Press Ctrl+C to stop monitoring" "Yellow" "MONITOR"
    
    try {
        while ($true) {
            Clear-Host
            Run-HealthCheck
            Start-Sleep -Seconds $MonitorInterval
        }
    } catch {
        Write-Status "🛑 Monitoring stopped" "Red" "STOP"
    }
} else {
    Run-HealthCheck
}
