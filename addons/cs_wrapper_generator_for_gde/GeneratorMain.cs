using Godot;

#if TOOLS

namespace GDExtensionAPIGenerator;

internal static class GeneratorMain
{
    /// <summary>
    /// TODO: User configurable wrappers path? 
    /// </summary>
    public const string WRAPPERS_PATH = "res://GDExtensionWrappers/";
    
    /// <summary>
    /// TODO: This file extension is too long, finding an alternative? 
    /// </summary>
    public const string WRAPPERS_EXT = ".gdextension.cs";

    /// <summary>
    /// Gets the full path (starts from res://) for the given type name.
    /// </summary>
    public static string GetWrapperPath(string typeName) => 
        WRAPPERS_PATH + typeName + WRAPPERS_EXT;

    /// <summary>
    /// Core Generator logic
    /// </summary>
    public static void Generate()
    {
        // Launch the Godot Editor and dump all builtin types and GDExtension types.
        if(!TypeCollector.TryCollectGDExtensionTypes(out var gdeClassTypes, out var builtinTypeNames)) return;
        
        // Generate source codes for the GDExtension types.
        var generatedCode = CodeGenerator.GenerateWrappersForGDETypes(gdeClassTypes, builtinTypeNames);
        
        // Write the generated result to the filesystem, and call update.
        FileWriter.WriteResult(generatedCode);
        
        // Print the result.
        GD.Print($"Finish generating wrappers for the following classes: \n{string.Join('\n', gdeClassTypes)}");
    }
}
#endif