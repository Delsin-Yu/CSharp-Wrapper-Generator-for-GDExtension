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

        GenerateCode(codeBuilder, gdeTypeInfo, gdeTypeMap, godotSharpTypeNameMap, godotBuiltinClassNames);

        return (gdeTypeInfo.TypeName, codeBuilder.ToString());
    }

    private const string TAB1 = "    ";
    private const string TAB2 = TAB1 + TAB1;
    private const string TAB3 = TAB2 + TAB1;
    private const string TAB4 = TAB2 + TAB2;
    private const string TAB5 = TAB4 + TAB1;
    private const string TAB6 = TAB3 + TAB3;
    private const string NAMESPACE = "GDExtension.Wrappers";
    private const string VariantToInstanceMethodName = "Bind";

    private static void GenerateCode(
        StringBuilder codeBuilder,
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, ClassInfo> gdeTypeMap,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        ICollection<string> godotBuiltinClassNames
    )
    {
        var displayTypeName = godotSharpTypeNameMap.GetValueOrDefault(gdeTypeInfo.TypeName, gdeTypeInfo.TypeName);
        var displayParentTypeName = godotSharpTypeNameMap.GetValueOrDefault(gdeTypeInfo.ParentType.TypeName, gdeTypeInfo.ParentType.TypeName);
        
        var engineBaseType = GetEngineBaseType(gdeTypeInfo, godotBuiltinClassNames);
        
        var isRootWrapper = gdeTypeInfo.ParentType.TypeName == engineBaseType || godotBuiltinClassNames.Contains(gdeTypeInfo.ParentType.TypeName);

        engineBaseType = godotSharpTypeNameMap.GetValueOrDefault(engineBaseType, engineBaseType);
        
        var newKeyWord = isRootWrapper ? string.Empty : "new ";
        
        codeBuilder.AppendLine(
            $$"""
              using System;
              using Godot;

              namespace {{NAMESPACE}};

              public partial class {{displayTypeName}} : {{displayParentTypeName}}
              {
              
              {{TAB1}}[Obsolete("Wrapper classes cannot be constructed with Ctor (it only instantiate the underlying {{engineBaseType}}), please use the Construct() method instead.")]
              {{TAB1}}protected {{displayTypeName}}() { }
              
              {{TAB1}}public {{newKeyWord}}static {{displayTypeName}} {{VariantToInstanceMethodName}}(Variant variant)
              {{TAB1}}{
              {{TAB2}}var godotObject = variant.As<GodotObject>();
              {{TAB2}}var instanceId = godotObject.GetInstanceId();
              {{TAB2}}godotObject.SetScript(ResourceLoader.Load("{{GeneratorMain.GetWrapperPath(displayTypeName)}}"));
              {{TAB2}}return ({{displayTypeName}})InstanceFromId(instanceId);
              {{TAB1}}}
              
              {{TAB1}}public {{newKeyWord}}static {{displayTypeName}} Instantiate() =>
              {{TAB2}}{{VariantToInstanceMethodName}}(ClassDB.Instantiate("{{gdeTypeInfo.TypeName}}"));
              
              """
        );

        GenerateMembers(
            codeBuilder,
            gdeTypeInfo,
            gdeTypeMap,
            godotSharpTypeNameMap,
            godotBuiltinClassNames,
            string.Empty
        );
        codeBuilder.Append('}');
    }


    private static void GenerateMembers(
        StringBuilder codeBuilder,
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, ClassInfo> gdeTypeMap,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        ICollection<string> godotBuiltinClassNames,
        string backingName
    )
    {
        var propertyInfoList = CollectPropertyInfo(gdeTypeInfo);
        var methodInfoList = CollectMethodInfo(gdeTypeInfo, propertyInfoList);
        var signalInfoList = CollectSignalInfo(gdeTypeInfo);
        var enumInfoList = CollectionEnumInfo(gdeTypeInfo);
        var occupiedNames = new HashSet<string>();
        ConstructEnums(occupiedNames, enumInfoList, codeBuilder, gdeTypeInfo);
        ConstructSignals(occupiedNames, signalInfoList, codeBuilder, gdeTypeMap, godotSharpTypeNameMap, godotBuiltinClassNames, backingName);
        ConstructProperties(occupiedNames, propertyInfoList, godotSharpTypeNameMap, codeBuilder, backingName);
        ConstructMethods(occupiedNames, methodInfoList, godotSharpTypeNameMap, gdeTypeMap, godotBuiltinClassNames, codeBuilder, backingName);
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

    private readonly struct MethodInfo
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

    private static string GetEngineBaseType(ClassInfo gdeTypeInfo, ICollection<string> builtinTypes)
    {
        while (true)
        {
            if (builtinTypes.Contains(gdeTypeInfo.TypeName)) return gdeTypeInfo.TypeName;
            gdeTypeInfo = gdeTypeInfo.ParentType;
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
    
    [GeneratedRegex(@"[0-9]+")]
    private static partial Regex EscapeNameDigitRegex();


    // TODO: Split escape types, some of these keywords are actually valid method argument name.
    private static readonly HashSet<string> _csKeyword =
    [
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
        "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
        "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
        "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
        "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc",
        "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
        "unsafe", "ushort", "using", "virtual", "void", "volatile", "while",
        "string", "object", "var", "dynamic", "yield", "add", "alias", "ascending", "async", "await", "by",
        "descending", "equals", "from", "get", "global", "group", "into", "join", "let", "nameof", "on", "orderby",
        "partial", "remove", "select", "set", "when", "where", "yield"
    ];
    
    public static string EscapeAndFormatName(string sourceName, bool camelCase = false)
    {
        var name = EscapeNameRegex()
            .Replace(sourceName, "_")
            .ToPascalCase();
        
        if (camelCase) name = ToCamelCase(name);
        
        if (_csKeyword.Contains(name)) name = $"@{name}";
        
        if (EscapeNameDigitRegex().IsMatch(name[..1])) name = $"_{name}";
        
        return name;
    }

    public static string ToCamelCase(string sourceName)
    {
        if (string.IsNullOrWhiteSpace(sourceName)) return sourceName;
        return sourceName[..1].ToLowerInvariant() + sourceName[1..];
    }
}