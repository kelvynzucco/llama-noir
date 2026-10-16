@echo off
cd /d "%~dp0"
set "PATH=%USERPROFILE%\.dotnet;%PATH%"
echo Iniciando LlamaInterrogation...
if exist "bin\Debug\net8.0\LlamaInterrogation.exe" (
    start "" "bin\Debug\net8.0\LlamaInterrogation.exe"
) else (
    "%USERPROFILE%\.dotnet\dotnet.exe" run
)
