using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;

namespace GDExtensionAPIGenerator;

internal static partial class CodeGenerator
{
    internal static IReadOnlyList<(string fileName, string fileContent)> GenerateWrappersForGDETypes(string[] gdeTypeNames, ICollection<string> godotBuiltinTypeNames)
    {
        // Certain types are named differently in C#,
        // such as GodotObject(C#) vs Object(Native),
        // here we create a map for converting the
        // native type name to C# type name.
        
        var classNameMap = GetGodotSharpTypeNameMap();
        var classInheritanceMap = new Dictionary<string, ClassInfo>();

        // We need to know the inheritance of the
        // GDExtension types for correctly generate
        // wrappers for them.
        
        foreach (var gdeTypeName in gdeTypeNames)
        {
            GenerateClassInheritanceMap(gdeTypeName, classInheritanceMap);
        }

        var generateTasks = new Task<(string, string)>[gdeTypeNames.Length];

        var nonGdeTypes = new List<string>();
        foreach (var gdeNameCandidate in classInheritanceMap.Keys)
        {
            var nameCandidate = gdeNameCandidate;
            if (godotBuiltinTypeNames.Contains(nameCandidate))
            {
                nonGdeTypes.Add(nameCandidate);
                continue;
            }
            nameCandidate = classNameMap.GetValueOrDefault(nameCandidate, nameCandidate);
            if (godotBuiltinTypeNames.Contains(nameCandidate))
            {
                nonGdeTypes.Add(nameCandidate);
            }
        }
        
        foreach (var builtinTypeName in nonGdeTypes)
        {
            classInheritanceMap.Remove(builtinTypeName);
        }
        
        // Run all the generate logic in parallel.
        
        for (var index = 0; index < gdeTypeNames.Length; index++)
        {
            var gdeTypeInfo = classInheritanceMap[gdeTypeNames[index]];
            generateTasks[index] = Task.Run(() => GenerateSourceCodeForType(gdeTypeInfo, classNameMap, classInheritanceMap, godotBuiltinTypeNames));
        }

        var generated = Task.WhenAll(generateTasks).Result.ToList();
        generated.Add(GenerateStaticHelper());
        return generated;
    }


    private const string STATIC_HELPER_CLASS = "StaticMethod";
    private const string METHOD_BLOCKER = "in int? _ = null";

    private static (string, string) GenerateStaticHelper()
    {
        var sourceCode =
            $$"""
            using Godot;
            
            public static class {{STATIC_HELPER_CLASS}}
            {
                private static readonly System.Collections.Generic.Dictionary<string, GodotObject> _instances = [];
            
                public static Variant Call(string className, string method, params Variant[] arguments)
                {
                    if (!_instances.TryGetValue(className, out var instance))
                    {
                        instance = ClassDB.Instantiate(className).AsGodotObject();
                        _instances[className] = instance;
                    }
            
                    return instance.Call(method, arguments);
                }
            }
            """;

        return (STATIC_HELPER_CLASS, sourceCode);
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