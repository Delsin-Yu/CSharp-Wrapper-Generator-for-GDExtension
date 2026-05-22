# Generate GDExtension C# wrappers (Windows PowerShell).
# Usage: .\generate-wrappers.ps1 [CLI options...]
# See: res://addons/csharp_wrapper_generator_for_gdextension/cli_entrypoint.gd --help

[CmdletBinding()]
param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$CliArgs
)

$ErrorActionPreference = "Stop"
$DotNetInstallUrl = "https://dot.net/sdk"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$AddonDir = Split-Path -Parent $ScriptDir
$ProjectDir = Split-Path -Parent (Split-Path -Parent $AddonDir)
$CliScript = "res://addons/csharp_wrapper_generator_for_gdextension/cli_entrypoint.gd"

$ProjectFile = Join-Path $ProjectDir "project.godot"
if (-not (Test-Path $ProjectFile)) {
    Write-Error "project.godot not found at $ProjectDir. Run this script from a Godot project that includes this addon."
}

function Test-DotNetSdk {
    $dotnetCmd = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $dotnetCmd) {
        Write-Error @"
.NET SDK not found on PATH.
Install the SDK from $DotNetInstallUrl
"@
    }

    $versionOutput = & dotnet --version 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "dotnet is present but failed to run. Repair or reinstall the .NET SDK from $DotNetInstallUrl"
    }

    $sdks = & dotnet --list-sdks 2>&1
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace(($sdks | Out-String).Trim())) {
        Write-Error @"
No .NET SDKs are installed (dotnet --list-sdks is empty).
Install the SDK from $DotNetInstallUrl
"@
    }

    return $versionOutput.Trim()
}

function Invoke-DotNetBuild {
    $sdkVersion = Test-DotNetSdk
    $csproj = Get-ChildItem -Path $ProjectDir -Filter "*.csproj" -File |
        Select-Object -First 1 -ExpandProperty FullName

    Write-Host "Using .NET SDK $sdkVersion"
    Write-Host "Building C# project in $ProjectDir..."
    Push-Location $ProjectDir
    try {
        if ($csproj) {
            dotnet build $csproj
        } else {
            dotnet build
        }
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    } finally {
        Pop-Location
    }
}

function Test-GodotMonoBinary {
    param([string]$Path)
    if (-not (Test-Path $Path)) { return $false }
    try {
        $version = & $Path --version 2>&1 | Out-String
        return $version -match "mono"
    } catch {
        return $true
    }
}

function Find-GodotMono {
    if ($env:GODOT_BIN -and (Test-Path $env:GODOT_BIN)) {
        return $env:GODOT_BIN
    }

    $candidates = @()
    foreach ($name in @("godot-mono", "Godot_v4.6-stable_mono_win64", "Godot_v4.5-stable_mono_win64")) {
        $cmd = Get-Command $name -ErrorAction SilentlyContinue
        if ($cmd) { $candidates += $cmd.Source }
    }

    $searchRoots = @(
        "${env:ProgramFiles}\Godot",
        "${env:LOCALAPPDATA}\Godot",
        "${env:USERPROFILE}\Godot",
        "${env:USERPROFILE}\Downloads"
    )
    foreach ($root in $searchRoots) {
        if (Test-Path $root) {
            $candidates += Get-ChildItem -Path $root -Filter "*mono*.exe" -Recurse -ErrorAction SilentlyContinue |
                Select-Object -ExpandProperty FullName
        }
    }

    foreach ($candidate in $candidates | Select-Object -Unique) {
        if (Test-GodotMonoBinary $candidate) { return $candidate }
    }

    if ($candidates.Count -gt 0) { return $candidates[0] }
    return $null
}

if ($env:SKIP_BUILD -ne "1") {
    Invoke-DotNetBuild
} else {
    Write-Host "Skipping dotnet build (SKIP_BUILD=1)."
}

$Godot = Find-GodotMono
if (-not $Godot) {
    Write-Error @"
Godot .NET editor not found.
Set GODOT_BIN to your Godot mono executable (e.g. Godot_v4.x-stable_mono_win64.exe), or add it to PATH.
"@
}

Write-Host "Generating wrappers with: $Godot"
& $Godot --headless --path $ProjectDir --script $CliScript -- @CliArgs
exit $LASTEXITCODE
