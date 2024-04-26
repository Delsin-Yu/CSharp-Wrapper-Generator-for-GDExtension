using System;
using System.Collections.Concurrent;
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
        ICollection<string> godotBuiltinClassNames,
        ConcurrentDictionary<string, string> enumNameToConstantMap)
    {
        var builtCode = GenerateCode(
            gdeTypeInfo,
            gdeTypeMap,
            godotSharpTypeNameMap,
            godotBuiltinClassNames,
            enumNameToConstantMap
        );
        return (gdeTypeInfo.TypeName, builtCode);
    }

    private const string TAB1 = "    ";
    private const string TAB2 = TAB1 + TAB1;
    private const string TAB3 = TAB2 + TAB1;
    private const string TAB4 = TAB2 + TAB2;
    private const string TAB5 = TAB4 + TAB1;
    private const string TAB6 = TAB3 + TAB3;
    private const string NAMESPACE = "GDExtension.Wrappers";
    private const string VariantToInstanceMethodName = "Bind";
    private const string CreateInstanceMethodName = "Instantiate";
    private const string VariantToGodotObject = "As<GodotObject>()";
    private const string GDExtensionName = "GDExtensionName";

    private static string GenerateCode(
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, ClassInfo> gdeTypeMap,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        ICollection<string> godotBuiltinClassNames,
        ConcurrentDictionary<string, string> enumNameToConstantMap)
    {
        var codeBuilder = new StringBuilder();
        var displayTypeName = godotSharpTypeNameMap.GetValueOrDefault(gdeTypeInfo.TypeName, gdeTypeInfo.TypeName);
        var displayParentTypeName = godotSharpTypeNameMap.GetValueOrDefault(gdeTypeInfo.ParentType.TypeName, gdeTypeInfo.ParentType.TypeName);

        var isAbstract = !ClassDB.CanInstantiate(gdeTypeInfo.TypeName);

        var engineBaseType = GetEngineBaseType(gdeTypeInfo, godotBuiltinClassNames);

        var isRootWrapper = gdeTypeInfo.ParentType.TypeName == engineBaseType || godotBuiltinClassNames.Contains(gdeTypeInfo.ParentType.TypeName);

        engineBaseType = godotSharpTypeNameMap.GetValueOrDefault(engineBaseType, engineBaseType);

        var abstractKeyWord = isAbstract ? "abstract " : string.Empty;
        var newKeyWord = isRootWrapper ? string.Empty : "new ";

        codeBuilder.AppendLine(
            $$"""
              using System;
              using Godot;

              namespace {{NAMESPACE}};

              public {{abstractKeyWord}}partial class {{displayTypeName}} : {{displayParentTypeName}}
              {
              {{TAB1}}public {{newKeyWord}}static readonly StringName {{GDExtensionName}} = "{{gdeTypeInfo.TypeName}}";

              {{TAB1}}[Obsolete("Wrapper classes cannot be constructed with Ctor (it only instantiate the underlying {{engineBaseType}}), please use the {{CreateInstanceMethodName}}() method instead.")]
              {{TAB1}}protected {{displayTypeName}}() { }
              
              {{TAB1}}/// <summary>
              {{TAB1}}/// Creates an instance of the GDExtension <see cref="{{displayTypeName}}"/> type, and attaches the wrapper script to it.
              {{TAB1}}/// </summary>
              {{TAB1}}/// <returns>The wrapper instance linked to the underlying GDExtension type.</returns>
              {{TAB1}}public {{newKeyWord}}static {{displayTypeName}} {{CreateInstanceMethodName}}()
              {{TAB1}}{
              {{TAB2}}return {{STATIC_HELPER_CLASS}}.{{CreateInstanceMethodName}}<{{displayTypeName}}>({{GDExtensionName}});
              {{TAB1}}}
              
              {{TAB1}}/// <summary>
              {{TAB1}}/// Try to cast the script on the supplied <paramref name="godotObject"/> to the <see cref="{{displayTypeName}}"/> wrapper type,
              {{TAB1}}/// if no script has attached to the type, or the script attached to the type does not inherit the <see cref="{{displayTypeName}}"/> wrapper type,
              {{TAB1}}/// a new instance of the <see cref="{{displayTypeName}}"/> wrapper script will get attaches to the <paramref name="godotObject"/>.
              {{TAB1}}/// </summary>
              {{TAB1}}/// <remarks>The developer should only supply the <paramref name="godotObject"/> that represents the correct underlying GDExtension type.</remarks>
              {{TAB1}}/// <param name="godotObject">The <paramref name="godotObject"/> that represents the correct underlying GDExtension type.</param>
              {{TAB1}}/// <returns>The existing or a new instance of the <see cref="{{displayTypeName}}"/> wrapper script attached to the supplied <paramref name="godotObject"/>.</returns>
              {{TAB1}}public {{newKeyWord}}static {{displayTypeName}} {{VariantToInstanceMethodName}}(GodotObject godotObject)
              {{TAB1}}{
              {{TAB2}}return {{STATIC_HELPER_CLASS}}.{{VariantToInstanceMethodName}}<{{displayTypeName}}>(godotObject);
              {{TAB1}}}
              """
        );

        GenerateMembers(
            codeBuilder,
            gdeTypeInfo,
            gdeTypeMap,
            godotSharpTypeNameMap,
            godotBuiltinClassNames,
            enumNameToConstantMap,
            string.Empty
        );

        return codeBuilder.Append('}').ToString();
    }


    private static void GenerateMembers(
        StringBuilder codeBuilder,
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, ClassInfo> gdeTypeMap,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        ICollection<string> godotBuiltinClassNames,
        ConcurrentDictionary<string, string> enumConstantMap,
        string backingName
    )
    {
        var propertyInfoList = CollectPropertyInfo(gdeTypeInfo);
        var methodInfoList = CollectMethodInfo(gdeTypeInfo, propertyInfoList);
        var signalInfoList = CollectSignalInfo(gdeTypeInfo);
        var enumInfoList = CollectEnumInfo(gdeTypeInfo);
        var occupiedNames = new HashSet<string>();
        var enumsBuilder = new StringBuilder();
        var signalsBuilder = new StringBuilder();
        var propertiesBuilder = new StringBuilder();
        var methodsBuilder = new StringBuilder();
        
        ConstructProperties(occupiedNames, propertyInfoList, godotSharpTypeNameMap, propertiesBuilder, backingName);
        ConstructMethods(occupiedNames, methodInfoList, godotSharpTypeNameMap, gdeTypeMap, godotBuiltinClassNames, methodsBuilder, gdeTypeInfo, backingName);
        ConstructSignals(occupiedNames, signalInfoList, signalsBuilder, gdeTypeMap, godotSharpTypeNameMap, godotBuiltinClassNames, backingName);
        ConstructEnums(occupiedNames, enumInfoList, enumsBuilder, gdeTypeInfo, enumConstantMap);

        codeBuilder
            .Append(enumsBuilder)
            .Append(propertiesBuilder)
            .Append(signalsBuilder)
            .Append(methodsBuilder);
    }

    private const string UNRESOLVED_ENUM_HINT = "ENUM_HINT";
    private const string UNRESOLVED_ENUM_TEMPLATE = $"<UNRESOLVED_ENUM_TYPE>{UNRESOLVED_ENUM_HINT}</UNRESOLVED_ENUM_TYPE>";

    [GeneratedRegex(@"<UNRESOLVED_ENUM_TYPE>(?<EnumConstants>.*)<\/UNRESOLVED_ENUM_TYPE>")]
    private static partial Regex GetExtractUnResolvedEnumValueRegex();
    
    
    private readonly struct PropertyInfo
    {
        public readonly Variant.Type Type = Variant.Type.Nil;
        public readonly string NativeName;
        public readonly string ClassName;
        public readonly PropertyHint Hint = PropertyHint.None;
        public readonly string HintString;
        public readonly PropertyUsageFlags Usage = PropertyUsageFlags.Default;
        public readonly string TypeClass;

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
            if (Hint is PropertyHint.Enum && Type is Variant.Type.Int)
            {
                var enumCandidates = HintString.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                TypeClass = UNRESOLVED_ENUM_TEMPLATE.Replace(UNRESOLVED_ENUM_HINT, string.Join(',', enumCandidates));
            }
            else
            {
                TypeClass = ClassName;
                if (string.IsNullOrEmpty(TypeClass)) TypeClass = HintString;
                if (string.IsNullOrEmpty(TypeClass)) TypeClass = nameof(Variant);
            }
        }

        public bool IsGroupOrSubgroup => Usage.HasFlag(PropertyUsageFlags.Group) || Usage.HasFlag(PropertyUsageFlags.Subgroup);
        public bool IsVoid => Type is Variant.Type.Nil;
        public bool IsEnum => Hint is PropertyHint.Enum;
        
        public string GetTypeName() => VariantToTypeName(Type, Hint, TypeClass);

        public string GetPropertyName() => EscapeAndFormatName(NativeName);

        public string GetArgumentName() => EscapeAndFormatName(NativeName, true);

        public override string ToString() =>
            $"""
             PropertyInfo:
             {TAB1}{nameof(Type)}: {Type}
             {TAB1}{nameof(NativeName)}: {NativeName}
             {TAB1}{nameof(ClassName)}: {ClassName}
             {TAB1}{nameof(Hint)}: {Hint}
             {TAB1}{nameof(HintString)}: {HintString}
             {TAB1}{nameof(Usage)}: {Usage}
             {TAB1}{nameof(IsGroupOrSubgroup)}: {IsGroupOrSubgroup}
             {TAB1}{nameof(IsVoid)}: {IsVoid}
             {TAB1}{nameof(TypeClass)}: {TypeClass}
             """;
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

        public override string ToString() => $"""
                                              MethodInfo;
                                              {TAB1}{nameof(NativeName)}: {NativeName}, 
                                              {TAB1}{nameof(ReturnValue)}: {ReturnValue}, 
                                              {TAB1}{nameof(Flags)}: {Flags}, 
                                              {TAB1}{nameof(Id)}: {Id}, 
                                              {TAB1}{nameof(Arguments)}: 
                                              {TAB2}[
                                              {string.Join(',', Arguments.Select(x => TAB3 + x.ToString().ReplaceLineEndings($"\n{TAB3}")))}
                                              {TAB2}], 
                                              {TAB1}{nameof(DefaultArguments)}: 
                                              {TAB2}[
                                              {string.Join(',', DefaultArguments.Select(x => TAB3 + x.ToString().ReplaceLineEndings($"\n{TAB3}")))}
                                              {TAB2}]
                                              """;
    }

    private static string GetEngineBaseType(ClassInfo gdeTypeInfo, ICollection<string> builtinTypes)
    {
        while (true)
        {
            if (builtinTypes.Contains(gdeTypeInfo.TypeName)) return gdeTypeInfo.TypeName;
            gdeTypeInfo = gdeTypeInfo.ParentType;
        }
    }

    private static void BuildupMethodArguments(StringBuilder stringBuilder, PropertyInfo[] propertyInfos, IReadOnlyDictionary<string, string> godotsharpTypeNameMap)
    {
        for (var i = 0; i < propertyInfos.Length; i++)
        {
            var propertyInfo = propertyInfos[i];
            var typeName = propertyInfo.GetTypeName();
            if (propertyInfo.IsVoid && propertyInfo.Usage.HasFlag(PropertyUsageFlags.NilIsVariant))
            {
                typeName = "Variant?";
            }
            else
            {
                typeName = godotsharpTypeNameMap.GetValueOrDefault(typeName, typeName);
            }
            stringBuilder
                .Append(typeName)
                .Append(' ')
                .Append(propertyInfo.GetArgumentName());

            if (i != propertyInfos.Length - 1)
            {
                stringBuilder.Append(", ");
            }
        }
    }

    public static string VariantToTypeName(Variant.Type type, PropertyHint hint, string className) =>
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
            Variant.Type.Int => hint is not PropertyHint.Enum ? "int" : (string.IsNullOrWhiteSpace(className) ? "UNDEFINED_ENUM" : className),
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

    private static string EscapeAndFormatName(string sourceName, bool camelCase = false)
    {
        ArgumentOutOfRangeException.ThrowIfNullOrEmpty(sourceName);
        
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