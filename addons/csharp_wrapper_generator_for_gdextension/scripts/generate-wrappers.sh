#!/usr/bin/env bash
# Generate GDExtension C# wrappers (Linux and macOS).
# Usage: ./generate-wrappers.sh [CLI options...]
# See: res://addons/csharp_wrapper_generator_for_gdextension/cli_entrypoint.gd --help

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ADDON_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
PROJECT_DIR="$(cd "${ADDON_DIR}/../.." && pwd)"
CLI_SCRIPT="res://addons/csharp_wrapper_generator_for_gdextension/cli_entrypoint.gd"
DOTNET_INSTALL_URL="https://dot.net/sdk"

if [[ ! -f "${PROJECT_DIR}/project.godot" ]]; then
	echo "error: project.godot not found at ${PROJECT_DIR}" >&2
	echo "Run this script from a Godot project that includes this addon." >&2
	exit 1
fi

require_dotnet_sdk() {
	if ! command -v dotnet >/dev/null 2>&1; then
		echo "error: .NET SDK not found on PATH." >&2
		echo "Install the SDK from ${DOTNET_INSTALL_URL}" >&2
		exit 1
	fi
	if ! dotnet --version >/dev/null 2>&1; then
		echo "error: 'dotnet' is present but failed to run. Repair or reinstall the .NET SDK." >&2
		exit 1
	fi
	if [[ -z "$(dotnet --list-sdks 2>/dev/null | grep -E '^[0-9]+\.[0-9]+' || true)" ]]; then
		echo "error: No .NET SDKs are installed (dotnet --list-sdks is empty)." >&2
		echo "Install the SDK from ${DOTNET_INSTALL_URL}" >&2
		exit 1
	fi
}

run_dotnet_build() {
	require_dotnet_sdk

	local csproj=""
	for candidate in "${PROJECT_DIR}"/*.csproj; do
		[[ -f "${candidate}" ]] || continue
		csproj="${candidate}"
		break
	done

	echo "Using $(dotnet --version)"
	echo "Building C# project in ${PROJECT_DIR}..."
	(
		cd "${PROJECT_DIR}"
		if [[ -n "${csproj}" ]]; then
			dotnet build "${csproj}"
		else
			dotnet build
		fi
	)
}

find_godot() {
	if [[ -n "${GODOT_BIN:-}" && -x "${GODOT_BIN}" ]]; then
		echo "${GODOT_BIN}"
		return 0
	fi

	local -a candidates=()
	if [[ "$(uname -s)" == "Darwin" ]]; then
		local app
		for app in /Applications/Godot*.app /Applications/Godot*/*.app; do
			[[ -d "${app}" ]] || continue
			local binary="${app}/Contents/MacOS/Godot"
			[[ -x "${binary}" ]] && candidates+=("${binary}")
		done
	fi

	local name
	for name in godot-mono godot Godot; do
		if command -v "${name}" >/dev/null 2>&1; then
			candidates+=("$(command -v "${name}")")
		fi
	done

	local candidate
	for candidate in "${candidates[@]}"; do
		if "${candidate}" --version 2>/dev/null | grep -qi mono; then
			echo "${candidate}"
			return 0
		fi
	done

	if [[ ${#candidates[@]} -gt 0 ]]; then
		echo "${candidates[0]}"
		return 0
	fi

	return 1
}

if [[ "${SKIP_BUILD:-0}" != "1" ]]; then
	run_dotnet_build
else
	echo "Skipping dotnet build (SKIP_BUILD=1)."
fi

GODOT="$(find_godot)" || {
	echo "error: Godot .NET editor not found." >&2
	echo "Set GODOT_BIN to your Godot mono executable, or add it to PATH." >&2
	exit 1
}

echo "Generating wrappers with: ${GODOT}"
set +e
"${GODOT}" --headless --path "${PROJECT_DIR}" --script "${CLI_SCRIPT}" -- "$@"
exit_code=$?
set -e
exit "${exit_code}"
