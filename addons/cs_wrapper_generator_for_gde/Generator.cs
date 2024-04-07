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
        // Process
        //     .Start(Environment.ProcessPath!, "--dump-extension-api --headless")!
        //     .WaitForExit();
        //
        // Dictionary<string, GodotClassInfo> godotBuiltinClass = GetClasses();
        //
        Process
            .Start(Environment.ProcessPath!, $"res://addons/cs_wrapper_generator_for_gde/empty_scene.tscn --path {Path.GetFullPath("./")} --dump-extension-api --headless")!
            .WaitForExit();

        Dictionary<string, GodotClassInfo> godotGDEClasses = GetClasses();

        foreach (var (key, godotClassInfo) in godotGDEClasses)
        {
            GD.Print(key);
        }
        
        // GD.Print(godotGDEClasses.Count - godotBuiltinClass.Count);
        //
        // foreach (var gdeClassName in godotGDEClasses.Keys.Except(godotBuiltinClass.Keys))
        // {
        //     var gdeClassInfo = godotGDEClasses[gdeClassName];
        //     GD.Print(gdeClassName);
        // }
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