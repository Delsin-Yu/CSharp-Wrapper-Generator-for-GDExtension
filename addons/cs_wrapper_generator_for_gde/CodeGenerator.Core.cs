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
    private static void GenerateSourceCodeForType(
        bool includeTests,
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        IReadOnlyDictionary<string, ClassInfo> inheritanceMap,
        ICollection<string> godotBuiltinClassNames,
        ConcurrentDictionary<string, string> enumNameToConstantMap,
        ConcurrentDictionary<string, ConcurrentBag<FileData>> files) =>
        GenerateCode(
            includeTests,
            gdeTypeInfo,
            inheritanceMap,
            godotSharpTypeNameMap,
            godotBuiltinClassNames,
            enumNameToConstantMap,
            files
        );

    private const string TAB1 = "    ";
    private const string TAB2 = TAB1 + TAB1;
    private const string TAB3 = TAB2 + TAB1;
    private const string TAB4 = TAB2 + TAB2;
    private const string TAB5 = TAB4 + TAB1;
    private const string TAB6 = TAB3 + TAB3;
    private const string NAMESPACE = "GDExtension.Wrappers";
    private const string VariantToInstanceMethodName = "Bind";
    private const string CastMethodName = "Cast";
    private const string CreateInstanceMethodName = "Instantiate";
    private const string VariantToGodotObject = "As<GodotObject>()";
    private const string GDExtensionName = "GDExtensionName";

    private static void GenerateCode(
        bool includeTests,
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, ClassInfo> inheritanceMap,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        ICollection<string> godotBuiltinClassNames,
        ConcurrentDictionary<string, string> enumNameToConstantMap,
        ConcurrentDictionary<string, ConcurrentBag<FileData>> files)
    {
        var codeBuilder = new StringBuilder();
        var displayTypeName = godotSharpTypeNameMap.GetValueOrDefault(gdeTypeInfo.TypeName, gdeTypeInfo.TypeName);
        var displayParentTypeName = godotSharpTypeNameMap.GetValueOrDefault(gdeTypeInfo.ParentType.TypeName, gdeTypeInfo.ParentType.TypeName);

        var canInstantiate = !ClassDB.CanInstantiate(gdeTypeInfo.TypeName);

        var engineBaseType = GetEngineBaseType(gdeTypeInfo, godotBuiltinClassNames);

        var isRootWrapper = gdeTypeInfo.ParentType.TypeName == engineBaseType || godotBuiltinClassNames.Contains(gdeTypeInfo.ParentType.TypeName);

        engineBaseType = godotSharpTypeNameMap.GetValueOrDefault(engineBaseType, engineBaseType);

        var abstractKeyWord = canInstantiate ? "abstract " : string.Empty;
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
            canInstantiate,
            includeTests,
            codeBuilder,
            gdeTypeInfo,
            inheritanceMap,
            godotSharpTypeNameMap,
            godotBuiltinClassNames,
            enumNameToConstantMap,
            files,
            string.Empty
        );
    }

    private static string NativeNameToCachedName(string nativeName) => $"_cached_{nativeName}";
    
    private static void GenerateMembers(
        bool isAbstract,
        bool includeTests,
        StringBuilder codeBuilder,
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, ClassInfo> inheritanceMap,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        ICollection<string> godotBuiltinClassNames,
        ConcurrentDictionary<string, string> enumConstantMap,
        ConcurrentDictionary<string, ConcurrentBag<FileData>> files,
        string backingName)
    {
        var nativeNameCache = new HashSet<string>();
        
        var propertyInfoList = CollectPropertyInfo(gdeTypeInfo);
        var methodInfoList = CollectMethodInfo(gdeTypeInfo, propertyInfoList);
        var signalInfoList = CollectSignalInfo(gdeTypeInfo);
        var enumInfoList = CollectEnumInfo(gdeTypeInfo);
        var occupiedNames = new HashSet<string>();
        var enumsBuilder = new StringBuilder();
        var signalsBuilder = new StringBuilder();
        var propertiesBuilder = new StringBuilder();
        var methodsBuilder = new StringBuilder();

        ConstructProperties(occupiedNames, propertyInfoList, godotSharpTypeNameMap, inheritanceMap, nativeNameCache, propertiesBuilder, backingName);
        ConstructMethods(occupiedNames, methodInfoList, godotSharpTypeNameMap, inheritanceMap, godotBuiltinClassNames, nativeNameCache, methodsBuilder, gdeTypeInfo, backingName);
        ConstructSignals(occupiedNames, signalInfoList, signalsBuilder, inheritanceMap, godotSharpTypeNameMap, godotBuiltinClassNames, nativeNameCache, backingName);
        ConstructEnums(occupiedNames, enumInfoList, enumsBuilder, gdeTypeInfo, enumConstantMap);

        var cachedStringNameBuilder = new StringBuilder();

        foreach (var nativeName in nativeNameCache)
        {
            var cachedName = NativeNameToCachedName(nativeName);
            cachedStringNameBuilder.AppendLine($"{TAB1}private static readonly StringName {cachedName} = \"{nativeName}\";");
        }
        
        codeBuilder
            .Append(enumsBuilder)
            .Append(propertiesBuilder)
            .Append(signalsBuilder)
            .Append(methodsBuilder)
            .Append(cachedStringNameBuilder);
        
        files.GetOrAdd(GeneratorMain.WRAPPERS_DIR_NAME, _ => new())
            .Add(
                new()
                {
                    FileName = gdeTypeInfo.TypeName,
                    Code = codeBuilder.Append('}').ToString()
                }
            );
        
        if(!includeTests) return;
        var testCodeBuilder = new StringBuilder();

        var testTypeName = $"{gdeTypeInfo.TypeName}_Test";
        
        testCodeBuilder.AppendLine(
            $$"""
              using GDExtension.Wrappers;
              using GdUnit4;
              
              namespace GDExtensionAPIGenerator.Tests;

              [TestSuite]
              public class {{testTypeName}}
              {

              """
        );

        if (!isAbstract)
        {
            testCodeBuilder.AppendLine(
                $$"""
                  {{TAB1}}[TestCase]
                  {{TAB1}}public void Constructor()
                  {{TAB1}}{
                  {{TAB2}}var instance = {{NAMESPACE}}.{{gdeTypeInfo.TypeName}}.{{CreateInstanceMethodName}}();
                  {{TAB2}}instance.Free();
                  {{TAB1}}}
                  
                  """
            );
        }

        testCodeBuilder.AppendLine("}");
        
        files.GetOrAdd(GeneratorMain.WRAPPERSTest_DIR_NAME, _ => new())
            .Add(
                new()
                {
                    FileName = testTypeName,
                    Code = testCodeBuilder.ToString()
                }
            );
    }

    private const string UNRESOLVED_ENUM_HINT = "ENUM_HINT";
    private const string UNRESOLVED_ENUM_TEMPLATE = $"<UNRESOLVED_ENUM_TYPE>{UNRESOLVED_ENUM_HINT}</UNRESOLVED_ENUM_TYPE>";
    private const string UNRESOLVED_MULTICLASS = "MULTICLASS_HINT";
    private const string UNRESOLVED_MULTICLASS_TEMPLATE = $"<UNRESOLVED_MULTICLASS_TYPE>{UNRESOLVED_MULTICLASS}</UNRESOLVED_MULTICLASS_TYPE>";

    [GeneratedRegex(@"<UNRESOLVED_ENUM_TYPE>(?<EnumConstants>.*)<\/UNRESOLVED_ENUM_TYPE>")]
    private static partial Regex GetExtractUnResolvedEnumValueRegex();

    [GeneratedRegex(@"<UNRESOLVED_MULTICLASS_TYPE>(?<MultiClassConstants>.*)<\/UNRESOLVED_MULTICLASS_TYPE>")]
    private static partial Regex GetExtractUnResolvedMultiClassValueRegex();

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

            Type = typeInfo.As<Variant.Type>();
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
                if (TypeClass.Contains(',')) TypeClass = UNRESOLVED_MULTICLASS_TEMPLATE.Replace(UNRESOLVED_MULTICLASS, TypeClass);
                if (string.IsNullOrEmpty(TypeClass))
                {
                    if (IsArray && HintString.Contains(':'))
                    {
                        TypeClass = HintString[(HintString.IndexOf(':') + 1)..];
                    }
                    else if (IsArray && HintString == "unsupported format character")
                    {
                        TypeClass = nameof(Variant);
                    }
                    else
                    {
                        TypeClass = HintString;
                    }
                }
                if (string.IsNullOrEmpty(TypeClass)) TypeClass = nameof(Variant);
            }
        }

        public bool IsGroupOrSubgroup => Usage.HasFlag(PropertyUsageFlags.Group) || Usage.HasFlag(PropertyUsageFlags.Subgroup);
        public bool IsVoid => Type is Variant.Type.Nil;
        public bool IsEnum => Hint is PropertyHint.Enum;
        public bool IsArray => Hint is PropertyHint.ArrayType && Type == Variant.Type.Array;

        public string GetTypeName() => VariantToTypeName(Type, Hint, TypeClass);

        public string GetPropertyName() => EscapeAndFormatName(NativeName);

        public string GetArgumentName() => EscapeAndFormatName(NativeName, true);

