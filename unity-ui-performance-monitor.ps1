# Unity UI Performance Monitor
# Specialized monitoring for UI-related performance issues

param(
    [switch]$MonitorMode,
    [int]$CheckInterval = 10,
    [switch]$AutoClearCache,
    [int]$CPUThreshold = 200,
    [int]$MemoryThreshold = 1500
)

function Write-UIStatus {
    param($Message, $Color = "White", $Icon = "ℹ️")
    $timestamp = Get-Date -Format "HH:mm:ss"
    Write-Host "[$timestamp] $Icon $Message" -ForegroundColor $Color
}

function Check-UIPerformanceIssues {
    $unityProcesses = Get-Process Unity -ErrorAction SilentlyContinue | Where-Object {$_.ProcessName -eq "Unity"}
    
    if ($unityProcesses.Count -eq 0) {
        Write-UIStatus "No Unity Editor running" "Gray" "⭕"
        return $true
    }
    
    if ($unityProcesses.Count -gt 1) {
        Write-UIStatus "⚠️ MULTIPLE UNITY INSTANCES DETECTED ($($unityProcesses.Count))" "Red" "🚨"
        Write-UIStatus "This often causes UI performance issues!" "Yellow" "⚠️"
        
        foreach ($process in $unityProcesses) {
            $memoryMB = [math]::Round($process.WorkingSet/1MB, 2)
            Write-UIStatus "   PID $($process.Id): Memory=${memoryMB}MB, CPU=$($process.CPU)s" "White" "   "
        }
        
        # Auto-close problematic instances
        Write-UIStatus "🔧 Auto-closing excess Unity instances..." "Yellow" "🔧"
        $sorted = $unityProcesses | Sort-Object CPU -Descending
        $toClose = $sorted | Select-Object -First ($sorted.Count - 1)
        
        foreach ($proc in $toClose) {
            Write-UIStatus "   Stopping PID $($proc.Id) (CPU: $($proc.CPU)s)" "Red" "🔴"
            Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
        }
        
        Start-Sleep -Seconds 2
        return $false
    }
    
    # Check single Unity instance performance
    $unity = $unityProcesses[0]
    $memoryMB = [math]::Round($unity.WorkingSet/1MB, 2)
    $cpuTime = $unity.CPU
    
    $cpuIssue = $cpuTime -gt $CPUThreshold
    $memoryIssue = $memoryMB -gt $MemoryThreshold
    
    if ($cpuIssue -or $memoryIssue) {
        Write-UIStatus "🔥 PERFORMANCE ISSUE DETECTED!" "Red" "🔥"
        
        if ($cpuIssue) {
            Write-UIStatus "   High CPU usage: ${cpuTime}s (threshold: ${CPUThreshold}s)" "Yellow" "⚠️"
        }
        if ($memoryIssue) {
            Write-UIStatus "   High memory usage: ${memoryMB}MB (threshold: ${MemoryThreshold}MB)" "Yellow" "⚠️"
        }
        
        if ($AutoClearCache) {
            Write-UIStatus "🧹 Auto-clearing UI caches..." "Cyan" "🧹"
            Clear-UISpecificCaches
        } else {
            Write-UIStatus "💡 Consider running with -AutoClearCache to fix automatically" "Cyan" "💡"
        }
        
        return $false
    } else {
        Write-UIStatus "✅ Unity performance OK (Memory: ${memoryMB}MB, CPU: ${cpuTime}s)" "Green" "✅"
        return $true
    }
}

