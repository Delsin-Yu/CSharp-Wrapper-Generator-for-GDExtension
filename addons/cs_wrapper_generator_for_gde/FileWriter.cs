using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;

namespace GDExtensionAPIGenerator;

internal static class FileWriter
{
    internal static void WriteResult(ConcurrentDictionary<string, ConcurrentBag<CodeGenerator.FileData>> codes)
    {

        foreach (var (dir, files) in codes)
        {
            var path = $"res://{dir}/";
            DirAccess.MakeDirAbsolute(path);
            foreach (var fileData in files)
            {
                if (fileData.Code is null) continue;
                using var fileAccess = FileAccess.Open(GeneratorMain.GetWrapperPath(path, fileData.FileName), FileAccess.ModeFlags.Write);
                fileAccess.StoreString(fileData.Code);
            }
        }

        EditorInterface
            .Singleton
            .GetResourceFilesystem()
            .Scan();
    }
}