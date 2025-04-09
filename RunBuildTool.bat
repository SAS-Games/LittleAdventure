@echo off
REM Set Unity executable path (adjust to your version)
SET "UNITY_PATH=C:\Program Files\Unity\Hub\Editor\6000.0.39f1\Editor\Unity.exe"

REM Set project path (since the script is placed at the root of the Unity project)
SET "PROJECT_PATH=."
SET "LOG_PATH=%PROJECT_PATH%\EditorBuildLog.txt"

REM Prompt user to select a platform
echo ===============================
echo Select platform to build:
echo 1 - Windows
echo 2 - Android
echo 3 - iOS
echo ===============================
set /p PLATFORM_CHOICE=Enter your choice (1-3): 

IF "%PLATFORM_CHOICE%"=="1" (
    SET "BUILD_TARGET=Windows"
) ELSE IF "%PLATFORM_CHOICE%"=="2" (
    SET "BUILD_TARGET=Android"
) ELSE IF "%PLATFORM_CHOICE%"=="3" (
    SET "BUILD_TARGET=iOS"
) ELSE (
    echo Invalid choice. Exiting.
    pause
    exit /b 1
)

echo.
echo Starting Unity build for %BUILD_TARGET%...
echo.

REM Use PowerShell to pipe Unity output to both console and log file
powershell -NoProfile -Command ^
 "& { & '%UNITY_PATH%' -batchmode -nographics -quit -projectPath '%PROJECT_PATH%' -executeMethod JenkinsBuild.PerformBuild platform=%BUILD_TARGET% 2>&1 | Tee-Object -FilePath '%LOG_PATH%' }"

echo.
echo Build finished. Check log: %LOG_PATH%
pause
