#if TOOLS

using System;
using System.Diagnostics;
using System.IO;
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
        GenerateAsync()
            .ContinueWith(_ => Button.Disabled = false)
            .ConfigureAwait(true);
    }


    private static async Task GenerateAsync()
    {
        const string dumpDBScript =
            """
            extends MainLoop
            	
            func _process(delta: float) -> bool:
            	print("WRAPPER_GENERATOR_DUMP_CLASS_DB_START");
            	for name in ClassDB.get_class_list():
            		print(name);
            	print("WRAPPER_GENERATOR_DUMP_CLASS_DB_END");
            	return true;
            """;
        
        const string dumpDBFileName = "dump_class_db.gd";
        
        var tempPath = Path.GetTempFileName();

        File.Delete(tempPath);

        Directory.CreateDirectory(tempPath);

        var scriptFullPath = Path.Combine(tempPath, dumpDBFileName);

        await File.WriteAllTextAsync(scriptFullPath, dumpDBScript).ConfigureAwait(true);
        
        var dumpGodotClassProcess = Environment.ProcessPath!;
        var dumpGodotClassCommands =
            $"--script {scriptFullPath} ";
        
        GD.Print($"""
                 Dumping Godot Builtin Classes...
                 Starting Godot Editor ({Path.GetFileName(Environment.ProcessPath)})
                 
                 Command Line: {dumpGodotClassProcess} {dumpGodotClassCommands}
                 
                 -----------------------Godot Message Start------------------------
                 """
                 );

        var process = new Process();
        process.StartInfo.FileName = dumpGodotClassProcess;
        process.StartInfo.Arguments = dumpGodotClassCommands;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.WorkingDirectory = tempPath;
        
        process.Start();
        await process.WaitForExitAsync().ConfigureAwait(true);

        var output = process.StandardOutput.ReadToEnd();
        process.Dispose();
        
        GD.Print(output);
    }
}
#endif