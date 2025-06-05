# Unity UI Performance Monitor
# Specialized monitoring for Unity UI elements and rendering performance

param(
    [switch]$AutoOptimize,
    [switch]$ClearCaches,
    [switch]$Help
)

if ($Help) {
    Write-Host @"
Unity UI Performance Monitor
===========================

Monitors Unity UI performance and provides optimization suggestions.

Parameters:
  -AutoOptimize  : Automatically apply performance optimizations
  -ClearCaches   : Clear UI-related cache files
  -Help          : Show this help message

Examples:
  .\unity-ui-performance-monitor.ps1                  # Basic monitoring
  .\unity-ui-performance-monitor.ps1 -AutoOptimize   # Monitor with auto-optimization
  .\unity-ui-performance-monitor.ps1 -ClearCaches    # Clear UI caches

"@
    return
}

function Write-UIStatus {
    param($Message, $Color = "White", $Icon = "ℹ️")
    $timestamp = Get-Date -Format "HH:mm:ss"
    Write-Host "[$timestamp] $Icon $Message" -ForegroundColor $Color
}

function Test-UIPerformanceIssues {
    $unityProcesses = Get-Process Unity -ErrorAction SilentlyContinue | Where-Object {$_.ProcessName -eq "Unity"}
    
    if ($unityProcesses.Count -eq 0) {
        Write-UIStatus "Unity Editor not running" "Yellow" "⚠️"
        return $false
    }
    
    $mainUnity = $unityProcesses | Select-Object -First 1
    $memoryMB = [Math]::Round($mainUnity.WorkingSet64 / 1MB, 2)
    
    Write-UIStatus "Unity Editor Status:" "Cyan" "🎮"
    Write-UIStatus "  Memory Usage: ${memoryMB} MB" "White"
    Write-UIStatus "  Process ID: $($mainUnity.Id)" "White"
    
    # Check for high memory usage
    if ($memoryMB -gt 2048) {
        Write-UIStatus "  HIGH MEMORY USAGE detected (>${memoryMB}MB)" "Red" "⚠️"
        return $false
    } elseif ($memoryMB -gt 1024) {
        Write-UIStatus "  Moderate memory usage (${memoryMB}MB)" "Yellow" "⚠️"
        return $true
    } else {
        Write-UIStatus "  Memory usage normal (${memoryMB}MB)" "Green" "✅"
        return $true
    }
}

function Get-UICacheSize {
    $cacheTypes = @(
        @{ Name = "UIElements"; Path = "Library\UIElements"; Size = 0 },
        @{ Name = "ShaderCache"; Path = "Library\ShaderCache"; Size = 0 },
        @{ Name = "BurstCache"; Path = "Library\BurstCache"; Size = 0 },
        @{ Name = "TempArtifacts"; Path = "Temp"; Size = 0 }
    )
    
    Write-UIStatus "Checking UI cache sizes..." "Cyan" "📁"
    
    foreach ($cache in $cacheTypes) {
        if (Test-Path $cache.Path) {
            try {
                $size = (Get-ChildItem $cache.Path -Recurse -File -ErrorAction SilentlyContinue | 
                        Measure-Object -Property Length -Sum).Sum
                $cache.Size = $size
                $sizeMB = [Math]::Round($size / 1MB, 2)
                
                $color = "Green"
                if ($sizeMB -gt 100) { $color = "Red" }
                elseif ($sizeMB -gt 50) { $color = "Yellow" }
                
                Write-UIStatus "  $($cache.Name): ${sizeMB} MB" $color
            } catch {
                Write-UIStatus "  $($cache.Name): Unable to calculate size" "Yellow" "⚠️"
            }
        } else {
            Write-UIStatus "  $($cache.Name): Not found" "Gray"
        }
    }
    
    return $cacheTypes
}

function Clear-UICaches {
    param($CacheTypes)
    
    Write-UIStatus "Clearing UI-related caches..." "Yellow" "🧹"
    
    foreach ($cache in $CacheTypes) {
        if (Test-Path $cache.Path) {
            try {
                $sizeMB = [Math]::Round($cache.Size / 1MB, 2)
                Remove-Item $cache.Path -Recurse -Force -ErrorAction SilentlyContinue
                Write-UIStatus "  ✅ Cleared $($cache.Name) (${sizeMB}MB)" "Green"
            } catch {
                Write-UIStatus "  ❌ Failed to clear $($cache.Name): $($_.Exception.Message)" "Red"
            }
        }
    }
}

