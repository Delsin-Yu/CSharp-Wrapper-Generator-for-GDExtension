#if TOOLS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Godot;
using Environment = System.Environment;
using FileAccess = Godot.FileAccess;

namespace GDExtensionAPIGenerator;

internal static partial class GeneratorMain
{
    private const string GENERATOR_DUMP_HEADER = "WRAPPER_GENERATOR_DUMP_CLASS_DB_START";
    private const string GENERATOR_DUMP_FOOTER = "WRAPPER_GENERATOR_DUMP_CLASS_DB_END";
    
    [GeneratedRegex(
        $"{GENERATOR_DUMP_HEADER}(?<ClassNames>.+?){GENERATOR_DUMP_FOOTER}",
        RegexOptions.Singleline | RegexOptions.NonBacktracking
    )]
    private static partial Regex GetExtractClassNameRegex();

    public static void Generate()
    {
        var tempPath = CreateTempDirectory();
        GD.Print($"Temp workspace directory: {tempPath}");
        var scriptFullPath = CreateDumpDBScript(tempPath);
        GD.Print($"Godot Core ClassDB Dump script path: {scriptFullPath}");
        var dummyProjectPath = CreateDummyProject(tempPath);
        GD.Print($"Dummy Project path: {dummyProjectPath}");
        var godotExecutablePath = CopyGodotExecutable(Environment.ProcessPath!, tempPath);
        GD.Print($"Godot Executable path: {godotExecutablePath}");
        string[] dumpGodotClassCommands = ["--headless", "--script", scriptFullPath, "--editor", "--verbose", "--path", dummyProjectPath];

        GD.Print(
            $"""
             Dumping Godot Builtin Classes...
             Starting Godot Editor ({Path.GetFileName(Environment.ProcessPath)})
             Command Line: {godotExecutablePath} {string.Join(' ', dumpGodotClassCommands)}
             """
        );

        string resultString;
        using (var result = new Godot.Collections.Array())
        {
            OS.Execute(godotExecutablePath, dumpGodotClassCommands, result);
            Directory.Delete(tempPath, true);
            try
            {
                resultString = result[0].AsString();
            }
            catch (Exception e)
            {
                GD.PrintErr(e.ToString());
                return;
            }
        }

        if (ExtractClassNamesFromStdOut(resultString, out var builtinClassTypes)) return;

        var currentClassTypes = ClassDB.GetClassList();

        var filteredTypes = currentClassTypes
            .Except(builtinClassTypes)
            .Select(x => (StringName)x)
            .Where(ClassDB.CanInstantiate)
            .ToArray();

        var generateSourceCodeTasks = new Task<(string fileName, string fileContent)>[filteredTypes.Length];

        for (var index = 0; index < filteredTypes.Length; index++)
        {
            var gdeClassName = filteredTypes[index];
            generateSourceCodeTasks[index] = Task.Run(() => GenerateSourceCodeForClassName(gdeClassName));
        }

        var whenAllTask = Task.WhenAll(generateSourceCodeTasks);
        whenAllTask.Wait();

        const string wrapperPath = "res://GDExtensionWrappers/";

        DirAccess.MakeDirAbsolute(wrapperPath);
        
        foreach (var (fileName, fileContent) in whenAllTask.Result)
        {
            using var fileAccess = FileAccess.Open($"{wrapperPath}{fileName}", FileAccess.ModeFlags.Write);
            fileAccess.StoreString(fileContent);
        }

        EditorInterface
            .Singleton
            .GetResourceFilesystem()
            .Scan();
    }

