# Unity UI Performance - Quick Commands for UI Work Sessions
# Use these commands when Unity Editor feels sluggish during UI work

Write-Host "🎨 Unity UI Performance Commands" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "📊 MONITOR COMMANDS:" -ForegroundColor Green
Write-Host ".\ui-work-monitor.ps1              # Start real-time UI monitoring"
Write-Host "Get-Process Unity | ft CPU,WorkingSet64,Id  # Quick Unity status"
Write-Host ""

Write-Host "🧹 QUICK FIXES:" -ForegroundColor Yellow
Write-Host "# Clear UI cache when editor feels slow:"
Write-Host "Remove-Item 'Library\UIElements\*' -Recurse -Force"
Write-Host ""
Write-Host "# Clear all UI-related caches:"
Write-Host "Remove-Item 'Library\UIElements\*' -Recurse -Force"
Write-Host "Remove-Item 'Library\BurstCache\*' -Recurse -Force"
Write-Host "Remove-Item 'Library\ShaderCache\*' -Recurse -Force"
Write-Host ""

Write-Host "🚨 EMERGENCY COMMANDS:" -ForegroundColor Red
Write-Host "# If Unity becomes completely unresponsive:"
Write-Host "Get-Process Unity | Stop-Process -Force"
Write-Host ""
Write-Host "# If multiple Unity instances are running:"
Write-Host "Get-Process Unity* | ft ProcessName,Id,CPU"
Write-Host "Get-Process Unity | Sort-Object CPU -Descending | Select-Object -First 2 | Stop-Process -Force"
Write-Host ""

Write-Host "💡 UI BEST PRACTICES:" -ForegroundColor Magenta
Write-Host "- Avoid scaling 9-slice sprites beyond 1000x1000"
Write-Host "- Use separate canvases for static vs dynamic UI"
Write-Host "- Disable 'Raycast Target' on decorative images"
Write-Host "- Limit Update() calls in UI scripts (use time-based checks)"
Write-Host "- Pool UI elements instead of creating/destroying"
Write-Host ""

Write-Host "✅ MONITORING ACTIVE - Your UI session is being monitored!" -ForegroundColor Green
