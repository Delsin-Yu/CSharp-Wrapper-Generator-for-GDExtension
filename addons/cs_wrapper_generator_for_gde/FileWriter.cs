using Godot;

namespace GDExtensionAPIGenerator;

internal static class FileWriter
{
    internal static void WriteResult((string fileName, string fileContent)[] codes)
    {
        const string wrapperPath = "res://GDExtensionWrappers/";

        DirAccess.MakeDirAbsolute(wrapperPath);
        
        foreach (var (fileName, fileContent) in codes)
        {
            if(fileContent is null) continue;
            using var fileAccess = FileAccess.Open($"{wrapperPath}{fileName}", FileAccess.ModeFlags.Write);
            fileAccess.StoreString(fileContent);
        }

        EditorInterface
            .Singleton
            .GetResourceFilesystem()
            .Scan();
    }
}