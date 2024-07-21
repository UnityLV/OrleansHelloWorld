@echo off

set BASE_DIR=%~dp0


echo Checking for .NET 8 runtime...
dotnet --list-runtimes | findstr /i "8.0" >nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET 8 runtime is not installed. Please install .NET 8 and try again.
    pause
    exit /b
)

echo .NET 8 runtime found. Proceeding with application start...


echo Starting Silo.exe...
start "" "%BASE_DIR%silo\bin\Release\net8.0\Silo.exe"


timeout /t 2 /nobreak >nul


echo Starting Client.exe (first instance)...
start "" "%BASE_DIR%Client\bin\Release\net8.0\Client.exe"

echo Starting Client.exe (second instance)...
start "" "%BASE_DIR%Client\bin\Release\net8.0\Client.exe"


echo Press any key to exit...
pause
