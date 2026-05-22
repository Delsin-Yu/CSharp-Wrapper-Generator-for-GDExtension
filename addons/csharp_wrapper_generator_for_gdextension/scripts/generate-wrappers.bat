@echo off
setlocal EnableExtensions

REM Generate GDExtension C# wrappers (Windows Command Prompt).
REM Usage: generate-wrappers.bat [CLI options...]

set "SCRIPT_DIR=%~dp0"
set "CLI_SCRIPT=res://addons/csharp_wrapper_generator_for_gdextension/cli_entrypoint.gd"
set "DOTNET_INSTALL_URL=https://dot.net/sdk"

pushd "%SCRIPT_DIR%..\..\.."
set "PROJECT_DIR=%CD%"
popd

if not exist "%PROJECT_DIR%\project.godot" (
	echo error: project.godot not found at "%PROJECT_DIR%"
	echo Run this script from a Godot project that includes this addon.
	exit /b 1
)

if /I "%SKIP_BUILD%"=="1" goto :skip_build
call :require_dotnet_sdk
if errorlevel 1 exit /b 1
call :run_dotnet_build
if errorlevel 1 exit /b 1
goto :godot_setup

:skip_build
echo Skipping dotnet build (SKIP_BUILD=1).

:godot_setup
if defined GODOT_BIN (
	set "GODOT=%GODOT_BIN%"
	goto :godot_found
)

where godot-mono >nul 2>&1 && set "GODOT=godot-mono" && goto :godot_found
where Godot_v4.6-stable_mono_win64.exe >nul 2>&1 && set "GODOT=Godot_v4.6-stable_mono_win64.exe" && goto :godot_found
where Godot_v4.5-stable_mono_win64.exe >nul 2>&1 && set "GODOT=Godot_v4.5-stable_mono_win64.exe" && goto :godot_found

echo error: Godot .NET editor not found.
echo Set GODOT_BIN to your Godot mono executable, or add godot-mono to PATH.
exit /b 1

:godot_found
echo Generating wrappers with: %GODOT%
pushd "%PROJECT_DIR%"
"%GODOT%" --headless --path "%PROJECT_DIR%" --script "%CLI_SCRIPT%" -- %*
set "EXIT_CODE=%ERRORLEVEL%"
popd
exit /b %EXIT_CODE%

:require_dotnet_sdk
where dotnet >nul 2>&1
if errorlevel 1 (
	echo error: .NET SDK not found on PATH.
	echo Install the SDK from %DOTNET_INSTALL_URL%
	exit /b 1
)
dotnet --version >nul 2>&1
if errorlevel 1 (
	echo error: dotnet is present but failed to run. Repair or reinstall the .NET SDK.
	exit /b 1
)
dotnet --list-sdks 2>nul | findstr /R "^[0-9][0-9]*\.[0-9]" >nul 2>&1
if errorlevel 1 (
	echo error: No .NET SDKs are installed (dotnet --list-sdks is empty).
	echo Install the SDK from %DOTNET_INSTALL_URL%
	exit /b 1
)
for /f "delims=" %%V in ('dotnet --version 2^>nul') do set "DOTNET_VERSION=%%V"
echo Using .NET SDK %DOTNET_VERSION%
exit /b 0

:run_dotnet_build
echo Building C# project in %PROJECT_DIR%...
pushd "%PROJECT_DIR%"
set "BUILT=0"
for %%F in (*.csproj) do (
	dotnet build "%%F"
	if errorlevel 1 (
		popd
		exit /b 1
	)
	set "BUILT=1"
	goto :build_done
)
if "%BUILT%"=="0" (
	dotnet build
	if errorlevel 1 (
		popd
		exit /b 1
	)
)
:build_done
popd
exit /b 0
