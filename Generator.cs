using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Godot;
using Godot.BindingsGenerator.ApiDump;

namespace GDExtensionAPIGenerator;

internal static class Generator
{
    public static bool CheckGodotPath(string path, out string? errorMessage)
    {
        const string validateMessage = "Hello from Godot Engine.";        
        
        const string validationScript =
            $"""
            extends SceneTree
            
            func _init():
            	print("{validateMessage}")
            	quit()
            """;

        const string validationFileName = "Validation.gd";

        File.WriteAllText(validationFileName, validationScript);
        
        var validationStartInfo =
            new ProcessStartInfo(
                path,
                $"--headless " +
                $"--script {Path.GetFullPath(validationFileName)} " +
                $"--quit" 
            ) { RedirectStandardOutput = true };

        bool isGodot;
        try
        {
            var process = Process.Start(validationStartInfo)!;
            process.WaitForExit();
            isGodot = process.StandardOutput.ReadToEnd().Contains(validateMessage, StringComparison.Ordinal);
        }
        catch (Exception e)
        {
            errorMessage = $"The specified Godot Executable Path is not valid!\n" +
                           $"{e.GetType().Name}:" +
                           $"\n    {e.Message}";
            return false;
        }
    
        if (!isGodot)
        {
            errorMessage = "The specified Godot Executable Path is not valid!";
            return false;
        }
        
        //File.Delete(validationFileName);

        errorMessage = null;
        return true;
    }
    
    public static bool CheckProjectPath(string path, out string? errorMessage)
    {
        if (Directory.GetFiles(path, "project.godot", SearchOption.TopDirectoryOnly).Length != 1)
        {
            errorMessage = "The specified Godot Project Path is not valid!";
            return false;
        }

        errorMessage = null;
        return true;
    }
    
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
                " --dump-extension-api" +
                " --verbose" +
                " --headless"
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
                  Command Line: {godotPath} --path {projectPath} --dump-extension-api --headless --verbose --editor --quit
                  
                  -----------------------Godot Message Start------------------------
                  """
                 );
        
        GD.Print("Dumping Godot Engine Classes With GDExtension...");
        var dumpJsonGDEStartInfo =
            new ProcessStartInfo(
                godotPath,
                $" --path {projectPath}" +
                $" --dump-extension-api" +
                $" --headless" + 
                $" --verbose" +
                $" --editor" +
                $" --quit"
            )
            {
                RedirectStandardOutput = true,
                WorkingDirectory = projectPath
            };
        
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
                 Current Project ({Path.GetFileName(projectPath)}): {godotGDEClasses.Count}
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