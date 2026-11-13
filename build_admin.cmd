@echo off
setlocal enabledelayedexpansion

REM 查找 MSBuild
for /f "delims=" %%i in ('"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe 2^>nul') do (
    set "MSBUILD=%%i"
)

if "%MSBUILD%"=="" (
    echo MSBuild not found via vswhere, trying dotnet msbuild
    cd /d d:\IDE-Workspace\Rxbit.BrigadeApp
    dotnet msbuild Rxbit.Brigade.Controll\Rxbit.Brigade.Controll.csproj /p:Configuration=Release /p:Platform=x86 /t:Rebuild /v:minimal
    goto :end
)

echo Using MSBuild: %MSBUILD%
cd /d d:\IDE-Workspace\Rxbit.BrigadeApp
"%MSBUILD%" Rxbit.Brigade.Controll\Rxbit.Brigade.Controll.csproj /p:Configuration=Release /p:Platform=x86 /t:Rebuild /v:minimal

:end
endlocal
