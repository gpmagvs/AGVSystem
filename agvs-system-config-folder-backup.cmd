@echo off
setlocal

rem Set the source folder to compress (replace with your folder path)
set "source_folder=C:\AGVS"
set "backup_folder=C:\AGVS_Configs_Daily_Backup"
rem Get current date in YYYY-MM-DD format
for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /value') do set datetime=%%I
set "date_string=%datetime:~0,4%-%datetime:~4,2%-%datetime:~6,2%"

rem Set the output zip file name
set "zip_file=%backup_folder%\%date_string%.zip"

mkdir %backup_folder% || echo %backup_folder% already exist

rem Compress the folder
powershell -command "Compress-Archive -Path '%source_folder%' -DestinationPath '%zip_file%'" -Force

echo Folder compressed to %zip_file%