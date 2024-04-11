using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;

namespace GDExtensionAPIGenerator;

internal static partial class CodeGenerator
{
    internal static (string fileName, string fileContent)[] GenerateWrappersForGDETypes(string[] gdeTypeNames)
    {
        var classNameMap = GetGodotSharpTypeNameMap();
        var classInheritanceMap = new Dictionary<string, ClassInfo>();

        foreach (var gdeTypeName in gdeTypeNames)
        {
            GenerateClassInheritanceMap(gdeTypeName, classInheritanceMap);
        }

        var generateTasks = new Task<(string, string)>[gdeTypeNames.Length];

        for (var index = 0; index < gdeTypeNames.Length; index++)
        {
            var gdeTypeInfo = classInheritanceMap[gdeTypeNames[index]];
            generateTasks[index] = Task.Run(() => GenerateSourceCodeForType(gdeTypeInfo, classNameMap));
        }

        var whenAll = Task.WhenAll(generateTasks);
        return whenAll.Result;
    }

    private static Dictionary<string, string> GetGodotSharpTypeNameMap()
    {
        return typeof(GodotObject)
            .Assembly
            .GetTypes()
            .Select(
                x => (x,
                    x.GetCustomAttributes()
                        .OfType<GodotClassNameAttribute>()
                        .FirstOrDefault())
            )
            .Where(x => x.Item2 is not null)
            .DistinctBy(x => x.Item2)
            .ToDictionary(x => x.Item2.Name, x => x.x.Name);
    }

    private record ClassInfo(string TypeName, ClassInfo ParentType);

    private static void GenerateClassInheritanceMap(
        string className,
        IDictionary<string, ClassInfo> classInheritanceMap
    )
    {
        ClassInfo classInfo;
        var parentTypeName = className == nameof(GodotObject) ? string.Empty : (string)ClassDB.GetParentClass(className);
        if (!string.IsNullOrWhiteSpace(parentTypeName))
        {
            if (!classInheritanceMap.ContainsKey(parentTypeName))
                GenerateClassInheritanceMap(parentTypeName, classInheritanceMap);
            classInfo = new(className, classInheritanceMap[parentTypeName]);
        }
        else
        {
            classInfo = new(className, null);
        }

        classInheritanceMap.TryAdd(className, classInfo);
    }
}