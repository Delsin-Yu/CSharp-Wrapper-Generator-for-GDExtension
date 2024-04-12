using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Godot;
using Godot.Collections;

namespace GDExtensionAPIGenerator;

internal static partial class CodeGenerator
{
    private static (string fileName, string fileContent) GenerateSourceCodeForType(
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        IReadOnlyDictionary<string, ClassInfo> gdeTypeMap,
        ICollection<string> godotBuiltinClassNames

    )
    {
        var codeBuilder = new StringBuilder();

        switch (GetBaseType(gdeTypeInfo))
        {
            case BaseType.Resource:
                GenerateCodeForResource(codeBuilder, gdeTypeInfo, gdeTypeMap, godotSharpTypeNameMap, godotBuiltinClassNames);
                break;
            case BaseType.RefCounted:
                GenerateCodeForNode(codeBuilder, gdeTypeInfo, gdeTypeMap, godotSharpTypeNameMap, godotBuiltinClassNames);
                break;
            case BaseType.Node:
                GenerateCodeForRefCounted(codeBuilder, gdeTypeInfo, gdeTypeMap, godotSharpTypeNameMap, godotBuiltinClassNames);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return ($"{gdeTypeInfo.TypeName}.gdextension.cs", codeBuilder.ToString());
    }
    
    private enum BaseType
    {
        Resource,
        RefCounted,
        Node
    }

    private static BaseType GetBaseType(ClassInfo classInfo)
    {
        if (ContainsParent(classInfo, nameof(Resource))) return BaseType.Resource;
        if (ContainsParent(classInfo, nameof(Node))) return BaseType.Node;
        return BaseType.RefCounted;
    }

    private static bool ContainsParent(ClassInfo classInfo, string parentName)
    {
        while (true)
        {
            var parentClass = classInfo.ParentType;
            if (parentClass == null) return false;
            if (parentClass.TypeName == parentName) return true;
            classInfo = parentClass;
        }
    }

    private const string TAB = "    ";
    private const string NAMESPACE_RES = "GDExtension.ResourcesWrappers";
    private const string NAMESPACE_RC = "GDExtension.RefCountedWrappers";
    private const string NAMESPACE_NODE = "GDExtension.NodeWrappers";

    private static void GenerateCodeForNode(
        StringBuilder codeBuilder,
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, ClassInfo> gdeTypeMap,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        ICollection<string> godotBuiltinClassNames
    )
    {
        var displayTypeName = godotSharpTypeNameMap.GetValueOrDefault(gdeTypeInfo.TypeName, gdeTypeInfo.TypeName);
        var displayParentTypeName = godotSharpTypeNameMap.GetValueOrDefault(gdeTypeInfo.ParentType.TypeName, gdeTypeInfo.ParentType.TypeName);
        
        codeBuilder.AppendLine(
            $$"""
              using Godot;

              namespace {{NAMESPACE_NODE}};

              public partial class {{displayTypeName}} : {{displayParentTypeName}}
              {
              """
        );

        var propertyInfoList = ConstructProperties(gdeTypeInfo, godotSharpTypeNameMap, codeBuilder, string.Empty);
        ConstructMethods(gdeTypeInfo, godotSharpTypeNameMap, gdeTypeMap, godotBuiltinClassNames, propertyInfoList, codeBuilder, string.Empty);

        codeBuilder.Append('}');
    }

    private static void GenerateCodeForRefCounted(
        StringBuilder codeBuilder,
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, ClassInfo> gdeTypeMap,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        ICollection<string> godotBuiltinClassNames
    )
    {
        var displayTypeName = godotSharpTypeNameMap.GetValueOrDefault(gdeTypeInfo.TypeName, gdeTypeInfo.TypeName);
        var displayParentTypeName = godotSharpTypeNameMap.GetValueOrDefault(gdeTypeInfo.ParentType.TypeName, gdeTypeInfo.ParentType.TypeName);
        
        const string backingName = "_backing";
        const string backingArgument = "backing";
        const string constructMethodName = "Construct";
        var refCountedName = nameof(RefCounted);

        var isRootWrapper = gdeTypeInfo.ParentType.TypeName == refCountedName || godotBuiltinClassNames.Contains(gdeTypeInfo.ParentType.TypeName);

        if (isRootWrapper)
        {
            codeBuilder.AppendLine(
                $$"""
                  using System;
                  using Godot;

                  namespace {{NAMESPACE_RC}};

                  public class {{displayTypeName}} : IDisposable
                  {
                  
                  {{TAB}}public static implicit operator Variant({{displayTypeName}} refCount) => refCount.{{backingName}};
                  
                  {{TAB}}protected virtual {{refCountedName}} {{constructMethodName}}() =>
                  {{TAB}}{{TAB}}({{refCountedName}})ClassDB.Instantiate("{{gdeTypeInfo.TypeName}}");
                  
                  {{TAB}}public {{displayTypeName}} {{constructMethodName}}({{refCountedName}} {{backingArgument}}) =>
                  {{TAB}}{{TAB}}new {{displayTypeName}}({{backingArgument}});
                  
                  {{TAB}}protected readonly {{refCountedName}} {{backingName}};
                  
                  {{TAB}}public {{displayTypeName}}() => {{backingName}} = {{constructMethodName}}();
                  
                  {{TAB}}private {{displayTypeName}}({{refCountedName}} {{backingArgument}}) => {{backingName}} = {{backingArgument}};
                  
                  {{TAB}}public void Dispose() => {{backingName}}.Dispose();
                  """
            );
        }
        else
        {
            codeBuilder.AppendLine(
                $$"""
                  using Godot;

                  namespace {{NAMESPACE_RC}};

                  public class {{displayTypeName}} : {{displayParentTypeName}}
                  {

                  {{TAB}}protected override {{refCountedName}} {{constructMethodName}}() =>
                  {{TAB}}{{TAB}}({{refCountedName}})ClassDB.Instantiate("{{gdeTypeInfo.TypeName}}");

                  """
            );
        }

        var propertyInfoList = ConstructProperties(gdeTypeInfo, godotSharpTypeNameMap, codeBuilder, $"{backingName}.");
        ConstructMethods(gdeTypeInfo, godotSharpTypeNameMap, gdeTypeMap, godotBuiltinClassNames, propertyInfoList, codeBuilder, $"{backingName}.");
        
        codeBuilder.Append('}');
    }

    
    private static void GenerateCodeForResource(
        StringBuilder codeBuilder,
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, ClassInfo> gdeTypeMap,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        ICollection<string> godotBuiltinClassNames
    )
    {
        var displayTypeName = godotSharpTypeNameMap.GetValueOrDefault(gdeTypeInfo.TypeName, gdeTypeInfo.TypeName);
        var displayParentTypeName = godotSharpTypeNameMap.GetValueOrDefault(gdeTypeInfo.ParentType.TypeName, gdeTypeInfo.ParentType.TypeName);
        
        const string backingName = "_backing";
        const string backingArgument = "backing";
        var resourceName = nameof(Resource);

        var isRootWrapper = gdeTypeInfo.ParentType.TypeName == resourceName || godotBuiltinClassNames.Contains(gdeTypeInfo.ParentType.TypeName);

        if (isRootWrapper)
        {
            codeBuilder.AppendLine(
                $$"""
                  using Godot;

                  namespace {{NAMESPACE_RES}};

                  public class {{displayTypeName}}
                  {
                  
                  {{TAB}}public static implicit operator Variant({{displayTypeName}} resource) => resource.{{backingName}};
                  
                  {{TAB}}protected readonly {{resourceName}} {{backingName}};

                  {{TAB}}public {{displayTypeName}}({{resourceName}} {{backingArgument}})
                  {{TAB}}{
                  {{TAB}}{{TAB}}{{backingName}} = {{backingArgument}};
                  {{TAB}}}

                  """
            );
        }
        else
        {
            codeBuilder.AppendLine(
                $$"""
                  using Godot;

                  namespace {{NAMESPACE_RES}};

                  public class {{displayTypeName}} : {{displayParentTypeName}}
                  {
                  
                  {{TAB}}public {{displayTypeName}}({{resourceName}} {{backingArgument}}) : base({{backingArgument}}) { }

                  """
            );
        }

        var propertyInfoList = ConstructProperties(gdeTypeInfo, godotSharpTypeNameMap, codeBuilder, $"{backingName}.");
        ConstructMethods(gdeTypeInfo, godotSharpTypeNameMap, gdeTypeMap, godotBuiltinClassNames, propertyInfoList, codeBuilder, $"{backingName}.");
        
        codeBuilder.Append('}');
    }

    private readonly struct PropertyInfo
    {
        public readonly Variant.Type Type = Variant.Type.Nil;
        public readonly string NativeName;
        public readonly string ClassName;
        public readonly PropertyHint Hint = PropertyHint.None;
        public readonly string HintString;
        public readonly PropertyUsageFlags Usage = PropertyUsageFlags.Default;

        public PropertyInfo(Dictionary dictionary)
        {
            using var nameInfo = dictionary["name"];
            using var classNameInfo = dictionary["class_name"];
            using var typeInfo = dictionary["type"];
            using var hintInfo = dictionary["hint"];
            using var hintStringInfo = dictionary["hint_string"];
            using var usageInfo = dictionary["usage"];
            
            Type = typeInfo.As<Variant.Type>();;
            NativeName = nameInfo.AsString();
            ClassName = classNameInfo.AsString(); 
            Hint = hintInfo.As<PropertyHint>();
            HintString = hintStringInfo.AsString();
            Usage = usageInfo.As<PropertyUsageFlags>();
        }

        public bool IsGroupOrSubgroup => Usage.HasFlag(PropertyUsageFlags.Group) || Usage.HasFlag(PropertyUsageFlags.Subgroup);
        public bool IsVoid => Type is Variant.Type.Nil;
        
        public string GetTypeName() => VariantToTypeName(Type, ClassName);
        public string GetPropertyName() => EscapeAndFormatName(NativeName);
        public string GetArgumentName() => EscapeAndFormatName(NativeName, true);
    }
    
    private static IReadOnlyList<PropertyInfo> ConstructProperties(
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        StringBuilder stringBuilder,
        string backing
    )
    {
        var propertyList = ClassDB.ClassGetPropertyList(gdeTypeInfo.TypeName, true);

        var propertyInfoList = new List<PropertyInfo>();
        
        foreach (var propertyDictionary in propertyList)
        {
            var propertyInfo = new PropertyInfo(propertyDictionary);
            propertyInfoList.Add(propertyInfo);

            if (propertyInfo.IsGroupOrSubgroup) continue;

            var typeName = propertyInfo.GetTypeName();

            godotSharpTypeNameMap.GetValueOrDefault(typeName, typeName);

            stringBuilder
                .AppendLine($"{TAB}public {typeName} {propertyInfo.GetPropertyName()}")
                .AppendLine($"{TAB}{{")
                .AppendLine($"""{TAB}{TAB}get => ({typeName}){backing}Get("{propertyInfo.NativeName}");""")
                .AppendLine($"""{TAB}{TAB}set => {backing}Set("{propertyInfo.NativeName}", Variant.From(value));""")
                .AppendLine($"{TAB}}}")
                .AppendLine();
            
            propertyDictionary.Dispose();
        }

        return propertyInfoList;
    }

    private struct MethodInfo
    {
        public readonly string NativeName;
        public readonly PropertyInfo ReturnValue;
        public readonly MethodFlags Flags;
        public readonly int Id = 0;
        public readonly PropertyInfo[] Arguments;
        public readonly Variant[] DefaultArguments;

        public MethodInfo(Dictionary dictionary)
        {
            using var nameInfo = dictionary["name"];
            using var argsInfo = dictionary["args"];
            using var defaultArgsInfo = dictionary["default_args"];
            using var flagsInfo = dictionary["flags"];
            using var idInfo = dictionary["id"];
            using var returnInfo = dictionary["return"];

            NativeName = nameInfo.AsString();
            ReturnValue = new(returnInfo.As<Dictionary>());
            Flags = flagsInfo.As<MethodFlags>();
            Id = idInfo.AsInt32();
            Arguments = argsInfo.As<Array<Dictionary>>().Select(x => new PropertyInfo(x)).ToArray();
            DefaultArguments = defaultArgsInfo.As<Array<Variant>>().ToArray();
        }
        
        public string GetMethodName() => EscapeAndFormatName(NativeName);
    }
    
    private static void ConstructMethods(
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        IReadOnlyDictionary<string, ClassInfo> gdeTypeMap,
        ICollection<string> builtinTypeNames,
        IReadOnlyList<PropertyInfo> propertyInfos,
        StringBuilder stringBuilder,
        string backing
    )
    {
        var methodList = ClassDB.ClassGetMethodList(gdeTypeInfo.TypeName, true);

        foreach (var methodDictionary in methodList)
        {
            var methodInfo = new MethodInfo(methodDictionary);

            var methodNativeName = methodInfo.NativeName;
            if (propertyInfos.Any(
                    x =>
                    {
                        var propertyNativeName = x.NativeName;
                        if (methodNativeName.Contains(propertyNativeName))
                        {
                            var index = methodNativeName.IndexOf(propertyNativeName, StringComparison.Ordinal);
                            var spiltResult = methodNativeName.Remove(index, propertyNativeName.Length);
                            if (spiltResult is "set_" or "get_") return true;
                        }
                        var propertyNativeNameEscaped = EscapeNameRegex().Replace(propertyNativeName, "_");
                        if (methodNativeName.Contains(propertyNativeNameEscaped))
                        {
                            var index = methodNativeName.IndexOf(propertyNativeNameEscaped, StringComparison.Ordinal);
                            var spiltResult = methodNativeName.Remove(index, propertyNativeNameEscaped.Length);
                            if (spiltResult is "set_" or "get_") return true;
                        }
                        return false;
                    }
                )) continue;
            var returnValueName = methodInfo.ReturnValue.GetTypeName();
            if (gdeTypeMap.TryGetValue(returnValueName, out var returnTypeInfo))
            {
                switch (returnTypeInfo.ParentType.TypeName)
                {
                    case nameof(Resource):
                        returnValueName = $"{NAMESPACE_RES}.{returnValueName}";
                        break;
                    case nameof(Node):
                        returnValueName = $"{NAMESPACE_NODE}.{returnValueName}";
                        break;
                    case nameof(RefCounted):
                        returnValueName = $"{NAMESPACE_RC}.{returnValueName}";
                        break;
                }
            }
            
            stringBuilder
                .Append($"{TAB}public ")
                .Append(returnValueName)
                .Append(' ')
                .Append(methodInfo.GetMethodName())
                .Append('(');
            
            BuildupMethodArguments(stringBuilder, methodInfo.Arguments);

            stringBuilder.Append(") => ");

            if (!methodInfo.ReturnValue.IsVoid && gdeTypeMap.TryGetValue(methodInfo.ReturnValue.ClassName, out returnTypeInfo))
            {
                stringBuilder.Append("new(");
            }
            
            stringBuilder
                .Append(backing)
                .Append("Call(\"")
                .Append(methodNativeName)
                .Append('"');

            if (methodInfo.Arguments.Length > 0)
            {
                stringBuilder.Append(", ");
                BuildupMethodCallArguments(stringBuilder, methodInfo.Arguments);
            }

            stringBuilder.Append(')');

            if (!methodInfo.ReturnValue.IsVoid)
            {
                if (gdeTypeMap.TryGetValue(methodInfo.ReturnValue.ClassName, out returnTypeInfo))
                {
                    /*
                         public JoltPhysicsServer3D CreateServer()
                        {
                            var baseType = Call("create_server").As<PhysicsServer3DExtension>();
                            baseType.SetScript(GD.Load(""));
                            return (JoltPhysicsServer3D)baseType;
                        }
                     */
                    
                    var interopType = GetRootParentType(returnTypeInfo, builtinTypeNames);
                    interopType = godotSharpTypeNameMap.GetValueOrDefault(interopType, interopType);
                    stringBuilder.Append($".As<{interopType}>())");
                }
                else
                {
                    stringBuilder.Append($".As<{methodInfo.ReturnValue.GetTypeName()}>()");
                }
            }
            
            stringBuilder.AppendLine(";").AppendLine();
            
            methodDictionary.Dispose();
        }
    }

    private static string GetRootParentType(ClassInfo gdeTypeInfo, ICollection<string> builtinTypes) =>
        GetBaseType(gdeTypeInfo) switch
        {
            BaseType.Resource => nameof(Resource),
            BaseType.RefCounted => nameof(RefCounted),
            BaseType.Node => GetParentGDERootParent(gdeTypeInfo, builtinTypes),
            _ => throw new ArgumentOutOfRangeException()
        };

    private static string GetParentGDERootParent(ClassInfo gdeTypeInfo, ICollection<string> builtinTypes)
    {
        while (true)
        {
            var parentType = gdeTypeInfo.ParentType;
            if (builtinTypes.Contains(parentType.TypeName)) return parentType.TypeName;
            gdeTypeInfo = parentType;
        }
    }

    private static void BuildupMethodArguments(StringBuilder stringBuilder, PropertyInfo[] propertyInfos)
    {
        for (var i = 0; i < propertyInfos.Length; i++)
        {
            var propertyInfo = propertyInfos[i];
            stringBuilder
                .Append(propertyInfo.GetTypeName())
                .Append(' ')
                .Append(propertyInfo.GetArgumentName());

            if (i != propertyInfos.Length - 1)
            {
                stringBuilder.Append(", ");
            }
        }
    }

    private static void BuildupMethodCallArguments(StringBuilder stringBuilder, PropertyInfo[] propertyInfos)
    {
        for (var i = 0; i < propertyInfos.Length; i++)
        {
            var propertyInfo = propertyInfos[i];
            stringBuilder.Append(propertyInfo.GetArgumentName());

            if (i != propertyInfos.Length - 1)
            {
                stringBuilder.Append(", ");
            }
        }  
    }
    
    public static string VariantToTypeName(Variant.Type type, string className) =>
        type switch
        {
            Variant.Type.Aabb => "Aabb",
            Variant.Type.Basis => "Basis",
            Variant.Type.Callable => "Callable",
            Variant.Type.Color => "Color",
            Variant.Type.NodePath => "NodePath",
            Variant.Type.Plane => "Plane",
            Variant.Type.Projection => "Projection",
            Variant.Type.Quaternion => "Quaternion",
            Variant.Type.Rect2 => "Rect2",
            Variant.Type.Rect2I => "Rect2I",
            Variant.Type.Rid => "Rid",
            Variant.Type.Signal => "Signal",
            Variant.Type.StringName => "StringName",
            Variant.Type.Transform2D => "Transform2D",
            Variant.Type.Transform3D => "Transform3D",
            Variant.Type.Vector2 => "Vector2",
            Variant.Type.Vector2I => "Vector2I",
            Variant.Type.Vector3 => "Vector3",
            Variant.Type.Vector3I => "Vector3I",
            Variant.Type.Vector4 => "Vector4",
            Variant.Type.Vector4I => "Vector4I",
            Variant.Type.Nil => "void",
            Variant.Type.PackedByteArray => "byte[]",
            Variant.Type.PackedInt32Array => "int[]",
            Variant.Type.PackedInt64Array => "long",
            Variant.Type.PackedFloat32Array => "float[]",
            Variant.Type.PackedFloat64Array => "double[]",
            Variant.Type.PackedStringArray => "string[]",
            Variant.Type.PackedVector2Array => "Vector2[]",
            Variant.Type.PackedVector3Array => "Vector3[]",
            Variant.Type.PackedColorArray => "Color[]",
            Variant.Type.Bool => "bool",
            Variant.Type.Int => "int",
            Variant.Type.Float => "float",
            Variant.Type.String => "string",
            Variant.Type.Object => className,
            Variant.Type.Dictionary => "Godot.Collections.Dictionary",
            Variant.Type.Array => "Godot.Collections.Array",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

    [GeneratedRegex(@"[^a-zA-Z0-9_]")]
    private static partial Regex EscapeNameRegex();
    private static HashSet<string> _csharp_keyword = new HashSet<string>()
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
        "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
        "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
        "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
        "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc",
        "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
        "unsafe", "ushort", "using", "virtual", "void", "volatile", "while",
        "string", "object", "var", "dynamic", "yield", "add", "alias", "ascending", "async", "await", "by",
        "descending", "equals", "from", "get", "global", "group", "into", "join", "let", "nameof", "on", "orderby",
        "partial", "remove", "select", "set","when", "where", "yield",
    };
    
    public static string EscapeAndFormatName(string sourceName, bool camelCase = false)
    {
        var pascalCaseName = EscapeNameRegex()
            .Replace(sourceName, "_")
            .ToPascalCase();

        if (camelCase && pascalCaseName.Length > 0)
        {
            pascalCaseName = pascalCaseName[..1].ToLowerInvariant() + pascalCaseName[1..];
        }
        if (_csharp_keyword.Contains(pascalCaseName))
        {
            pascalCaseName = $"@{pascalCaseName}";
        }
        return pascalCaseName;
    }
}