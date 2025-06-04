# Unity UI Performance Monitor - Simple Version
# Monitors and fixes UI-related performance issues

param(
    [switch]$MonitorMode,
    [switch]$AutoFix,
    [int]$CheckInterval = 15
)

function Write-Status {
    param($Message, $Color = "White")
    $timestamp = Get-Date -Format "HH:mm:ss"
    Write-Host "[$timestamp] $Message" -ForegroundColor $Color
}

function Check-UnityUIPerformance {
    Write-Status "=== UI Performance Check ===" "Cyan"
    
    # Check for multiple Unity instances
    $unityProcs = Get-Process Unity -ErrorAction SilentlyContinue | Where-Object {$_.ProcessName -eq "Unity"}
    
    if ($unityProcs.Count -eq 0) {
        Write-Status "No Unity Editor running" "Gray"
        return $true
    }
    
    if ($unityProcs.Count -gt 1) {
        Write-Status "WARNING: Multiple Unity instances detected ($($unityProcs.Count))" "Red"
        Write-Status "This causes major UI performance issues!" "Yellow"
        
        foreach ($proc in $unityProcs) {
            $memMB = [math]::Round($proc.WorkingSet/1MB, 2)
            Write-Status "  PID $($proc.Id): Memory=${memMB}MB, CPU=$($proc.CPU)s" "White"
        }
        
        if ($AutoFix) {
            Write-Status "Auto-fixing: Closing problematic Unity instances..." "Yellow"
            $sorted = $unityProcs | Sort-Object CPU -Descending
            $toClose = $sorted | Select-Object -First ($sorted.Count - 1)
            
            foreach ($proc in $toClose) {
                Write-Status "  Stopping PID $($proc.Id)" "Red"
                Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
            }
            Start-Sleep -Seconds 3
        }
        return $false
    }
    
    # Check single Unity performance
    $unity = $unityProcs[0]
    $memMB = [math]::Round($unity.WorkingSet/1MB, 2)
    $cpuTime = $unity.CPU
    
    if ($cpuTime -gt 150 -or $memMB -gt 1500) {
        Write-Status "PERFORMANCE ISSUE: Memory=${memMB}MB, CPU=${cpuTime}s" "Red"
        
        if ($AutoFix) {
            Write-Status "Auto-fixing: Clearing UI caches..." "Yellow"
            Clear-UICaches
        }
        return $false
    } else {
        Write-Status "Unity performance OK: Memory=${memMB}MB, CPU=${cpuTime}s" "Green"
        return $true
    }
}

function Clear-UICaches {
    Write-Status "Clearing UI-related caches..." "Cyan"
    
    $caches = @("UIElements", "UIBuilder", "ShaderCache", "TempArtifacts")
    $totalCleared = 0
    
    foreach ($cache in $caches) {
        $path = "Library\$cache"
        if (Test-Path $path) {
            $size = (Get-ChildItem $path -Recurse -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
            $sizeMB = [math]::Round($size/1MB, 2)
            Remove-Item $path -Recurse -Force -ErrorAction SilentlyContinue
            Write-Status "  Cleared $cache (${sizeMB}MB)" "Green"
            $totalCleared += $sizeMB
        }
    }
    
    Write-Status "Total cache cleared: ${totalCleared}MB" "Green"
}

function Show-UITips {
    Write-Status "UI Performance Tips:" "Cyan"
    Write-Status "  • Avoid extreme 9-slice sprite scaling" "White"
    Write-Status "  • Use separate canvases for static/dynamic UI" "White"
    Write-Status "  • Disable raycastTarget on decorative UI elements" "White"
    Write-Status "  • Group UI changes to minimize Canvas rebuilds" "White"
}

# Main execution
if ($MonitorMode) {
    Write-Status "Starting UI Performance Monitor (${CheckInterval}s intervals)" "Cyan"
    Write-Status "Press Ctrl+C to stop" "Yellow"
    
    try {
        while ($true) {
            Clear-Host
            $isOk = Check-UnityUIPerformance
            
            if (-not $isOk) {
                Write-Host ""
                Show-UITips
            }
            
            Write-Host ""
            Write-Status "Next check in ${CheckInterval} seconds..." "Gray"
            Start-Sleep -Seconds $CheckInterval
        }
    } catch {
        Write-Status "Monitoring stopped" "Red"
    }
} else {
    Check-UnityUIPerformance
    Write-Host ""
    Show-UITips
    Write-Host ""
    Write-Status "Run with -MonitorMode to continuously monitor" "Cyan"
    Write-Status "Use -AutoFix to automatically resolve issues" "Cyan"
}
