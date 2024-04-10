using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Godot;
using Godot.BindingsGenerator.ApiDump;

namespace GDExtensionAPIGenerator;

internal static class Generator
{
    public static void Generate(string godotPath, string projectPath)
    {
        GD.Print($"""
                 Dumping Godot Builtin Classes...
                 Starting Godot Editor ({godotPath})
                 
                 Command Line: {godotPath} --dump-extension-api --verbose --headless
                 
                 -----------------------Godot Message Start------------------------
                 """
                 );
        var dumpJsonCoreStartInfo =
            new ProcessStartInfo(
                godotPath,
                "--dump-extension-api --verbose --headless"
            ) { RedirectStandardOutput = true };
        
        var process = Process.Start(dumpJsonCoreStartInfo)!;
        process.WaitForExit();
        GD.Print("> " + process.StandardOutput.ReadToEnd().ReplaceLineEndings("\n> "));
        process.Dispose();

        
        var godotBuiltinClass = GetClasses();
        GD.Print($"""
                  -----------------------Godot Message End------------------------
                  
                  Finish Collecting Builtin Classes.

                  Dumping Godot Builtin Classes With GDExtensions...
                  Starting Godot Editor ({godotPath})
                  With Current Project Root ({projectPath})
                  Command Line: {godotPath} res://addons/cs_wrapper_generator_for_gde/empty_scene.tscn --path {projectPath} --dump-extension-api --headless --verbose --editor
                  
                  -----------------------Godot Message Start------------------------
                  """
                 );
        
        GD.Print("Dumping Godot Engine Classes With GDExtension...");
        var dumpJsonGDEStartInfo =
            new ProcessStartInfo(
                godotPath,
                $"res://addons/cs_wrapper_generator_for_gde/empty_scene.tscn" +
                $" --path {projectPath}" +
                $" --dump-extension-api" +
                $" --headless" +
                $" --verbose" +
                $" --editor"
            ) { RedirectStandardOutput = true };
        
        process = Process.Start(dumpJsonGDEStartInfo)!;
        process.WaitForExit();
        GD.Print("> " + process.StandardOutput.ReadToEnd().ReplaceLineEndings("\n> "));
        process.Dispose();

        var godotGDEClasses = GetClasses();
        
        GD.Print($"""
                  -----------------------Godot Message End------------------------
                  
                  Finish Collecting GDExtension Classes.
                  """
        );
        
        GD.Print($"""
                 Classes in extension_api.json from:
                 Godot Editor ({Path.GetFileName(godotPath)}): {godotBuiltinClass.Count}
                 Current Project ({Path.GetDirectoryName(projectPath)}): {godotGDEClasses.Count}
                 Class Count in ClassDB {ClassDB.GetClassList().Length}
                 """
                 );
    }

    private static Dictionary<string, GodotClassInfo> GetClasses()
    {
        const string extensionPath = "./extension_api.json";
        var fileStream = File.OpenRead(extensionPath);
        var dictionary = GodotApi.Deserialize(fileStream)!.Classes.ToDictionary(x => x.Name, x => x);
        fileStream.Dispose();
        File.Delete(extensionPath);
        return dictionary;
    }
}