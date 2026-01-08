@echo off
setlocal 

REM ----------------------------------
REM %1 = Full path of folder to zip
REM %2 = Output archive name (without .tgz)
REM ----------------------------------

if "%~1"=="" (
    echo ERROR: Source folder path missing
    exit /b 1
)

if "%~2"=="" (
    echo ERROR: Output name missing
    exit /b 1
)

set SOURCE_DIR=%~1
set OUTPUT_NAME=%~2

REM ---- Resolve paths ----
for %%I in ("%SOURCE_DIR%") do (
    set SOURCE_PARENT=%%~dpI
)

set TEMP_DIR=%SOURCE_PARENT%__package_temp__

REM ---- Cleanup if exists ----
if exist "%TEMP_DIR%" rmdir /s /q "%TEMP_DIR%"

REM ---- Create temp Package folder ----
mkdir "%TEMP_DIR%\Package"

REM ---- Copy contents (not the folder itself) ----
xcopy "%SOURCE_DIR%\*" "%TEMP_DIR%\package\" /E /I /Y >nul

REM ---- Create tgz ----
pushd "%TEMP_DIR%"
tar -czf "%SOURCE_PARENT%%OUTPUT_NAME%.tgz" package
popd

REM ---- Cleanup ----
rmdir /s /q "%TEMP_DIR%"

echo ----------------------------------
echo Archive created successfully
echo Output: %SOURCE_PARENT%%OUTPUT_NAME%.tgz
echo ----------------------------------
