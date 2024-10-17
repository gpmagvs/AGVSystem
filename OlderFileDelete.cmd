@echo off
setlocal
:: Check for help argument
if "%~1"=="-h" (
    goto :help
) else if "%~1"=="--help" (
    goto :help
)
:: Check if the first argument (days_old) is provided, otherwise default to 1
if "%~1"=="" (
    set "days_old=90"
) else (
    set "days_old=%~1"
)

:: Check if the second argument (target_folder) is provided, otherwise default to C:\AGVSystemLog
if "%~2"=="" (
    set "target_folder=C:\AGVSystemLog"
) else (
    set "target_folder=%~2"
)

set "event_source=OlderLogCleanup"
set "event_log=Application"

:: Ensure the event source exists
powershell -NoProfile -Command ^
    "if (-not (Get-EventLog -LogName '%event_log%' -Source '%event_source%' -ErrorAction SilentlyContinue)) { New-EventLog -LogName '%event_log%' -Source '%event_source%' }"

:: Use PowerShell to delete files older than specified days based on last modification time without confirmation
powershell -NoProfile -Command ^
    "Get-ChildItem -Path '%target_folder%' -Recurse | Where-Object { $_.PSIsContainer -eq $false -and $_.LastWriteTime -lt (Get-Date).AddDays(-%days_old%) } | Remove-Item -Force -Confirm:$false; " ^
    "Get-ChildItem -Path '%target_folder%' -Recurse | Where-Object { $_.PSIsContainer -eq $true -and @(Get-ChildItem -Path $_.FullName -Force -Recurse | Where-Object { $_.PSIsContainer -eq $false }).Count -eq 0 } | Remove-Item -Force -Recurse"

:: Calculate the date threshold and write to event log
powershell -NoProfile -Command ^
    "$dateThreshold = (Get-Date).AddDays(-%days_old%).ToString('yyyy-MM-dd'); " ^
    "Write-EventLog -LogName '%event_log%' -Source '%event_source%' -EntryType Information -EventId 1000 -Message ('Older files cleanup completed successfully for folder: %target_folder%. Files older than ' + $dateThreshold + ' were deleted.')"

endlocal
exit /b

:help
echo Usage: LogDelete.cmd [days_old] [target_folder]
echo.
echo days_old      - Number of days to look back for file deletions. Default is 1.
echo target_folder - The folder path to clean up. Default is C:\AGVSystemLog.
echo.
echo Example: LogDelete.cmd 10 "D:\Logs"
endlocal
exit /b