using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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

        var enumNameToConstantMap = new ConcurrentDictionary<string, string>();
        
        for (var index = 0; index < gdeTypeNames.Length; index++)
        {
            var gdeTypeInfo = classInheritanceMap[gdeTypeNames[index]];
            generateTasks[index] = Task.Run(
                () => GenerateSourceCodeForType(
                    gdeTypeInfo,
                    classNameMap,
                    classInheritanceMap,
                    godotBuiltinTypeNames,
                    enumNameToConstantMap
                )
            );
        }
        
        var generated = Task.WhenAll(generateTasks).Result.ToList();
        generated.Add(GenerateStaticHelper());

        PopulateBuiltinEnumTypes(enumNameToConstantMap);

        var span = CollectionsMarshal.AsSpan(generated);

        foreach (ref (string FileName, string Code) data in span)
        {
            data.Code = GetExtractUnResolvedEnumValueRegex().Replace(
                data.Code,
                match =>
                {
                    var unresolvedConstants = match.Groups["EnumConstants"].Value.Replace(" ", "");
                    if (string.IsNullOrEmpty(unresolvedConstants)) return "ENUM_UNRESOLVED";
                    var split = unresolvedConstants
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(x=>EscapeAndFormatName(x));
                    
                    foreach (var enumValue in split)
                    {
                        if (!enumNameToConstantMap.TryGetValue(enumValue, out var enumName)) continue;
                        if (enumName == null) return $"long /*{unresolvedConstants}*/";
                        return enumName;
                    }
                    
                    return "ENUM_UNRESOLVED"; 
                }
            );
        }
        
        return generated;
    }


    private const string STATIC_HELPER_CLASS = "GDExtensionHelper";

    private static (string, string) GenerateStaticHelper()
    {
        var sourceCode =
            $$"""
            using System.Reflection;
            using Godot;
            
            public static class {{STATIC_HELPER_CLASS}}
            {
                private static readonly System.Collections.Generic.Dictionary<string, GodotObject> _instances = [];
            
                /// <summary>
                /// Calls a static method within the given type.
                /// </summary>
                /// <param name="className">The type name.</param>
                /// <param name="method">The method name.</param>
                /// <param name="arguments">The arguments.</param>
                /// <returns>The return value of the method.</returns>
                public static Variant Call(string className, string method, params Variant[] arguments)
                {
                    if (!_instances.TryGetValue(className, out var instance))
                    {
                        instance = ClassDB.Instantiate(className).AsGodotObject();
                        _instances[className] = instance;
                    }
            
                    return instance.Call(method, arguments);
                }
                
                /// <summary>
                /// Try to cast the script on the supplied <paramref name="godotObject"/> to the <typeparamref name="T"/> wrapper type,
                /// if no script has attached to the type, or the script attached to the type does not inherit the <typeparamref name="T"/> wrapper type,
                /// a new instance of the <typeparamref name="T"/> wrapper script will get attaches to the <paramref name="godotObject"/>.
                /// </summary>
                /// <remarks>The developer should only supply the <paramref name="godotObject"/> that represents the correct underlying GDExtension type.</remarks>
                /// <param name="godotObject">The <paramref name="godotObject"/> that represents the correct underlying GDExtension type.</param>
                /// <returns>The existing or a new instance of the <typeparamref name="T"/> wrapper script attached to the supplied <paramref name="godotObject"/>.</returns>
                public static T {{VariantToInstanceMethodName}}<T>(GodotObject godotObject) where T : GodotObject
                {
                    if (godotObject is T wrapperScript) return wrapperScript;
                    var instanceId = godotObject.GetInstanceId();
                    godotObject.SetScript(ResourceLoader.Load(typeof(T).GetCustomAttribute<ScriptPathAttribute>()!.Path));
                    return (T)GodotObject.InstanceFromId(instanceId);
                }
                
                /// <summary>
                /// Creates an instance of the GDExtension <typeparam name="T"/> type, and attaches the wrapper script to it.
                /// </summary>
                /// <returns>The wrapper instance linked to the underlying GDExtension type.</returns>
                public static T {{CreateInstanceMethodName}}<T>(string className) where T : GodotObject
                {
                    return Bind<T>(ClassDB.Instantiate(className).As<GodotObject>());
                }
            }
            """;

        return ($"_{STATIC_HELPER_CLASS}", sourceCode);
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