#if GODOT4_4_OR_GREATER
        public bool IsProperty(string methodName) => methodName == PropertyGetter || methodName == PropertySetter;
        public string PropertyGetter => ClassDB.ClassGetPropertyGetter(ClassName, NativeName);
        public string PropertySetter => ClassDB.ClassGetPropertySetter(ClassName, NativeName);
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
             {TAB1}{nameof(PropertyGetter)}: {PropertyGetter}
             {TAB1}{nameof(PropertySetter)}: {PropertySetter}
             """;

#else
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
#endif
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
                                              {TAB1}[
                                              {string.Join(",\n", Arguments.Select(x => TAB2 + x.ToString().ReplaceLineEndings($"\n{TAB2}")))}
                                              {TAB1}], 
                                              {TAB1}{nameof(DefaultArguments)}: 
                                              {TAB1}[
                                              {string.Join(",\n", DefaultArguments.Select(x => TAB2 + x.ToString().ReplaceLineEndings($"\n{TAB2}")))}
                                              {TAB1}]
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
                if (propertyInfo.IsArray)
                {
                    typeName = typeName.Replace("Godot.GodotObject", godotsharpTypeNameMap.GetValueOrDefault(propertyInfo.TypeClass, propertyInfo.TypeClass));
                }
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
            Variant.Type.Array => hint is PropertyHint.ArrayType ? $"Godot.Collections.Array<Godot.GodotObject>" : "Godot.Collections.Array",
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