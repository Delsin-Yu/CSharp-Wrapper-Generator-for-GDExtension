#if TOOLS

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Environment = System.Environment;

namespace GDExtensionAPIGenerator;

internal static class Generator
{
    public static Button Button { get; set; }

    public static void Generate()
    {
        Button.Disabled = true;
        GenerateSync();
        Button.Disabled = false;
    }


    private static void GenerateSync()
    {
        const string dumpDBScript =
            """
            #!/usr/bin/env -S godot -s
            @tool
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

        var dumpGodotClassProcess = Environment.ProcessPath!;
        var dumpGodotClassCommands =
            $"--headless  -s {scriptFullPath}";

        GD.Print($"""
                  Dumping Godot Builtin Classes...
                  Starting Godot Editor ({Path.GetFileName(Environment.ProcessPath)})
                  Command Line: {dumpGodotClassProcess} {dumpGodotClassCommands}

                  -----------------------Godot Message Start------------------------
                  """
        );


        var result = new Godot.Collections.Array();
        OS.Execute(dumpGodotClassProcess, dumpGodotClassCommands.Split(" "), result);
        var output     = result.SelectMany(x => x.AsString().Split(Environment.NewLine)).ToArray();
        var startIndex = Array.IndexOf(output, "WRAPPER_GENERATOR_DUMP_CLASS_DB_START") + 1;
        var endIndex   = Array.IndexOf(output, "WRAPPER_GENERATOR_DUMP_CLASS_DB_END");
        output = output[startIndex..endIndex];
        foreach (var str in output)
        {
            GD.Print(str);
        }


    }
}
#endif