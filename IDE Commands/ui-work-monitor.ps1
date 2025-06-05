# UI Work Session Monitor
# Monitors Unity performance while working on UI elements

Write-Host "[UI MONITOR] Starting UI Work Session Monitor..." -ForegroundColor Cyan
Write-Host "[UI MONITOR] Monitoring Unity performance every 30 seconds" -ForegroundColor Green
Write-Host "[UI MONITOR] Press Ctrl+C to stop monitoring" -ForegroundColor Yellow
Write-Host ""

$counter = 0

while ($true) {
    $counter++
    $timestamp = Get-Date -Format "HH:mm:ss"
      # Check Unity process
    $unityProcesses = Get-Process Unity -ErrorAction SilentlyContinue
    
    if ($unityProcesses) {
        # Get the main Unity editor process (usually the one with highest memory usage)
        $unityProcess = $unityProcesses | Sort-Object WorkingSet64 -Descending | Select-Object -First 1
        
        $cpuTime = [math]::Round($unityProcess.CPU, 1)
        $memoryMB = [math]::Round($unityProcess.WorkingSet64 / 1MB, 0)
        
        # Color coding based on CPU usage
        $cpuColor = "Green"
        if ($cpuTime -gt 200) { $cpuColor = "Yellow" }
        if ($cpuTime -gt 400) { $cpuColor = "Red" }
        
        Write-Host "[$timestamp] Unity Performance:" -ForegroundColor Cyan
        Write-Host "  CPU: ${cpuTime}s | Memory: ${memoryMB}MB | PID: $($unityProcess.Id)" -ForegroundColor $cpuColor
        
        # Check for performance issues
        if ($cpuTime -gt 300) {
            Write-Host "  [WARNING] HIGH CPU USAGE DETECTED!" -ForegroundColor Red
            Write-Host "  [TIP] Consider clearing UI cache if UI feels sluggish" -ForegroundColor Yellow
        }
        
        # Check UI cache size every 5 checks (2.5 minutes)
        if ($counter % 5 -eq 0) {
            $uiElementsPath = "Library\UIElements"
            if (Test-Path $uiElementsPath) {
                $uiCacheSize = (Get-ChildItem $uiElementsPath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
                $uiCacheSize = [math]::Round($uiCacheSize, 1)
                Write-Host "  [CACHE] UI Cache: ${uiCacheSize}MB" -ForegroundColor Magenta
                
                if ($uiCacheSize -gt 100) {
                    Write-Host "  [WARNING] Large UI cache detected - may impact performance" -ForegroundColor Yellow
                }
            }
        }
    } else {
        Write-Host "[$timestamp] Unity not running" -ForegroundColor Gray
    }
    
    Write-Host ""
    Start-Sleep 30
}
