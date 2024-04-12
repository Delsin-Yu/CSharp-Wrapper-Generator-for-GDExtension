using Godot;

namespace GDExtensionAPIGenerator;

internal static class FileWriter
{
    internal static void WriteResult((string fileName, string fileContent)[] codes)
    {
        DirAccess.MakeDirAbsolute(GeneratorMain.WRAPPERS_PATH);
        
        foreach (var (fileName, fileContent) in codes)
        {
            if(fileContent is null) continue;
            using var fileAccess = FileAccess.Open(GeneratorMain.GetWrapperPath(fileName), FileAccess.ModeFlags.Write);
            fileAccess.StoreString(fileContent);
        }

        EditorInterface
            .Singleton
            .GetResourceFilesystem()
            .Scan();
    }
}