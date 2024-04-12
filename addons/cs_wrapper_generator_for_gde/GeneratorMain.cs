using Godot;

#if TOOLS

namespace GDExtensionAPIGenerator;

internal static class GeneratorMain
{
    public const string WRAPPERS_PATH = "res://GDExtensionWrappers/";
    public const string WRAPPERS_EXT = ".gdextension.cs";

    public static string GetWrapperPath(string fileNameWithOutExtension) => 
        WRAPPERS_PATH + fileNameWithOutExtension + WRAPPERS_EXT;

    public static void Generate()
    {
        if(!TypeCollector.TryCollectGDExtensionTypes(out var gdeClassTypes, out var builtinTypeNames)) return;
        var generatedCode = CodeGenerator.GenerateWrappersForGDETypes(gdeClassTypes, builtinTypeNames);
        FileWriter.WriteResult(generatedCode);
        GD.Print($"Finish generating wrappers for the following classes: \n{string.Join('\n', gdeClassTypes)}");
    }
}
#endif