function Clear-UISpecificCaches {
    $cacheTypes = @(
        @{Name="UIElements"; Path="Library\UIElements"; Description="UI rendering cache"},
        @{Name="UIBuilder"; Path="Library\UIBuilder"; Description="UI Builder cache"},
        @{Name="ShaderCache"; Path="Library\ShaderCache"; Description="UI shader cache"},
        @{Name="TempArtifacts"; Path="Library\TempArtifacts"; Description="Temporary UI artifacts"}
    )
    
    $totalCleared = 0
    
    foreach ($cache in $cacheTypes) {
        if (Test-Path $cache.Path) {
            try {
                $size = (Get-ChildItem $cache.Path -Recurse -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
                $sizeMB = [math]::Round($size/1MB, 2)
                Remove-Item $cache.Path -Recurse -Force -ErrorAction SilentlyContinue
                Write-UIStatus "   ✓ Cleared $($cache.Name) (${sizeMB}MB) - $($cache.Description)" "Green" "   "
                $totalCleared += $sizeMB
            } catch {
                Write-UIStatus "   ✗ Failed to clear $($cache.Name)" "Red" "   "
            }
        } else {
            Write-UIStatus "   ○ $($cache.Name) already clear" "Gray" "   "
        }
    }
    
    Write-UIStatus "🎯 Total UI cache cleared: ${totalCleared}MB" "Green" "🎯"
}

function Monitor-UICanvasElements {
    Write-UIStatus "🎨 Checking for problematic UI elements in project..." "Cyan" "🎨"
    
    # Check for large UI assets that might cause performance issues
    $uiAssets = Get-ChildItem -Path "Assets" -Recurse -Include "*.prefab", "*.unity" -ErrorAction SilentlyContinue | 
                Where-Object { $_.Length -gt 1MB }
    
    if ($uiAssets) {
        Write-UIStatus "⚠️ Large UI-related files found:" "Yellow" "⚠️"
        foreach ($asset in $uiAssets) {
            $sizeMB = [math]::Round($asset.Length/1MB, 2)
            Write-UIStatus "   $($asset.Name): ${sizeMB}MB" "White" "   "
        }
        Write-UIStatus "💡 Large UI files can cause performance issues" "Cyan" "💡"
    } else {
        Write-UIStatus "✅ No problematically large UI files found" "Green" "✅"
    }
}

function Show-UIPerformanceTips {
    Write-UIStatus "💡 UI Performance Tips:" "Cyan" "💡"
    Write-UIStatus "   • Avoid scaling 9-slice sprites to extreme sizes" "White" "   "
    Write-UIStatus "   • Use UI.Graphic.raycastTarget = false for non-interactive elements" "White" "   "
    Write-UIStatus "   • Minimize Canvas rebuilds by grouping UI changes" "White" "   "
    Write-UIStatus "   • Use separate canvases for static vs dynamic UI" "White" "   "
    Write-UIStatus "   • Clear UI caches when experiencing slowdowns" "White" "   "
}

# Main execution
if ($MonitorMode) {
    Write-UIStatus "👁️ Starting UI Performance Monitor (interval: ${CheckInterval}s)" "Cyan" "👁️"
    Write-UIStatus "Press Ctrl+C to stop monitoring" "Yellow" "⚠️"
    Write-UIStatus "Thresholds: CPU > ${CPUThreshold}s, Memory > ${MemoryThreshold}MB" "Gray" "⚙️"
    
    try {
        while ($true) {
            Clear-Host
            Write-UIStatus "=== UNITY UI PERFORMANCE MONITOR ===" "Cyan" "🎨"
            Write-Host ""
            
            $performanceOk = Check-UIPerformanceIssues
            
            if (-not $performanceOk) {
                Write-Host ""
                Show-UIPerformanceTips
            }
            
            Write-Host ""
            Write-UIStatus "Next check in ${CheckInterval} seconds..." "Gray" "⏱️"
            Start-Sleep -Seconds $CheckInterval
        }
    } catch {
        Write-UIStatus "🛑 Monitoring stopped" "Red" "🛑"
    }
} else {
    Write-UIStatus "=== UNITY UI PERFORMANCE CHECK ===" "Cyan" "🎨"
    Write-Host ""
    
    Check-UIPerformanceIssues
    Monitor-UICanvasElements
    
    Write-Host ""
    Show-UIPerformanceTips
    
    Write-Host ""
    Write-UIStatus "💡 Run with -MonitorMode to continuously monitor UI performance" "Cyan" "💡"
    Write-UIStatus "💡 Use -AutoClearCache to automatically clear caches when issues are detected" "Cyan" "💡"
}
