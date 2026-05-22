extends SceneTree

const DEFAULT_OUTPUT_PATH := "GDExtensionWrappers"
const DEFAULT_NAMESPACE := "GDExtension.Wrappers"

const EDITOR_NAMESPACE_KEY := "gdextension_wrapper_generator/namespace"
const EDITOR_PATH_KEY := "gdextension_wrapper_generator/target_path"
const EDITOR_INDENTATION_KEY := "gdextension_wrapper_generator/indentation_mode"
const EDITOR_INTERNAL_VISIBILITY_KEY := "gdextension_wrapper_generator/internal_visibility_mode"

func _initialize() -> void:
	var parsed := _parse_args(OS.get_cmdline_user_args())
	if parsed.get("version_only", false):
		print("GDExtension C# wrapper generator (CLI) v2.0")
		quit(0)
		return
	if parsed.get("help", false):
		_print_help()
		quit(0)
		return

	if parsed.has("error"):
		push_error(parsed["error"])
		_print_help()
		quit(2)
		return

	const bridge_script_path := (
		"res://addons/csharp_wrapper_generator_for_gdextension/WrapperGeneratorBridge.cs"
	)
	var bridge_script: Script = load(bridge_script_path)
	if bridge_script == null:
		push_error("Failed to load %s. Build the C# solution (dotnet build)." % bridge_script_path)
		quit(2)
		return
	var bridge: Object = bridge_script.new()
	var exit_code: Variant = bridge.call(
		"Generate",
		parsed["namespace"],
		parsed["output"],
		parsed["use_tabs"],
		parsed["expose_internal"],
	)
	if exit_code == null:
		push_error("WrapperGeneratorBridge.Generate returned null (invalid call).")
		bridge.free()
		quit(2)
		return
	var code := int(exit_code)
	bridge.free()
	quit(code)