    private static (string fileName, string fileContent) GenerateSourceCodeForClassName(string className)
    {
        const string indentation = "    ";
        
        var scriptBuilder = new StringBuilder();

        const string namespaceDefinition = "GDExtension.Wrappers";
        
        var classNameMap = GeneratorMain.GetGodotSharpTypeNameMap();

        var parentTypeName = ClassDB.GetParentClass(className);

        parentTypeName = classNameMap.GetValueOrDefault(parentTypeName, parentTypeName);
        
        scriptBuilder.AppendLine(
            $$"""
             using Godot;
             
             namespace {{namespaceDefinition}};
             
             public partial class {{className}} : {{parentTypeName}}
             {
             """
        );
        
        var propertyList = ClassDB.ClassGetPropertyList(className, true);

        foreach (var propertyDictionary in propertyList)
        {
            if (!PropertyGenerator.TryGenerate(
                    classNameMap,
                    propertyDictionary,
                    out var generatedPropertyCode
                )) continue;
            scriptBuilder.AppendLine($"{indentation}{generatedPropertyCode.ReplaceLineEndings($"\n{indentation}")}");
        }

        scriptBuilder.Append('}');
        return ($"{className}.gdextension.wrapper.cs" ,scriptBuilder.ToString());
    }

    private static bool ExtractClassNamesFromStdOut(string resultString, out HashSet<string> builtinClassTypes)
    {
        var matchResult = GetExtractClassNameRegex().Match(resultString);
        if (!matchResult.Success)
        {
            builtinClassTypes = null;
            return true;
        }

        builtinClassTypes = matchResult
            .Groups["ClassNames"]
            .Value
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();
        return false;
    }

    private static string CopyGodotExecutable(string godotExecutablePath, string tempPath)
    {
        var godotExecutableDir = Path.GetDirectoryName(godotExecutablePath)!;
        var allGodotExecutableFiles = Directory.GetFiles(godotExecutableDir, "*.*", SearchOption.AllDirectories);
        var copyTasks = new Task[allGodotExecutableFiles.Length];
        for (var i = 0; i < allGodotExecutableFiles.Length; i++)
        {
            var sourcePath = allGodotExecutableFiles[i];
            copyTasks[i] = Task.Run(
                () =>
                {
                    var relativePath = Path.GetRelativePath(godotExecutableDir, sourcePath);
                    var destPath = Path.Combine(tempPath, relativePath);
                    var destDir = Path.GetDirectoryName(destPath)!;
                    if (!Directory.Exists(destDir))
                    {
#if GODOT_OSX || GODOT_LINUXBSD
                        Directory.CreateDirectory(destDir!, UnixFileMode.UserExecute);
#else
                        Directory.CreateDirectory(destDir!);
#endif
                    }

                    File.Copy(sourcePath, destPath);
                }
            );
        }

        Task.WhenAll(copyTasks).Wait();
        return Path.Combine(tempPath, Path.GetFileName(godotExecutablePath));
    }

    private static string CreateDummyProject(string tempPath)
    {
        var dummyProjectPath = Path.Combine(tempPath, "project");
        Directory.CreateDirectory(dummyProjectPath);
        using var config = new ConfigFile();
        config.SetValue(string.Empty, "config_version", 5);
        config.SetValue("application", "config/features", ProjectSettings.GetSetting("config/features"));
        config.SetValue("application", "config/name", "Empty Project");
        File.WriteAllText(Path.Combine(dummyProjectPath, "project.godot"), config.ToString());

        return dummyProjectPath;
    }

    private static string CreateDumpDBScript(string tempPath)
    {
        const string dumpDBScript =
            $"""
            @tool
            extends SceneTree
            func  _process(delta: float) -> bool:
            	print("{GENERATOR_DUMP_HEADER}");
            	for name in ClassDB.get_class_list():
            		print(name);
            	print("{GENERATOR_DUMP_FOOTER}");
            	quit()
            	return true
            """;
        
        const string dumpDBFileName = "dump_class_db.gd";
        var scriptFullPath = Path.Combine(tempPath, dumpDBFileName);
        File.WriteAllText(scriptFullPath, dumpDBScript);
        return scriptFullPath;
    }

    private static string CreateTempDirectory()
    {
        var tempPath = Path.GetTempFileName();
        File.Delete(tempPath);
        Directory.CreateDirectory(tempPath);
        return tempPath;
    }
}
#endif