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
    [GeneratedRegex(@"WRAPPER_GENERATOR_DUMP_CLASS_DB_START(?<ClassNames>.+?)WRAPPER_GENERATOR_DUMP_CLASS_DB_END", RegexOptions.Singleline | RegexOptions.NonBacktracking)]
    private static partial Regex GetExtractClassNameRegex(); 
    
    public static void Generate()
    {
        const string dumpDBScript =
            """
            extends SceneTree

            func _init() -> void:
            	print("WRAPPER_GENERATOR_DUMP_CLASS_DB_START");
            	for name in ClassDB.get_class_list():
            		print(name);
            	print("WRAPPER_GENERATOR_DUMP_CLASS_DB_END");
            	quit()
            """;
        const string dumpDBFileName = "dump_class_db.gd";

        var tempPath = Path.GetTempFileName();
        
        File.Delete(tempPath);
        
        Directory.CreateDirectory(tempPath);

        var scriptFullPath = Path.Combine(tempPath, dumpDBFileName);

        File.WriteAllText(scriptFullPath, dumpDBScript);
        
#if GODOT_LINUXBSD || GODOT_MACOS
        File.SetUnixFileMode(scriptFullPath, UnixFileMode.UserExecute);
#endif
        
        var dumpGodotClassProcess = Environment.ProcessPath!;
        string[] dumpGodotClassCommands = ["--headless", "--editor", "--script", scriptFullPath];

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
        
        if(!matchResult.Success) return;

        var builtinClassTypes = matchResult
            .Groups["ClassNames"]
            .Value
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        var currentClassTypes = ClassDB.GetClassList();

        foreach (var gdeClassNames in currentClassTypes.Except(builtinClassTypes))
        {
            GD.Print(gdeClassNames);
        }
    }
}
#endif