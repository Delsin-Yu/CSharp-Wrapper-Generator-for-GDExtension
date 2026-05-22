#if TOOLS
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Godot;
using FileAccess = Godot.FileAccess;

namespace GDExtensionAPIGenerator;

public partial class WrapperGeneratorMain
{
    public const string DefaultNamespace = "GDExtension.Wrappers";
    public const string DefaultOutputPath = "GDExtensionWrappers";

    public readonly record struct WrapperGeneratorOptions(
        string Namespace,
        string OutputPathRelative,
        bool UseTabs,
        bool ExposeInternalMembers);

    public readonly record struct WrapperGeneratorRunResult(
        int ExitCode,
        string[] Warnings,
        string EscapedNamespace,
        string EscapedOutputPathRelative);

    public static class WrapperGeneratorRunner
    {
        public static WrapperGeneratorRunResult Run(WrapperGeneratorOptions options)
        {
            var warnings = new ConcurrentBag<string>();

            TypeCollector.CreateClassDiagram(options.ExposeInternalMembers, out var gdExtensionTypes, warnings);

            var files = new ConcurrentBag<FileConstruction>();

            var escapedNamespace = EscapeNamespaceKeyWords(options.Namespace);
            var indent = options.UseTabs ? "\t" : "    ";

            gdExtensionTypes.AsParallel().ForAll(type => TypeWriter.WriteType(type, escapedNamespace, indent, files, warnings));
            files.Add(TypeWriter.CreateEnumExtensionsFile(escapedNamespace));

            var escapedPath = EscapePath(options.OutputPathRelative);

            var godotTargetPath = $"res://{escapedPath}";
            var outputDir = ProjectSettings.GlobalizePath(godotTargetPath);
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir!);
            else
            {
                foreach (var path in Directory.GetFiles(outputDir, "*.cs"))
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception e)
                    {
                        GD.PrintErr($"Unable to delete old wrapper file at path {path}, exception: {e}");
                        return Failed(warnings, escapedNamespace, escapedPath);
                    }
                }
            }

            foreach (var file in files)
            {
                var targetPath = godotTargetPath.PathJoin(file.FileName);
                using var fileAccess = FileAccess.Open(targetPath, FileAccess.ModeFlags.Write);
                if (fileAccess is null)
                {
                    GD.PrintErr($"Unable to write wrapper file at path {targetPath}");
                    return Failed(warnings, escapedNamespace, escapedPath);
                }

                fileAccess.StoreString(file.SourceCode);
            }

            foreach (var uidPath in Directory.GetFiles(outputDir, "*.uid"))
            {
                var directory = Path.GetDirectoryName(uidPath);
                if (directory is null) continue;
                var originalFileName = Path.GetFileName(uidPath);
                if (originalFileName.GetExtension() != "cs") continue;
                if (File.Exists(Path.Combine(directory, originalFileName))) continue;
                try
                {
                    File.Delete(uidPath);
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Unable to delete old wrapper uid file at path {uidPath}, exception: {e}");
                }
            }

            var warningArray = warnings.ToHashSet().Order().ToArray();
            return new WrapperGeneratorRunResult(0, warningArray, escapedNamespace, escapedPath);
        }

        private static WrapperGeneratorRunResult Failed(
            ConcurrentBag<string> warnings,
            string escapedNamespace,
            string escapedPath) =>
            new(1, warnings.ToHashSet().Order().ToArray(), escapedNamespace, escapedPath);
    }
}
#endif