function Test-UIElementsPerformance {
    Write-UIStatus "Checking UIElements performance..." "Cyan" "🎨"
    
    # Check for UIElements cache
    $uiElementsPath = "Library\UIElements"
    if (Test-Path $uiElementsPath) {
        $uiSize = (Get-ChildItem $uiElementsPath -Recurse -File -ErrorAction SilentlyContinue | 
                  Measure-Object -Property Length -Sum).Sum
        $uiSizeMB = [Math]::Round($uiSize / 1MB, 2)
        
        if ($uiSizeMB -gt 100) {
            Write-UIStatus "  Large UIElements cache detected (${uiSizeMB}MB)" "Red" "⚠️"
            Write-UIStatus "  This may cause UI performance issues" "Yellow" "💡"
            return $false
        } elseif ($uiSizeMB -gt 50) {
            Write-UIStatus "  Moderate UIElements cache (${uiSizeMB}MB)" "Yellow" "⚠️"
            return $true
        } else {
            Write-UIStatus "  UIElements cache size normal (${uiSizeMB}MB)" "Green" "✅"
            return $true
        }
    } else {
        Write-UIStatus "  UIElements cache not found" "Gray"
        return $true
    }
}

function Show-OptimizationSuggestions {
    Write-UIStatus "Performance Optimization Suggestions:" "Cyan" "💡"
    Write-UIStatus "  • Clear UIElements cache regularly" "White"
    Write-UIStatus "  • Avoid deep UI hierarchy nesting" "White"
    Write-UIStatus "  • Use UI pooling for dynamic elements" "White"
    Write-UIStatus "  • Optimize UI texture sizes" "White"
    Write-UIStatus "  • Consider Canvas optimization" "White"
}

function Invoke-AutoOptimization {
    param($CacheTypes)
    
    Write-UIStatus "Running auto-optimization..." "Yellow" "🔧"
    
    # Clear large caches
    foreach ($cache in $CacheTypes) {
        $sizeMB = [Math]::Round($cache.Size / 1MB, 2)
        if ($sizeMB -gt 50) {
            Write-UIStatus "  Auto-clearing large cache: $($cache.Name) (${sizeMB}MB)" "Yellow"
            try {
                Remove-Item $cache.Path -Recurse -Force -ErrorAction SilentlyContinue
                Write-UIStatus "  ✅ Cleared $($cache.Name)" "Green"
            } catch {
                Write-UIStatus "  ❌ Failed to clear $($cache.Name)" "Red"
            }
        }
    }
}

function Invoke-UIPerformanceCheck {
    Write-UIStatus "Starting Unity UI Performance Check..." "Cyan" "🚀"
    Write-UIStatus "=====================================" "Cyan"
    
    $allPassed = $true
    
    # Check Unity process performance
    if (-not (Test-UIPerformanceIssues)) {
        $allPassed = $false
    }
    
    # Get cache information
    $cacheTypes = Get-UICacheSize
    
    # Check UIElements performance
    if (-not (Test-UIElementsPerformance)) {
        $allPassed = $false
    }
    
    # Clear caches if requested
    if ($ClearCaches) {
        Clear-UICaches $cacheTypes
    }
    
    # Auto-optimize if requested
    if ($AutoOptimize) {
        Invoke-AutoOptimization $cacheTypes
    }
    
    # Show optimization suggestions
    if (-not $allPassed) {
        Show-OptimizationSuggestions
    }
    
    Write-UIStatus "=====================================" "Cyan"
    if ($allPassed) {
        Write-UIStatus "UI Performance Check PASSED" "Green" "✅"
    } else {
        Write-UIStatus "UI Performance Issues Detected" "Red" "⚠️"
        Write-UIStatus "Use -AutoOptimize to apply fixes automatically" "Yellow" "💡"
    }
    
    return $allPassed
}

# Main execution
Invoke-UIPerformanceCheck
