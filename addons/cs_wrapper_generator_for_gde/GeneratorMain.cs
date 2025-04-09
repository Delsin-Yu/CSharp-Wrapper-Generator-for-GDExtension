using Godot;

#if TOOLS

namespace GDExtensionAPIGenerator;

internal static class GeneratorMain
{
    public const string WRAPPERS_DIR_NAME = "GDExtensionWrappers";
    public const string WRAPPERSTest_DIR_NAME = "GDExtensionWrappers.Tests";
    
    public const string WRAPPERS_EXT = ".cs";

    /// <summary>
    /// Gets the full path (starts from res://) for the given type name.
    /// </summary>
    public static string GetWrapperPath(string dir, string typeName) => 
        dir + typeName + WRAPPERS_EXT;

    /// <summary>
    /// Core Generator logic
    /// </summary>
    public static void Generate(bool includeTests)
    {
        // Launch the Godot Editor and dump all builtin types and GDExtension types.
        if(!TypeCollector.TryCollectGDExtensionTypes(out var gdeClassTypes, out var builtinTypeNames)) return;
        
        // Generate source codes for the GDExtension types.
        var generatedCode = CodeGenerator.GenerateWrappersForGDETypes(gdeClassTypes, builtinTypeNames, includeTests);
        
        // Write the generated result to the filesystem, and call update.
        FileWriter.WriteResult(generatedCode);
        
        // Print the result.
        GD.Print($"Finish generating wrappers for the following classes: \n{string.Join('\n', gdeClassTypes)}");
    }
}
#endif