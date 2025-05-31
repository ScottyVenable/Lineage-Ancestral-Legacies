# Unity Performance Monitor Script
# Run this script to continuously monitor Unity Editor performance

param(
    [int]$IntervalSeconds = 5,
    [switch]$LogToFile,
    [string]$LogPath = "unity-performance.log"
)

function Write-Output-Message {
    param($Message, $Color = "White")
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $output = "[$timestamp] $Message"
    
    Write-Host $output -ForegroundColor $Color
    
    if ($LogToFile) {
        Add-Content -Path $LogPath -Value $output
    }
}

function Get-UnityPerformance {
    Write-Output-Message "=== Unity Performance Report ===" "Green"
    
    # Unity Processes
    $unityProcesses = Get-Process Unity* -ErrorAction SilentlyContinue
    if ($unityProcesses) {
        Write-Output-Message "Unity Processes Found:" "Yellow"
        foreach ($process in $unityProcesses) {
            $memoryMB = [math]::Round($process.WorkingSet / 1MB, 2)
            $cpuTime = $process.CPU
            Write-Output-Message "  $($process.ProcessName): Memory=${memoryMB}MB, CPU=${cpuTime}s, Handles=$($process.Handles)" "White"
        }
    } else {
        Write-Output-Message "No Unity processes found" "Red"
    }
    
    # System Resources
    Write-Output-Message "`nSystem Resources:" "Yellow"
    
    # CPU Usage
    $cpu = Get-WmiObject -Class Win32_Processor
    $cpuUsage = $cpu.LoadPercentage
    Write-Output-Message "  CPU Usage: ${cpuUsage}%" "White"
    
    # Memory Usage
    $memory = Get-WmiObject -Class Win32_OperatingSystem
    $totalMemoryGB = [math]::Round($memory.TotalVisibleMemorySize / 1MB, 2)
    $freeMemoryGB = [math]::Round($memory.FreePhysicalMemory / 1MB, 2)
    $usedMemoryGB = $totalMemoryGB - $freeMemoryGB
    $memoryUsagePercent = [math]::Round(($usedMemoryGB / $totalMemoryGB) * 100, 2)
    Write-Output-Message "  Memory Usage: ${usedMemoryGB}GB / ${totalMemoryGB}GB (${memoryUsagePercent}%)" "White"
    
    # Disk I/O for project folder
    Write-Output-Message "`nProject Folder Size:" "Yellow"
    $projectSize = Get-ChildItem -Path "." -Recurse -ErrorAction SilentlyContinue | 
                   Measure-Object -Property Length -Sum
    $projectSizeGB = [math]::Round($projectSize.Sum / 1GB, 2)
    Write-Output-Message "  Total Size: ${projectSizeGB}GB" "White"
    
    # Library folder size (Unity cache)
    if (Test-Path "Library") {
        $librarySize = Get-ChildItem -Path "Library" -Recurse -ErrorAction SilentlyContinue | 
                       Measure-Object -Property Length -Sum
        $librarySizeGB = [math]::Round($librarySize.Sum / 1GB, 2)
        Write-Output-Message "  Library Cache: ${librarySizeGB}GB" "White"
    }
    
    # Recent Unity log errors
    $editorLogPath = "$env:LOCALAPPDATA\Unity\Editor\Editor.log"
    if (Test-Path $editorLogPath) {
        Write-Output-Message "`nRecent Unity Log Issues:" "Yellow"
        $recentErrors = Select-String -Path $editorLogPath -Pattern "(error|Error|ERROR|exception|Exception)" -ErrorAction SilentlyContinue | 
                        Select-Object -Last 3
        if ($recentErrors) {
            foreach ($error in $recentErrors) {
                Write-Output-Message "  Line $($error.LineNumber): $($error.Line.Substring(0, [Math]::Min(100, $error.Line.Length)))" "Red"
            }
        } else {
            Write-Output-Message "  No recent errors found" "Green"
        }
    }
    
    Write-Output-Message "`n" + "="*50 + "`n" "Green"
}

# Main monitoring loop
if ($LogToFile) {
    Write-Output-Message "Logging to: $LogPath" "Green"
}

Write-Output-Message "Unity Performance Monitor Started (Interval: ${IntervalSeconds}s)" "Green"
Write-Output-Message "Press Ctrl+C to stop monitoring" "Yellow"

try {
    while ($true) {
        Clear-Host
        Get-UnityPerformance
        Start-Sleep -Seconds $IntervalSeconds
    }
}
catch {
    Write-Output-Message "Monitoring stopped" "Red"
}
