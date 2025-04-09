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
    internal class FileData
    {
        public string FileName { get; init; }
        public string Code { get; set; } = string.Empty;
    }
    
    internal static ConcurrentDictionary<string, ConcurrentBag<FileData>> GenerateWrappersForGDETypes(string[] gdeTypeNames, ICollection<string> godotBuiltinTypeNames, bool includeTests)
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

        foreach (var godotName in godotBuiltinTypeNames)
        {
            GenerateClassInheritanceMap(godotName, classInheritanceMap);
        }

        var generateTasks = new Task[gdeTypeNames.Length];
        
        // Run all the generate logic in parallel.

        var enumNameToConstantMap = new ConcurrentDictionary<string, string>();
        var files = new ConcurrentDictionary<string, ConcurrentBag<FileData>>();
        
        for (var index = 0; index < gdeTypeNames.Length; index++)
        {
            var gdeTypeInfo = classInheritanceMap[gdeTypeNames[index]];
            generateTasks[index] = Task.Run(
                () => GenerateSourceCodeForType(
                    includeTests,
                    gdeTypeInfo,
                    classNameMap,
                    classInheritanceMap,
                    godotBuiltinTypeNames,
                    enumNameToConstantMap,
                    files
                )
            );
        }
        
        Task.WhenAll(generateTasks).GetAwaiter().GetResult();

        files.GetOrAdd(GeneratorMain.WRAPPERS_DIR_NAME, _ => new()).Add(GenerateStaticHelper());
        
        PopulateBuiltinEnumTypes(enumNameToConstantMap);

        foreach (var fileCollection in files.Values)
        foreach (var data in fileCollection)
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
                    
                    // At this point it mean the enum value is not provided by the generator,
                    // Terrain3D is having this issue on its debug_level property, we fall back to long
                    return  $"long /*{unresolvedConstants}*/";
                }
            );

            // Some GDE declares their type as multi-type, in this case we find a common base type to use
            data.Code = GetExtractUnResolvedMultiClassValueRegex().Replace(
                data.Code,
                match =>
                {
                    var unresolvedConstants = match.Groups["MultiClassConstants"].Value.Replace(" ", "");
                    if (string.IsNullOrEmpty(unresolvedConstants)) return "ENUM_UNRESOLVED";
                    var split = unresolvedConstants
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    var commonBaseType = FindCommonBaseType(split, classInheritanceMap);
                    
                    return commonBaseType is null 
                        ? $"Godot.GodotObject /*{unresolvedConstants}*/" 
                        : $"{commonBaseType.TypeName} /*{unresolvedConstants}*/";
                }
            );
        }
        
        return files;
    }

    private static ClassInfo FindCommonBaseType(string[] typeNames, Dictionary<string, ClassInfo> inheritanceMap)
    {
        if (typeNames.Length == 0) return null;

        var typeChains = new List<HashSet<ClassInfo>>();

        foreach (var typeName in typeNames)
        {
            var typeChain = new HashSet<ClassInfo>();
            var currentType = typeName;
            while (currentType != null)
            {
                if (!inheritanceMap.TryGetValue(currentType, out var typeInfo)) return null;
                typeChain.Add(typeInfo);
                currentType = typeInfo.ParentType?.TypeName;
            }
            typeChains.Add(typeChain);
        }

        var commonBaseTypes = typeChains[0];

        foreach (var typeChain in typeChains.Skip(1))
        {
            commonBaseTypes.IntersectWith(typeChain);
        }
        
        return commonBaseTypes.OrderByDescending(x => x.GetInheritanceDepth()).FirstOrDefault();
    }

    private const string STATIC_HELPER_CLASS = "GDExtensionHelper";

    private static FileData GenerateStaticHelper()
    {
        const string sourceCode =
            $$"""
              using System;
              using System.Linq;
              using System.Reflection;
              using System.Collections.Concurrent;
              using Godot;

              public static class {{STATIC_HELPER_CLASS}}
              {
                  private static readonly ConcurrentDictionary<string, GodotObject> _instances = [];
                  private static readonly ConcurrentDictionary<Type,Variant> _scripts = [];
                  /// <summary>
                  /// Calls a static method within the given type.
                  /// </summary>
                  /// <param name="className">The type name.</param>
                  /// <param name="method">The method name.</param>
                  /// <param name="arguments">The arguments.</param>
                  /// <returns>The return value of the method.</returns>
                  public static Variant Call(StringName className, StringName method, params Variant[] arguments)
                  {
                      return _instances.GetOrAdd(className,InstantiateStaticFactory).Call(method, arguments);
                  }
                  
                  private static GodotObject InstantiateStaticFactory(string className) => ClassDB.Instantiate(className).As<GodotObject>();
                  
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
              #if DEBUG
                      if (!GodotObject.IsInstanceValid(godotObject)) throw new ArgumentException(nameof(godotObject),"The supplied GodotObject is not valid.");
              #endif
                      if (godotObject is T wrapperScript) return wrapperScript;
                      var type = typeof(T);
              #if DEBUG
                      var className = godotObject.GetClass();
                      if (!ClassDB.IsParentClass(type.Name, className)) throw new ArgumentException(nameof(godotObject),$"The supplied GodotObject {className} is not a {type.Name}.");
              #endif
                      var script =_scripts.GetOrAdd(type,GetScriptFactory);
                      var instanceId = godotObject.GetInstanceId();
                      godotObject.SetScript(script);
                      return (T)GodotObject.InstanceFromId(instanceId);
                  }
                  
                  private static Variant GetScriptFactory(Type type)
                  {
                      var scriptPath = type.GetCustomAttributes<ScriptPathAttribute>().FirstOrDefault();
                      return scriptPath is null ? null : ResourceLoader.Load(scriptPath.Path);
                  }
              
                  public static Godot.Collections.Array<T> Cast<[MustBeVariant]T>(Godot.Collections.Array<GodotObject> godotObjects) where T : GodotObject
                  {
                      return new Godot.Collections.Array<T>(godotObjects.Select(Bind<T>));
                  }
                  
                  /// <summary>
                  /// Creates an instance of the GDExtension <typeparam name="T"/> type, and attaches the wrapper script to it.
                  /// </summary>
                  /// <returns>The wrapper instance linked to the underlying GDExtension type.</returns>
                  public static T {{CreateInstanceMethodName}}<T>(StringName className) where T : GodotObject
                  {
                      return Bind<T>(ClassDB.Instantiate(className).As<GodotObject>());
                  }
              }
              """;

        return new()
        {
            FileName = $"_{STATIC_HELPER_CLASS}",
            Code = sourceCode
        };
    }

    private static bool IsGodotObjectChild(Type type)
    {
        if (type == null) return false;
        if (type == typeof(GodotObject)) return true;

        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType == typeof(GodotObject)) return true;
            baseType = baseType.BaseType;
        }

        return false;
    }

    private static Dictionary<string, string> GetGodotSharpTypeNameMap()
    {
        var baseDictionary = typeof(GodotObject)
            .Assembly
            .GetTypes()
            .Select(
                x =>
                {
                    var godotNativeName = x.GetCustomAttributes()
                        .OfType<GodotClassNameAttribute>()
                        .FirstOrDefault()?.Name ?? x.Name;

                    return (x, godotNativeName);
                }
            )
            .Where(x => IsGodotObjectChild(x.x))
            .DistinctBy(x => x.godotNativeName)
            .ToDictionary(x => x.godotNativeName, x => x.x.Name);

        baseDictionary.Add("Vector2", nameof(Vector2));
        baseDictionary.Add("Vector2i", nameof(Vector2I));
        baseDictionary.Add("Rect2", nameof(Rect2));
        baseDictionary.Add("Rect2i", nameof(Rect2I));
        baseDictionary.Add("Transform2D", nameof(Transform2D));
        baseDictionary.Add("Vector3", nameof(Vector3));
        baseDictionary.Add("Vector3i", nameof(Vector3I));
        baseDictionary.Add("Basis", nameof(Basis));
        baseDictionary.Add("Quaternion", nameof(Quaternion));
        baseDictionary.Add("Transform3D", nameof(Transform3D));
        baseDictionary.Add("AABB", nameof(Aabb));
        baseDictionary.Add("Color", nameof(Color));
        baseDictionary.Add("Plane", nameof(Plane));
        baseDictionary.Add("Vector4", nameof(Vector4));
        baseDictionary.Add("Vector4i", nameof(Vector4I));
        baseDictionary.Add("Projection", nameof(Projection));

        return baseDictionary;
    }

    private record ClassInfo(string TypeName, ClassInfo ParentType)
    {
        public int GetInheritanceDepth()
        {
            var depth = 0;
            var currentType = ParentType;
            while (currentType != null)
            {
                depth++;
                currentType = currentType.ParentType;
            }

            return depth;
        }
    }

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
        else if (className == nameof(Variant))
        {
            classInfo = new(className, null);
        }
        else
        {
            if (!classInheritanceMap.ContainsKey("Variant"))
                GenerateClassInheritanceMap("Variant", classInheritanceMap);
            classInfo = new(className, classInheritanceMap["Variant"]);
        }

        classInheritanceMap.TryAdd(className, classInfo);
    }
}