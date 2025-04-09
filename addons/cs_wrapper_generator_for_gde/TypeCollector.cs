using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using Environment = System.Environment;

namespace GDExtensionAPIGenerator;

internal static partial class TypeCollector
{

#if GODOT4_4_OR_GREATER
    public static bool TryCollectGDExtensionTypes(out string[] gdeClassTypes, out ICollection<string> godotBuiltinTypeNames)
    {
        var classList = ClassDB.GetClassList();
        godotBuiltinTypeNames = classList
            .Where(x => ClassDB.ClassGetApiType(x) is ClassDB.ApiType.Core or ClassDB.ApiType.Editor).Append("Variant").ToHashSet();
        gdeClassTypes =  classList
            .Where(x => ClassDB.ClassGetApiType(x) is ClassDB.ApiType.Extension or ClassDB.ApiType.EditorExtension).ToArray();
        return true;
    }
#else
    public static readonly HashSet<string> BanClassType =
    [
        "CodeTextEditor",
        "ConnectionInfoDialog",
        "EditorPlainTextSyntaxHighlighter",
        "EditorStandardSyntaxHighlighter",
        "FramebufferCacheRD",
        "GotoLineDialog",
        "RenderBufferCustomDataRD",
        "RenderBufferDataForwardClustered",
        "RenderBuffersGI",
        "ScriptEditorQuickOpen",
        "ScriptTextEditor",
        "UniformSetCacheRD",
        "EditorHelp",
        "FindBar",
        "GodotPhysicsDirectSpaceState2D",
        "NativeMenuWindows",
    ];
    public static bool TryCollectGDExtensionTypes(out string[] gdeClassTypes, out ICollection<string> godotBuiltinTypeNames)
    {
        // The builtin types are obtained by creating & launching an empty project,
        // and make the Godot Editor execute a custom GDScript that prints every types
        // from the ClassDB, and parse the final StandardOutput when finish,
        // it's a dumb approach, but this is the only way we succeed.
        gdeClassTypes = null;
        godotBuiltinTypeNames = null;
        var tempPath = CreateTempDirectory();
        GD.Print($"Temp workspace directory: {tempPath}");
        var scriptFullPath = CreateDumpDBScript(tempPath);
        GD.Print($"Godot Core ClassDB Dump script path: {scriptFullPath}");
        var dummyProjectPath = CreateDummyProject(tempPath);
        GD.Print($"Dummy Project path: {dummyProjectPath}");
        var godotExecutablePath = Environment.ProcessPath!;
        GD.Print($"Godot Executable path: {godotExecutablePath}");
        string[] dumpGodotClassCommands = ["--headless", "--script", scriptFullPath, "--editor", "--verbose", "--path", dummyProjectPath];

        GD.Print(
            $"""
             Dumping Godot Builtin Classes...
             Starting Godot Editor ({Path.GetFileName(Environment.ProcessPath)})
             Command Line: {godotExecutablePath} {string.Join(' ', dumpGodotClassCommands)}
             """
        );

        string resultString;
        using (var result = new Godot.Collections.Array())
        {
            // We use os instead of dotnet process here because the latter one does not working properly.
            OS.Execute(godotExecutablePath, dumpGodotClassCommands, result);
            Directory.Delete(tempPath, true);
            try
            {
                resultString = result[0].AsString();
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed!\n{e}");
                return false;
            }
        }

        if (!ExtractClassNamesFromStdOut(resultString, out var builtinClassTypes))
        {
            GD.PrintErr("Error when extracting builtin class names!");
            return false;
        }
        
        // GDExtension types are the difference
        // between the builtin types and the types
        // existing in the current project's ClassDB. 
        var currentClassTypes = ClassDB.GetClassList();
        godotBuiltinTypeNames = builtinClassTypes;
        gdeClassTypes = currentClassTypes
            .Except(builtinClassTypes)
            .Except(BanClassType)
            .ToArray();
        return true;
    }
        private static string CreateTempDirectory()
    {
        var tempPath = Path.GetTempFileName();
        File.Delete(tempPath);
        Directory.CreateDirectory(tempPath);
        return tempPath;
    }
    private static string CreateDumpDBScript(string tempPath)
    {
        const string dumpDBScript =
            $"""
             @tool
             extends SceneTree
             func  _process(delta: float) -> bool:
             	print("{GENERATOR_DUMP_HEADER}");
             	for name in ClassDB.get_class_list():
             		print(name);
             	print("{GENERATOR_DUMP_FOOTER}");
             	quit()
             	return true
             """;
        const string dumpDBFileName = "dump_class_db.gd";
        var scriptFullPath = Path.Combine(tempPath, dumpDBFileName);
        File.WriteAllText(scriptFullPath, dumpDBScript);
        return scriptFullPath;
    }

    private static string CreateDummyProject(string tempPath)
    {
        var dummyProjectPath = Path.Combine(tempPath, "project");
        Directory.CreateDirectory(dummyProjectPath);
        using var config = new ConfigFile();
        config.SetValue(string.Empty, "config_version", 5);
        config.SetValue("application", "config/features", ProjectSettings.GetSetting("config/features"));
        config.SetValue("application", "config/name", "Empty Project");
        File.WriteAllText(Path.Combine(dummyProjectPath, "project.godot"), config.ToString());

        return dummyProjectPath;
    }
    
    private static bool ExtractClassNamesFromStdOut(string resultString, out HashSet<string> builtinClassTypes)
    {
        var matchResult = GetExtractClassNameRegex().Match(resultString);
        if (!matchResult.Success)
        {
            builtinClassTypes = null;
            return false;
        }

        builtinClassTypes = matchResult
            .Groups["ClassNames"]
            .Value
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();
        return true;
    }
    private const string GENERATOR_DUMP_HEADER = "WRAPPER_GENERATOR_DUMP_CLASS_DB_START";
    private const string GENERATOR_DUMP_FOOTER = "WRAPPER_GENERATOR_DUMP_CLASS_DB_END";
    [GeneratedRegex(
        $"{GENERATOR_DUMP_HEADER}(?<ClassNames>.+?){GENERATOR_DUMP_FOOTER}",
        RegexOptions.Singleline | RegexOptions.NonBacktracking
    )]
    private static partial Regex GetExtractClassNameRegex();
#endif


}