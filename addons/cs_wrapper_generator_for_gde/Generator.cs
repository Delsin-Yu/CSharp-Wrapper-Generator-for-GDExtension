#if TOOLS

using System;
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
    static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }

    [GeneratedRegex(@"WRAPPER_GENERATOR_DUMP_CLASS_DB_START(?<ClassNames>.+?)WRAPPER_GENERATOR_DUMP_CLASS_DB_END",
        RegexOptions.Singleline | RegexOptions.NonBacktracking)]
    private static partial Regex GetExtractClassNameRegex();

    public static void Generate()
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

        var tempPath = Path.GetTempFileName();

        File.Delete(tempPath);

        Directory.CreateDirectory(tempPath);

        var scriptFullPath = Path.Combine(tempPath, dumpDBFileName);

        File.WriteAllText(scriptFullPath, dumpDBScript);

        var dumpGodotClassProcess = Environment.ProcessPath!;
        var newDumpGodotClassProcess = Path.Combine(tempPath, Path.GetFileName(dumpGodotClassProcess)!);
        var tempProjectPath = Path.Combine(tempPath, "project");
        Directory.CreateDirectory(tempProjectPath);
        var config = new ConfigFile();
        config.SetValue("", "config_version", 5);
        config.SetValue("application", "config/features", ProjectSettings.GetSetting("config/features"));
        config.SetValue("application", "config/name", "Empty Project");
        File.WriteAllText(Path.Combine(tempProjectPath, "project.godot"), config.ToString());
        config.Dispose();
        CopyDirectory(Path.GetDirectoryName(dumpGodotClassProcess), tempPath, true);
        dumpGodotClassProcess = newDumpGodotClassProcess;
        string[] dumpGodotClassCommands =
            ["--headless", "--script", scriptFullPath, "--editor", "-v", $"--path \"{tempProjectPath}\""];

        GD.Print($"""
                  Dumping Godot Builtin Classes...
                  Starting Godot Editor ({Path.GetFileName(Environment.ProcessPath)})
                  Command Line: {dumpGodotClassProcess} {string.Join(' ', dumpGodotClassCommands)}
                  """
        );

        string resultString;
        {
            using var result = new Godot.Collections.Array();
            OS.Execute(dumpGodotClassProcess, dumpGodotClassCommands, result);
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

        var matchResult = GetExtractClassNameRegex().Match(resultString);

        if (!matchResult.Success) return;

        var builtinClassTypes = matchResult
            .Groups["ClassNames"]
            .Value
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();


        var currentClassTypes = ClassDB.GetClassList();
        var diffType = currentClassTypes.Except(builtinClassTypes).Where(x => ClassDB.CanInstantiate(x)).ToHashSet();
        foreach (var gdeClassNames in diffType)
        {
            GD.Print($"ClassNames:{gdeClassNames} IsClassEnabled:{ClassDB.IsClassEnabled(gdeClassNames)} GetParentClass:{ClassDB.GetParentClass(gdeClassNames)}");
        }

        GD.Print($"currentClassTypes:{currentClassTypes.Length} builtinClassTypes:{builtinClassTypes.Count} diffType:{diffType.Count}");
        EditorInterface.Singleton.RestartEditor(true);
    }
}
#endif