func _parse_args(args: PackedStringArray) -> Dictionary:
	var result := {
		"help": false,
		"output": DEFAULT_OUTPUT_PATH,
		"namespace": DEFAULT_NAMESPACE,
		"use_tabs": false,
		"expose_internal": false,
	}
	var explicit := {
		"output": false,
		"namespace": false,
		"use_tabs": false,
		"expose_internal": false,
	}
	var use_editor_settings := false
	var positionals: Array[String] = []

	var i := 0
	while i < args.size():
		var raw_arg := args[i]
		var split := _split_key_value(raw_arg)
		var key: String = split["key"]
		var inline_value = split["value"]

		match key:
			"--help", "-h", "-?":
				result["help"] = true
			"--version", "-v":
				result["help"] = true
				result["version_only"] = true
			"--use-editor-settings", "--from-editor", "--editor-settings":
				use_editor_settings = true
			# Output path
			"--output", "-o", "--path", "--target-path", "--target", "--save-path":
				var output_value := _take_value(args, i, inline_value, key)
				if output_value == null:
					return _error("Option %s requires a value." % key)
				result["output"] = output_value
				explicit["output"] = true
				if inline_value == null:
					i += 1
			# Namespace
			"--namespace", "-n":
				var namespace_value := _take_value(args, i, inline_value, key)
				if namespace_value == null:
					return _error("Option %s requires a value." % key)
				result["namespace"] = namespace_value
				explicit["namespace"] = true
				if inline_value == null:
					i += 1
			# Indentation
			"--indent", "-i", "--indentation":
				var indent_value := _take_value(args, i, inline_value, key)
				if indent_value == null:
					return _error("Option %s requires 'spaces' or 'tabs'." % key)
				var indent_parsed := _parse_indent(indent_value)
				if indent_parsed == null:
					return _error(
						"Invalid indentation '%s'. Use: spaces, tabs, space, tab, 0, 1." % indent_value
					)
				result["use_tabs"] = indent_parsed
				explicit["use_tabs"] = true
				if inline_value == null:
					i += 1
			"--spaces", "--use-spaces":
				result["use_tabs"] = false
				explicit["use_tabs"] = true
			"--tabs", "--use-tabs":
				result["use_tabs"] = true
				explicit["use_tabs"] = true
			# Internal visibility
			"--expose-internal", "--expose-internal-members", "--internal", "--expose":
				var expose_value := _take_optional_bool(args, i, inline_value, true)
				if expose_value == null:
					return _error(
						"Option %s requires true/false, yes/no, on/off, or 1/0 when a value is given." % key
					)
				result["expose_internal"] = expose_value
				explicit["expose_internal"] = true
				if inline_value == null and i + 1 < args.size() and _looks_like_bool_value(args[i + 1]):
					i += 1
			"--hide-internal", "--no-expose-internal", "--no-internal":
				var hide_value := _take_optional_bool(args, i, inline_value, false)
				if hide_value == null:
					return _error(
						"Option %s requires true/false, yes/no, on/off, or 1/0 when a value is given." % key
					)
				result["expose_internal"] = hide_value
				explicit["expose_internal"] = true
				if inline_value == null and i + 1 < args.size() and _looks_like_bool_value(args[i + 1]):
					i += 1
			"--internal-visibility", "--internal-visibility-mode":
				var visibility_value := _take_value(args, i, inline_value, key)
				if visibility_value == null:
					return _error("Option %s requires 'expose' or 'hide'." % key)
				var visibility_parsed := _parse_internal_visibility(visibility_value)
				if visibility_parsed == null:
					return _error(
						"Invalid internal visibility '%s'. Use: expose, hide, do-not-expose, do_not_expose."
						% visibility_value
					)
				result["expose_internal"] = visibility_parsed
				explicit["expose_internal"] = true
				if inline_value == null:
					i += 1
			_:
				if key.begins_with("-"):
					return _error("Unknown option: %s" % raw_arg)
				positionals.append(raw_arg)
		i += 1

	# Positionals fill only options not already set by flags (flags win).
	if positionals.size() >= 1 and not explicit["output"]:
		result["output"] = positionals[0]
	if positionals.size() >= 2 and not explicit["namespace"]:
		result["namespace"] = positionals[1]
	if positionals.size() > 2:
		return _error("Too many positional arguments. Expected at most [OUTPUT_PATH] [NAMESPACE].")

	if use_editor_settings:
		var editor_err := _apply_editor_settings(result, explicit)
		if editor_err != "":
			return _error(editor_err)

	return result


func _split_key_value(arg: String) -> Dictionary:
	var eq_pos := arg.find("=")
	if eq_pos == -1:
		return {"key": arg, "value": null}
	return {"key": arg.substr(0, eq_pos), "value": arg.substr(eq_pos + 1)}


func _take_value(args: PackedStringArray, index: int, inline_value, option_name: String) -> Variant:
	if inline_value != null:
		return inline_value if str(inline_value) != "" else null
	if index + 1 >= args.size():
		return null
	var next_arg: String = args[index + 1]
	if next_arg.begins_with("-"):
		return null
	return next_arg


func _take_optional_bool(
	args: PackedStringArray, index: int, inline_value, default_value: bool
) -> Variant:
	if inline_value != null:
		if str(inline_value) == "":
			return default_value
		return _parse_bool(str(inline_value))
	if index + 1 >= args.size():
		return default_value
	var next_arg: String = args[index + 1]
	if not _looks_like_bool_value(next_arg):
		return default_value
	return _parse_bool(next_arg)


func _looks_like_bool_value(value: String) -> bool:
	var lower := value.to_lower()
	return lower in ["true", "false", "yes", "no", "on", "off", "1", "0"]


func _parse_bool(value: String) -> Variant:
	var lower := value.to_lower()
	match lower:
		"true", "yes", "on", "1":
			return true
		"false", "no", "off", "0":
			return false
		_:
			return null


func _parse_indent(value: String) -> Variant:
	var lower := value.to_lower()
	match lower:
		"spaces", "space", "0":
			return false
		"tabs", "tab", "1":
			return true
		_:
			return null


