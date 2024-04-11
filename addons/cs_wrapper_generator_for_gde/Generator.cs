#if TOOLS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Godot;
using Environment = System.Environment;

namespace GDExtensionAPIGenerator;

internal static partial class Generator
{
    [GeneratedRegex(
        "WRAPPER_GENERATOR_DUMP_CLASS_DB_START(?<ClassNames>.+?)WRAPPER_GENERATOR_DUMP_CLASS_DB_END",
        RegexOptions.Singleline | RegexOptions.NonBacktracking
    )]
    private static partial Regex GetExtractClassNameRegex();

    public static void Generate()
    {
        var tempPath = CreateTempDirectory();
        GD.Print($"Temp workspace directory: {tempPath}");
        var scriptFullPath = CreateDumpDBScript(tempPath);
        GD.Print($"Godot Core ClassDB Dump script path: {scriptFullPath}");
        var dummyProjectPath = CreateDummyProject(tempPath);
        GD.Print($"Dummy Project path: {dummyProjectPath}");
        var godotExecutablePath = CopyGodotExecutable(Environment.ProcessPath!, tempPath);
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
            OS.Execute(godotExecutablePath, dumpGodotClassCommands, result);
            Directory.Delete(tempPath, true);
            try
            {
                resultString = result[0].AsString();
            }
            catch (Exception e)
            {
                GD.PrintErr(e.ToString());
                return;
            }
        }

        if (ExtractClassNamesFromStdOut(resultString, out var builtinClassTypes)) return;

        var currentClassTypes = ClassDB.GetClassList();

        var filteredTypes = currentClassTypes
            .Except(builtinClassTypes)
            .Select(x => (StringName)x)
            .Where(ClassDB.CanInstantiate)
            .ToHashSet();

        foreach (var gdeClassName in filteredTypes)
        {
            GD.Print($"ClassNames:{gdeClassName} IsClassEnabled:{ClassDB.IsClassEnabled(gdeClassName)} GetParentClass:{ClassDB.GetParentClass(gdeClassName)}");
        }

        GD.Print($"currentClassTypes:{currentClassTypes.Length} builtinClassTypes:{builtinClassTypes.Count} diffType:{filteredTypes.Count}");

        //EditorInterface.Singleton.RestartEditor(true);
    }

    private static bool ExtractClassNamesFromStdOut(string resultString, out HashSet<string> builtinClassTypes)
    {
        var matchResult = GetExtractClassNameRegex().Match(resultString);
        if (!matchResult.Success)
        {
            builtinClassTypes = null;
            return true;
        }

        builtinClassTypes = matchResult
            .Groups["ClassNames"]
            .Value
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();
        return false;
    }

    private static string CopyGodotExecutable(string godotExecutablePath, string tempPath)
    {
        var godotExecutableDir = Path.GetDirectoryName(godotExecutablePath)!;
        var allGodotExecutableFiles = Directory.GetFiles(godotExecutableDir, "*.*", SearchOption.AllDirectories);
        var copyTasks = new Task[allGodotExecutableFiles.Length];
        for (var i = 0; i < allGodotExecutableFiles.Length; i++)
        {
            var sourcePath = allGodotExecutableFiles[i];
            copyTasks[i] = Task.Run(
                () =>
                {
                    var relativePath = Path.GetRelativePath(godotExecutableDir, sourcePath);
                    var destPath = Path.Combine(tempPath, relativePath);
                    var destDir = Path.GetDirectoryName(destPath);
                    if (!Directory.Exists(destDir))
                    {
#if GODOT_OSX || GODOT_LINUXBSD
                        Directory.CreateDirectory(destDir, UnixFileMode.UserExecute);
#else
                        Directory.CreateDirectory(destDir);
#endif
                    }

                    File.Copy(sourcePath, destPath);
                }
            );
        }

        Task.WhenAll(copyTasks).Wait();
        return Path.Combine(tempPath, Path.GetFileName(godotExecutablePath));
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

    private static string CreateDumpDBScript(string tempPath)
    {
        const string dumpDBScript =
            """
            @tool
            extends SceneTree
            func  _process(delta: float) -> bool:
            	print("WRAPPER_GENERATOR_DUMP_CLASS_DB_START");
            	for name in ClassDB.get_class_list():
            		print(name);
            	print("WRAPPER_GENERATOR_DUMP_CLASS_DB_END");
            	quit()
            	return true
            	
            """;
        const string dumpDBFileName = "dump_class_db.gd";
        var scriptFullPath = Path.Combine(tempPath, dumpDBFileName);
        File.WriteAllText(scriptFullPath, dumpDBScript);
        return scriptFullPath;
    }

    private static string CreateTempDirectory()
    {
        var tempPath = Path.GetTempFileName();
        File.Delete(tempPath);
        Directory.CreateDirectory(tempPath);
        return tempPath;
    }
}
#endif