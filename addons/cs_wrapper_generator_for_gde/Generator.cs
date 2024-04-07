#if TOOLS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Godot;
using Godot.BindingsGenerator.ApiDump;
using Environment = System.Environment;

namespace GDExtensionAPIGenerator;

internal static class Generator
{
    public static void Generate()
    {
        GD.Print($"""
                 Dumping Godot Builtin Classes...
                 Starting Godot Editor ({Path.GetFileName(Environment.ProcessPath)})
                 
                 Command Line: {Environment.ProcessPath} --dump-extension-api --verbose --headless
                 
                 -----------------------Godot Message Start------------------------
                 """
                 );
        var dumpJsonCoreStartInfo =
            new ProcessStartInfo(
                Environment.ProcessPath!,
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
                  Starting Godot Editor ({Path.GetFileName(Environment.ProcessPath)})
                  With Current Project Root ({Path.GetFullPath("./")})
                  Command Line: {Environment.ProcessPath} res://addons/cs_wrapper_generator_for_gde/empty_scene.tscn --path {Path.GetFullPath("./")} --dump-extension-api --headless --verbose --editor
                  
                  -----------------------Godot Message Start------------------------
                  """
                 );
        
        GD.Print("Dumping Godot Engine Classes With GDExtension...");
        var dumpJsonGDEStartInfo =
            new ProcessStartInfo(
                Environment.ProcessPath!,
                $"res://addons/cs_wrapper_generator_for_gde/empty_scene.tscn" +
                $" --path {Path.GetFullPath("./")}" +
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
                 Godot Editor ({Path.GetFileName(Environment.ProcessPath)}): {godotBuiltinClass.Count}
                 Current Project ({Path.GetDirectoryName(Path.GetFullPath("./"))}): {godotGDEClasses.Count}
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

    public static void UnloadSystemTextJson()
    {
        var assembly = typeof(JsonSerializerOptions).Assembly;
        var updateHandlerType = assembly.GetType("System.Text.Json.JsonSerializerOptionsUpdateHandler");
        var clearCacheMethod = updateHandlerType?.GetMethod("ClearCache", BindingFlags.Static | BindingFlags.Public);
        clearCacheMethod?.Invoke(null, new object[] { null }); 
    }
}
#endif