func _parse_internal_visibility(value: String) -> Variant:
	var lower := value.to_lower().replace("_", "-")
	match lower:
		"expose", "exposed", "show", "visible", "1", "true", "yes", "on":
			return true
		"hide", "hidden", "do-not-expose", "do-not-expose-internal", "conceal", "0", "false", "no", "off":
			return false
		_:
			return null


func _apply_editor_settings(result: Dictionary, explicit: Dictionary) -> String:
	if not Engine.has_singleton(&"EditorInterface"):
		return (
			"--use-editor-settings requires the editor build of Godot "
			+ "(for example: godot-mono --editor --headless ...)."
		)
	var editor_interface: Object = Engine.get_singleton(&"EditorInterface")
	if editor_interface == null:
		return "--use-editor-settings is unavailable (EditorInterface not found)."

	var editor_settings: Object = editor_interface.get_editor_settings()
	if editor_settings == null:
		return "--use-editor-settings is unavailable (EditorSettings not found)."

	if not explicit["namespace"] and editor_settings.has_setting(EDITOR_NAMESPACE_KEY):
		result["namespace"] = str(editor_settings.get_setting(EDITOR_NAMESPACE_KEY))

	if not explicit["output"] and editor_settings.has_setting(EDITOR_PATH_KEY):
		result["output"] = str(editor_settings.get_setting(EDITOR_PATH_KEY))

	if not explicit["use_tabs"] and editor_settings.has_setting(EDITOR_INDENTATION_KEY):
		result["use_tabs"] = int(editor_settings.get_setting(EDITOR_INDENTATION_KEY)) == 1

	if not explicit["expose_internal"] and editor_settings.has_setting(EDITOR_INTERNAL_VISIBILITY_KEY):
		result["expose_internal"] = int(editor_settings.get_setting(EDITOR_INTERNAL_VISIBILITY_KEY)) == 1

	return ""


func _error(message: String) -> Dictionary:
	return {"error": message, "help": false}


func _print_help() -> void:
	print(
		"""GDExtension C# wrapper generator (CLI)

Usage:
  godot-mono --headless --path <project> \\
    --script res://addons/csharp_wrapper_generator_for_gdextension/cli_entrypoint.gd -- \\
    [OUTPUT_PATH] [NAMESPACE]

Options:
  -h, --help, -?                    Show this help
  -v, --version                     Show version and exit

  -o, --output PATH                 Output folder under res:// (default: GDExtensionWrappers)
      --path, --target-path,
      --target, --save-path PATH    Aliases for --output

  -n, --namespace NAME              C# namespace (default: GDExtension.Wrappers)

  -i, --indent STYLE                Indentation: spaces|tabs (also: space, tab, 0, 1)
      --indentation STYLE           Alias for --indent
      --spaces, --use-spaces        Use spaces (default)
      --tabs, --use-tabs            Use tabs

      --expose-internal             Expose internal (_) members
      --expose-internal[=BOOL]      Optional true/false/yes/no/on/off/1/0
      --expose-internal-members,
      --internal, --expose          Aliases for --expose-internal
      --hide-internal,
      --no-expose-internal,
      --no-internal                 Do not expose internal members (default)
      --hide-internal[=BOOL]        Optional boolean value
      --internal-visibility MODE    expose|hide|do-not-expose

      --use-editor-settings         Use saved editor plugin settings for unset options
      --from-editor,
      --editor-settings             Aliases for --use-editor-settings

All value options accept --option=value syntax.

Positionals (only apply when the matching flag was not given):
  [OUTPUT_PATH] [NAMESPACE]

Exit codes:
  0  Success
  1  Generation failed (I/O or write error)
  2  Invalid arguments

Examples:
  godot-mono --headless --path . \\
    --script res://addons/csharp_wrapper_generator_for_gdextension/cli_entrypoint.gd

  godot-mono --headless --path . \\
    --script res://addons/csharp_wrapper_generator_for_gdextension/cli_entrypoint.gd -- \\
    --output=MyWrappers --namespace=MyGame.GDExtension --indent=tabs

  godot-mono --editor --headless --path . \\
    --script res://addons/csharp_wrapper_generator_for_gdextension/cli_entrypoint.gd -- \\
    --use-editor-settings
"""
	)
