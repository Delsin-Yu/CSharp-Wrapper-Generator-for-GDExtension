using Godot;

#if TOOLS

namespace GDExtensionAPIGenerator;

internal static partial class GeneratorMain
{
    public static void Generate()
    {
        if(!TypeCollector.TryCollectGDExtensionTypes(out var gdeClassTypes, out var builtinTypeNames)) return;
        var generatedCode = CodeGenerator.GenerateWrappersForGDETypes(gdeClassTypes, builtinTypeNames);
        FileWriter.WriteResult(generatedCode);
        GD.Print($"Finish generating wrappers for the following classes: \n{string.Join('\n', gdeClassTypes)}");
    }
}
#endif