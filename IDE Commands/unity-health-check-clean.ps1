param(
    [switch]$AutoFix,
    [switch]$Monitor,
    [int]$MonitorInterval = 30,
    [switch]$Help
)

if ($Help) {
    Write-Host @"
Unity Health Check Tool
======================

This script performs comprehensive health checks on Unity and related processes.

Parameters:
  -AutoFix            : Automatically fix detected issues
  -Monitor            : Run continuous monitoring
  -MonitorInterval    : Monitoring interval in seconds (default: 30)
  -Help               : Show this help message

Examples:
  .\unity-health-check.ps1                    # Run basic health check
  .\unity-health-check.ps1 -AutoFix           # Run with auto-fix enabled
  .\unity-health-check.ps1 -Monitor           # Run continuous monitoring
  .\unity-health-check.ps1 -Monitor -MonitorInterval 60  # Monitor every 60 seconds

"@
    return
}

function Write-Status {
    param([string]$Message, [string]$Color = "White", [string]$Prefix = "INFO")
    
    $timestamp = Get-Date -Format "HH:mm:ss"
    switch($Color) {
        "Green" { Write-Host "[$timestamp][$Prefix] $Message" -ForegroundColor Green }
        "Red" { Write-Host "[$timestamp][$Prefix] $Message" -ForegroundColor Red }
        "Yellow" { Write-Host "[$timestamp][$Prefix] $Message" -ForegroundColor Yellow }
        "Cyan" { Write-Host "[$timestamp][$Prefix] $Message" -ForegroundColor Cyan }
        default { Write-Host "[$timestamp][$Prefix] $Message" -ForegroundColor White }
    }
}

function Test-MultipleUnityInstances {
    $unityProcesses = Get-Process Unity -ErrorAction SilentlyContinue | Where-Object {$_.ProcessName -eq "Unity"}
    
    if ($unityProcesses.Count -gt 1) {
        Write-Status "Multiple Unity Editor instances detected ($($unityProcesses.Count))" "Yellow" "WARN"
        
        foreach ($process in $unityProcesses) {
            $memoryMB = [math]::Round($process.WorkingSet/1MB, 2)
            Write-Status "   PID $($process.Id): Memory=${memoryMB}MB, CPU=$($process.CPU)s" "White"
        }
        
        if ($AutoFix) {
            Write-Status "Auto-fixing: Closing extra Unity instances..." "Yellow" "FIX"
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
        Write-Status "Single Unity instance running" "Green" "OK"
        return $true
    }
}

function Test-UnityPerformance {
    $unityProcess = Get-Process Unity -ErrorAction SilentlyContinue | Where-Object {$_.ProcessName -eq "Unity"} | Select-Object -First 1
    
    if ($unityProcess) {
        $memoryMB = [math]::Round($unityProcess.WorkingSet/1MB, 2)
        $cpuPercent = [math]::Round($unityProcess.CPU, 2)
        
        Write-Status "Unity Performance Check:" "Cyan" "PERF"
        Write-Status "   Memory Usage: ${memoryMB} MB" "White"
        Write-Status "   CPU Time: ${cpuPercent}s" "White"
        
        if ($memoryMB -gt 2048) {
            Write-Status "   High memory usage detected (>2GB)" "Yellow" "WARN"
            if ($AutoFix) {
                Write-Status "   Consider restarting Unity Editor" "Yellow" "SUGGEST"
            }
            return $false
        } else {
            Write-Status "   Memory usage within normal range" "Green" "OK"
            return $true
        }
    } else {
        Write-Status "Unity Editor not running" "Yellow" "WARN"
        return $false
    }
}

function Test-UnityHubProcesses {
    $hubProcesses = Get-Process "Unity Hub" -ErrorAction SilentlyContinue
    
    if ($hubProcesses) {
        Write-Status "Unity Hub is running" "Green" "OK"
        foreach ($process in $hubProcesses) {
            $memoryMB = [math]::Round($process.WorkingSet/1MB, 2)
            Write-Status "   PID $($process.Id): Memory=${memoryMB}MB" "White"
        }
        return $true
    } else {
        Write-Status "Unity Hub not running" "Yellow" "WARN"
        return $false
    }
}

function Clear-UnityCaches {
    Write-Status "Checking Unity cache directories..." "Cyan" "CACHE"
    
    $cacheLocations = @(
        "$env:LOCALAPPDATA\Unity\cache",
        "$env:APPDATA\Unity\cache",
        "$env:TEMP\Unity"
    )
    
    $totalCleared = 0
    
    foreach ($cache in $cacheLocations) {
        $cachePath = $cache
        Write-Status "Checking cache: $cache" "White"
        
        if (Test-Path $cachePath) {
            try {
                $size = (Get-ChildItem $cachePath -Recurse -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
                $sizeMB = [math]::Round($size/1MB, 2)
                Remove-Item $cachePath -Recurse -Force -ErrorAction SilentlyContinue
                Write-Status "   [OK] Cleared $cache (${sizeMB} MB)" "Green" "CLEAN"
                $totalCleared += $sizeMB
            } catch {
                Write-Status "   [ERROR] Failed to clear $cache" "Red" "ERROR"
            }
        } else {
            Write-Status "   [SKIP] Cache not found: $cache" "White"
        }
    }
    
    Write-Status "[SUCCESS] Total cache cleared: $totalCleared MB" "Green" "CLEAN"
}

function Invoke-HealthCheck {
    Write-Status "Starting Unity Health Check..." "Cyan" "START"
    Write-Status "==============================" "Cyan"
    
    $allPassed = $true
    
    # Check multiple Unity instances
    if (-not (Test-MultipleUnityInstances)) {
        $allPassed = $false
    }
    
    # Check Unity performance
    if (-not (Test-UnityPerformance)) {
        $allPassed = $false
    }
    
    # Check Unity Hub
    if (-not (Test-UnityHubProcesses)) {
        $allPassed = $false
    }
    
    # Clear caches if AutoFix is enabled
    if ($AutoFix) {
        Clear-UnityCaches
    }
    
    Write-Status "==============================" "Cyan"
    if ($allPassed) {
        Write-Status "Health Check PASSED - All systems OK" "Green" "PASS"
    } else {
        Write-Status "Health Check FAILED - Issues detected" "Red" "FAIL"
        if (-not $AutoFix) {
            Write-Status "Use -AutoFix parameter to automatically resolve issues" "Yellow" "HINT"
        }
    }
    
    return $allPassed
}

# Main execution
if ($Monitor) {
    Write-Status "Starting continuous monitoring (Interval: ${MonitorInterval}s)" "Cyan" "MONITOR"
    Write-Status "Press Ctrl+C to stop monitoring" "Yellow"
    
    try {
        while ($true) {
            Invoke-HealthCheck
            Start-Sleep -Seconds $MonitorInterval
        }
    } catch {
        Write-Status "[STOP] Monitoring stopped" "Red" "STOP"
    }
} else {
    Invoke-HealthCheck
}
