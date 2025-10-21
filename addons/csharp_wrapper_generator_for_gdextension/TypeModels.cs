using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Godot;

namespace GDExtensionAPIGenerator;

public partial class WrapperGeneratorMain
{
    #region Models

    private const string __ = "    ";

    [DebuggerTypeProxy(typeof(GodotClassTypeDebugView))]
    private record GodotClassType(
        GodotName GodotTypeName,
        CSharpName CSharpTypeName,
        ClassDB.ApiType ApiType,
        bool CanInstantiate
    ) : GodotNamedType(GodotTypeName, CSharpTypeName)
    {
        public int GetInheritanceDepth()
        {
            var depth = 0;
            var currentType = ParentType;
            while (currentType != null)
            {
                depth++;
                currentType = (currentType as GodotClassType)?.ParentType;
            }

            return depth;
        }

        public GodotNamedType ParentType { get; set; }
        public List<GodotFunctionInfo> Methods { get; } = [];
        public List<GodotFunctionInfo> Signals { get; } = [];
        public List<GodotClassPropertyInfo> Properties { get; } = [];
        public List<GodotEnumType> Enums { get; } = [];

        public override string ToString() => $"Class<{GodotTypeName}>";

        public override HashSet<Variant.Type> Accepts { get; } = [Variant.Type.Object, Variant.Type.Nil];

        private class GodotClassTypeDebugView(GodotClassType godotClassType)
        {
            public List<GodotFunctionInfo> Methods => godotClassType.Methods;
            public List<GodotFunctionInfo> Signals => godotClassType.Signals;
            public List<GodotClassPropertyInfo> Properties => godotClassType.Properties;
            public List<GodotEnumType> Enums => godotClassType.Enums;

            // ReSharper disable once InconsistentNaming
            public GodotNamedType _ParentType => godotClassType.ParentType;
        }

        private const string BindMethodName = "Bind";
        private const string TypeGDExtensionCacheName = "NativeName";
        private const string WrapperConstructorName = "Instantiate";

        public void RenderClass(StringBuilder classBuilder, string nameSpace, GenerationLogger logger)
        {
            classBuilder.Append(
                $"""
                 #pragma warning disable CS0109
                 using System;
                 using System.Diagnostics;
                 using System.Linq;
                 using System.Reflection;
                 using Godot;
                 using Godot.Collections;

                 namespace {nameSpace};

                 public 
                 """
            );

            if (!CanInstantiate) classBuilder.Append("abstract ");

            var parentTypeName = ParentType switch
            {
                GodotClassType parentClassType => parentClassType.CSharpTypeName.String,
                GodotAnnotatedVariantType { VariantType: Variant.Type.Object } => nameof(GodotObject),
                _ => throw new UnreachableException()
            };

            classBuilder.AppendLine(
                $$"""
                  partial class {{CSharpTypeName}} : {{parentTypeName}}
                  {

                  {{__}}private new static readonly StringName {{TypeGDExtensionCacheName}} = new StringName("{{GodotTypeName}}");

                  {{__}}[Obsolete("Wrapper types cannot be constructed with constructors (it only instantiate the underlying {{CSharpTypeName}} object), please use the {{WrapperConstructorName}}() method instead.")]
                  {{__}}protected {{CSharpTypeName}}() { }

                  {{__}}private static CSharpScript _wrapperScriptAsset;

                  {{__}}/// <summary>
                  {{__}}/// Try to cast the script on the supplied <paramref name="godotObject"/> to the <see cref="{{CSharpTypeName}}"/> wrapper type,
                  {{__}}/// if no script has attached to the type, or the script attached to the type does not inherit the <see cref="{{CSharpTypeName}}"/> wrapper type,
                  {{__}}/// a new instance of the <see cref="{{CSharpTypeName}}"/> wrapper script will get attaches to the <paramref name="godotObject"/>.
                  {{__}}/// </summary>
                  {{__}}/// <remarks>The developer should only supply the <paramref name="godotObject"/> that represents the correct underlying GDExtension type.</remarks>
                  {{__}}/// <param name="godotObject">The <paramref name="godotObject"/> that represents the correct underlying GDExtension type.</param>
                  {{__}}/// <returns>The existing or a new instance of the <see cref="{{CSharpTypeName}}"/> wrapper script attached to the supplied <paramref name="godotObject"/>.</returns>
                  {{__}}public new static {{CSharpTypeName}} {{BindMethodName}}(GodotObject godotObject)
                  {{__}}{
                  #if DEBUG
                  {{__ + __}}if (!IsInstanceValid(godotObject))
                  {{__ + __ + __}}throw new InvalidOperationException("The supplied GodotObject instance is not valid.");
                  #endif
                  {{__ + __}}if (godotObject is {{CSharpTypeName}} wrapperScriptInstance)
                  {{__ + __ + __}}return wrapperScriptInstance;

                  #if DEBUG
                  {{__ + __}}var expectedType = typeof({{CSharpTypeName}});
                  {{__ + __}}var currentObjectClassName = godotObject.GetClass();
                  {{__ + __}}if (!ClassDB.IsParentClass(expectedType.Name, currentObjectClassName))
                  {{__ + __ + __}}throw new InvalidOperationException($"The supplied GodotObject ({currentObjectClassName}) is not the {expectedType.Name} type.");
                  #endif

                  {{__ + __}}if (_wrapperScriptAsset is null)
                  {{__ + __}}{
                  {{__ + __ + __}}var scriptPathAttribute = typeof({{CSharpTypeName}}).GetCustomAttributes<ScriptPathAttribute>().FirstOrDefault();
                  {{__ + __ + __}}if (scriptPathAttribute is null) throw new UnreachableException();
                  {{__ + __ + __}}_wrapperScriptAsset = ResourceLoader.Load<CSharpScript>(scriptPathAttribute.Path);
                  {{__ + __}}}

                  {{__ + __}}var instanceId = godotObject.GetInstanceId();
                  {{__ + __}}godotObject.SetScript(_wrapperScriptAsset);
                  {{__ + __}}return ({{CSharpTypeName}})InstanceFromId(instanceId);
                  {{__}}}

                  """
            );

            if (CanInstantiate)
            {
                classBuilder.AppendLine(
                    $"""
                     {__}/// <summary>
                     {__}/// Creates an instance of the GDExtension <see cref="{CSharpTypeName}"/> type, and attaches a wrapper script instance to it.
                     {__}/// </summary>
                     {__}/// <returns>The wrapper instance linked to the underlying GDExtension "{GodotTypeName}" type.</returns>
                     {__}public new static {CSharpTypeName} {WrapperConstructorName}() => {BindMethodName}(ClassDB.Instantiate({TypeGDExtensionCacheName}).As<GodotObject>());

                     """
                );
            }

            foreach (var enumInfo in Enums)
            {
                enumInfo.RenderEnum(classBuilder, logger);
                classBuilder.AppendLine();
            }

            RenderCacheString(
                classBuilder,
                "GDExtensionSignalName",
                Signals,
                info => (info.CSharpFunctionName, info.GodotFunctionName)
            );

            foreach (var signalInfo in Signals)
            {
                signalInfo.RenderSignal(classBuilder, logger);
                classBuilder.AppendLine();
            }

            RenderCacheString(
                classBuilder,
                "GDExtensionPropertyName",
                Properties,
                info => (info.CSharpPropertyName, info.GodotPropertyName)
            );

            foreach (var propertyInfo in Properties)
            {
                propertyInfo.RenderProperty(classBuilder, logger);
                classBuilder.AppendLine();
            }

            RenderCacheString(
                classBuilder,
                "GDExtensionMethodName",
                Methods,
                info => (info.CSharpFunctionName, info.GodotFunctionName)
            );

            foreach (var methodInfo in Methods)
            {
                methodInfo.RenderMethod(classBuilder, logger);
                classBuilder.AppendLine();
            }

            classBuilder
                .AppendLine("}");
        }

        private static void RenderCacheString<T>(StringBuilder builder, string className, IList<T> elements, Func<T, (string CSharpName, string GodotName)> selector)
        {
            if (elements.Count == 0) return;

            builder.AppendLine(
                $$"""
                  {{__}}public new static class {{className}}
                  {{__}}{
                  """
            );

            foreach (var element in elements)
            {
                var (elementCSharpName, elementGodotName) = selector(element);
                builder.AppendLine($"{__ + __}public new static readonly StringName {elementCSharpName} = \"{elementGodotName}\";");
            }

            builder.AppendLine(
                $$"""
                  {{__}}}

                  """
            );
        }
    }

    private record GodotAnnotatedVariantType(
        GodotName GodotTypeName,
        CSharpName CSharpTypeName,
        Variant.Type VariantType
    ) : GodotNamedType(GodotTypeName, CSharpTypeName)
    {
        public override HashSet<Variant.Type> Accepts { get; } = VariantType switch
        {
            Variant.Type.Nil => [],
            Variant.Type.Bool => [VariantType],
            Variant.Type.Int => [VariantType],
            Variant.Type.Float => [VariantType, Variant.Type.Int],
            Variant.Type.String => [VariantType],
            Variant.Type.Object => [VariantType, Variant.Type.Nil],
            _ => [VariantType]
        };

        public override void RenderType(StringBuilder builder, GenerationLogger logger)
        {
            using var _ = logger.BeginScope(ToString());
            builder.Append(CSharpTypeName);
        }

        public override string ToString() => CSharpTypeName;
    }

    private record GodotVariantType() : GodotNamedType("variant", nameof(Variant))
    {
        public override HashSet<Variant.Type> Accepts { get; } = [];
        public override string ToString() => "Variant";
    }

    private record UserUndefinedEnumType(string EnumDefine, GodotAnnotatedVariantType BackedType) : GodotType
    {
        public override HashSet<Variant.Type> Accepts { get; } = [Variant.Type.Int];

        public override void RenderType(StringBuilder builder, GenerationLogger logger)
        {
            using var _ = logger.BeginScope(ToString());
            BackedType.RenderType(builder, logger);
            var enumDefineString = string.IsNullOrWhiteSpace(EnumDefine) ? "Empty Enum Constant String" : EnumDefine;
            builder.Append($"/* \"{enumDefineString}\" */");
            logger.Add($"Using an unregistered enum type \"{enumDefineString}\".");
        }

        public override string ToString() => $"UnDefEnum<{BackedType}/*{EnumDefine}*/>";
    }

    private record GodotEnumType(GodotName GodotTypeName, CSharpName CSharpTypeName, GodotType OwnerType, bool IsBitField) : GodotNamedType(GodotTypeName, CSharpTypeName)
    {
        public string DefaultEnumValue { get; set; }

        public override string ToString() => IsBitField ? $"Flags<{GodotTypeName}>" : $"Enum<{GodotTypeName}>";

        public List<(string EnumName, long EnumValue)> EnumConstants { get; } = [];

        public override HashSet<Variant.Type> Accepts { get; } = [Variant.Type.Int];

        public bool UseAlias { get; set; }

        public override void RenderType(StringBuilder builder, GenerationLogger logger)
        {
            using var _ = logger.BeginScope(ToString());
            if (OwnerType is null)
            {
                builder.Append(CSharpTypeName);
                return;
            }

            OwnerType.RenderType(builder, logger);
            builder.Append('.');
            RenderEnumName(builder);
        }

        public void RenderEnum(StringBuilder builder, GenerationLogger logger)
        {
            using var _ = logger.BeginScope(GodotTypeName);
            if (IsBitField) builder.Append(__).AppendLine("[Flags]");
            builder.Append($"{__}public enum ");
            RenderEnumName(builder);
            builder.AppendLine();
            builder.AppendLine(
                $$"""
                  {{__}}{
                  {{string.Join('\n', EnumConstants.Select(x => $"{__ + __}{x.EnumName} = {x.EnumValue},"))}}
                  {{__}}}
                  """
            );
        }

        private void RenderEnumName(StringBuilder builder)
        {
            builder.Append(CSharpTypeName);
            if (UseAlias) builder.Append(IsBitField ? "Flags" : "Enum");
        }

        public static string FormatEnumName(string enumName, string enumConstName)
        {
            var enumNameWords = enumName.ToSnakeCase().ToUpperInvariant().Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var enumConstNameWords = enumConstName.ToSnakeCase().ToUpperInvariant().Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            var intersection = enumNameWords.Intersect(enumConstNameWords).ToArray();

            if (!enumConstNameWords.Take(intersection.Length).SequenceEqual(intersection))
            {
                return enumConstName.ToPascalCase();
            }

            var trimmedEnumNameWords = enumConstNameWords.Skip(intersection.Length).ToList();

            if (trimmedEnumNameWords.Count == 0)
            {
                return enumConstName.ToPascalCase();
            }

            if (!IsValidIdentifierNameRegex().IsMatch(trimmedEnumNameWords[0]))
            {
                for (var i = 0; i < trimmedEnumNameWords.Count; i++)
                    enumConstNameWords.RemoveAt(enumConstNameWords.Count - 1);

                while (true)
                {
                    if (enumConstNameWords.Count == 0) break;
                    trimmedEnumNameWords.Insert(0, enumConstNameWords[^1]);
                    enumConstNameWords.RemoveAt(enumConstNameWords.Count - 1);
                    if (IsValidIdentifierNameRegex().IsMatch(trimmedEnumNameWords[0])) break;
                }
            }

            if (!IsValidIdentifierNameRegex().IsMatch(trimmedEnumNameWords[0]))
                return enumConstName.ToPascalCase();

            return string.Join('_', trimmedEnumNameWords).ToPascalCase();
        }
    }

    private abstract record GodotNamedType(GodotName GodotTypeName, CSharpName CSharpTypeName) : GodotType
    {
        public override string ToString() => GodotTypeName;
        public override void RenderType(StringBuilder builder, GenerationLogger logger)
        {
            using var _ = logger.BeginScope(ToString());
            builder.Append(CSharpTypeName);
        }
    }

    private record GodotAnnotatedDictionaryType(GodotType KeyType, GodotType ValueType) : GodotType
    {
        public override string ToString() => $"Dictionary<{KeyType}, {ValueType}>";

        public override HashSet<Variant.Type> Accepts { get; } = [Variant.Type.Dictionary];

        public override void RenderType(StringBuilder builder, GenerationLogger logger)
        {
            using var _ = logger.BeginScope(ToString());
            builder.Append("Dictionary<");
            KeyType.RenderType(builder, logger);
            builder.Append(", ");
            ValueType.RenderType(builder, logger);
            builder.Append('>');
        }
    }

    private record GodotAnnotatedArrayType(GodotType ElementType) : GodotType
    {
        public override HashSet<Variant.Type> Accepts { get; } = [Variant.Type.Array];

        public override void RenderType(StringBuilder builder, GenerationLogger logger)
        {
            using var _ = logger.BeginScope(ToString());
            builder.Append("Array<");
            ElementType.RenderType(builder, logger);
            builder.Append('>');
        }

        public override string ToString() => $"Array<{ElementType}>";
    }

    private abstract record GodotType
    {
        public abstract HashSet<Variant.Type> Accepts { get; }
        public abstract void RenderType(StringBuilder builder, GenerationLogger logger);

        public void RenderVariantToCSharp(StringBuilder builder, GenerationLogger logger)
        {
            using var _ = logger.BeginScope(ToString());
            if (this is GodotAnnotatedVariantType { VariantType: Variant.Type.Nil }) return;
            builder.Append(".As<");
            RenderType(builder, logger);
            builder.Append(">()");
        }

        public void RenderCSharpToVariant(string csharpVariableName, StringBuilder builder)
        {
            if (this is GodotEnumType)
            {
                builder
                    .Append("Variant.From(")
                    .Append(csharpVariableName)
                    .Append(')');
            }
            else
            {
                builder.Append(csharpVariableName);
            }
        }
    }

    private record GodotMultiType : GodotType
    {
        public GodotMultiType(GodotType[] types, GodotVariantType variantType)
        {
            if (types.Length == 0) throw new ArgumentException("Types cannot be empty", nameof(types));
            Types = types;

            // If both types are both GodotClassType, find their common ancestor
            // and render that instead
            if (types.Length == 1)
            {
                CommonBaseType = types[0];
                return;
            }

            var godotClassCandidate = types.Select(x => x as GodotClassType).ToArray();
            if (godotClassCandidate.All(x => x != null))
            {
                CommonBaseType = FindCommonBaseType(godotClassCandidate);
                Accepts = [Variant.Type.Object];
                return;
            }

            var godotVariantCandidate = types.Select(x => x as GodotAnnotatedVariantType).ToArray();
            if (godotVariantCandidate.All(x => x != null))
            {
                var type = godotVariantCandidate.Select(x => x.VariantType).Distinct().ToArray();
                if (type.Length == 1)
                {
                    CommonBaseType = godotVariantCandidate[0];
                    Accepts = [godotVariantCandidate[0].VariantType];
                    return;
                }
            }

            CommonBaseType = variantType;
            Accepts = [];
        }

        public override HashSet<Variant.Type> Accepts { get; }

        public override void RenderType(StringBuilder builder, GenerationLogger logger) =>
            CommonBaseType.RenderType(builder, logger);

        public override string ToString() => $"<{string.Join<GodotType>(", ", Types)}>";
        public GodotType[] Types { get; }
        public GodotType CommonBaseType { get; }

        private static GodotType FindCommonBaseType(GodotClassType[] types)
        {
            if (types.Length == 0) return null;

            var typeChains = new List<HashSet<GodotType>>();

            foreach (var type in types)
            {
                var typeChain = new HashSet<GodotType>();
                GodotType currentType = type;
                while (currentType != null)
                {
                    typeChain.Add(currentType);
                    currentType = (currentType as GodotClassType)?.ParentType;
                }

                typeChains.Add(typeChain);
            }

            var commonBaseTypes = typeChains[0];

            foreach (var typeChain in typeChains.Skip(1))
            {
                commonBaseTypes.IntersectWith(typeChain);
            }

            return commonBaseTypes.OrderByDescending(x => (x as GodotClassType)?.GetInheritanceDepth() ?? 0).FirstOrDefault();
        }
    }

    private record GodotPropertyInfo(
        GodotName GodotName,
        CSharpName CSharpName,
        GodotType Type,
        PropertyHint Hint,
        string HintString,
        PropertyUsageFlags Usage
    )
    {
        public override string ToString() => $"{Type} {CSharpName}";

        public void RenderCSharpToVariant(StringBuilder builder)
        {
            Type.RenderCSharpToVariant(EscapeCSharpKeyWords(CSharpName), builder);
        }

        public void RenderVariantToCSharp(StringBuilder marshallingBuilder, GenerationLogger logger)
        {
            marshallingBuilder.Append(EscapeCSharpKeyWords(CSharpName));
            Type.RenderVariantToCSharp(marshallingBuilder, logger);
        }

        public void RenderName(StringBuilder builder) =>
            builder.Append(EscapeCSharpKeyWords(CSharpName));

        public void RenderTypeArgument(StringBuilder methodTypeArgumentBuilder, GenerationLogger logger)
        {
            if (Usage.HasFlag(PropertyUsageFlags.NilIsVariant)) methodTypeArgumentBuilder.Append("Variant");
            else Type.RenderType(methodTypeArgumentBuilder, logger);
        }

        public void Render(StringBuilder methodBuilder, GenerationLogger logger)
        {
            RenderTypeArgument(methodBuilder, logger);
            methodBuilder.Append(' ');
            methodBuilder.Append(EscapeCSharpKeyWords(CSharpName));
        }
    }

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9_]*$")]
    private static partial Regex IsValidIdentifierNameRegex();

    [GeneratedRegex("[^A-Za-z0-9_]")] private static partial Regex ValidIdentifierCharacterRegex();

    private static readonly HashSet<string> CSharpKeyWords =
    [
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue",
        "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
        "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is",
        "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return",
        "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof",
        "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while",
    ];

    private static string EscapeNamespaceKeyWords(string sourceNamespace)
    {
        var namespaceParts = sourceNamespace.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        for (var i = 0; i < namespaceParts.Length; i++)
            namespaceParts[i] = EscapeCSharpKeyWords(namespaceParts[i]);
        return string.Join('.', namespaceParts);
    }

    [GeneratedRegex("[:/\\\\?*\"|%<>]")] private static partial Regex InvalidPathCharacterRegex();
    
    public static string EscapePath(string path)
    {
        var parts = path.Split(["/", "\\"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        for (var i = 0; i < parts.Length; i++)
            parts[i] = InvalidPathCharacterRegex().Replace(parts[i], "_");
        return string.Join('/', parts);
    }
    
    private static string EscapeCSharpKeyWords(string sourceName)
    {
        sourceName = ValidIdentifierCharacterRegex().Replace(sourceName, "_");
        if (CSharpKeyWords.Contains(sourceName))
            return $"@{sourceName}";
        if (!IsValidIdentifierNameRegex().IsMatch(sourceName))
            return $"arg{sourceName.ToPascalCase()}";
        return sourceName;
    }

    private readonly record struct GodotMethodArgument(GodotPropertyInfo Info, Variant? Default)
    {
        public override string ToString() => Default == null ? Info.ToString() : $"{Info} = {Default}";

        private static readonly HashSet<Variant.Type> RequireExtraDefaultValuePopulationVariantTypes =
        [
            Variant.Type.Vector2, Variant.Type.Vector2I, Variant.Type.Rect2, Variant.Type.Rect2I,
            Variant.Type.Vector3, Variant.Type.Vector3I, Variant.Type.Transform2D,
            Variant.Type.Vector4, Variant.Type.Vector4I,
            Variant.Type.Plane, Variant.Type.Quaternion, Variant.Type.Aabb,
            Variant.Type.Basis, Variant.Type.Transform3D, Variant.Type.Projection,
            Variant.Type.Color, Variant.Type.StringName, Variant.Type.NodePath,
            Variant.Type.Callable, Variant.Type.Dictionary, Variant.Type.Array,
            Variant.Type.PackedByteArray, Variant.Type.PackedInt32Array, Variant.Type.PackedInt64Array,
            Variant.Type.PackedFloat32Array, Variant.Type.PackedFloat64Array, Variant.Type.PackedStringArray,
            Variant.Type.PackedVector2Array, Variant.Type.PackedVector3Array, Variant.Type.PackedColorArray, Variant.Type.PackedVector4Array
        ];

        public void RenderCSharpToVariant(StringBuilder methodArgumentBuilder) =>
            Info.RenderCSharpToVariant(methodArgumentBuilder);

        public void RenderVariantToCSharp(StringBuilder methodArgumentBuilder, GenerationLogger logger) =>
            Info.RenderVariantToCSharp(methodArgumentBuilder, logger);

        public void RenderVariant(StringBuilder builder)
        {
            builder.Append("Variant ");
            Info.RenderName(builder);
        }

        public void RenderFunctionArgument(StringBuilder methodBuilder, GenerationLogger logger)
        {
            using var _ = logger.BeginScope();
            Info.Render(methodBuilder, logger);
            if (!Default.HasValue) return;
            methodBuilder.Append(" = ");
            if (RequireExtraDefaultValuePopulationVariantTypes.Contains(Default.Value.VariantType)
                || Info.Usage.HasFlag(PropertyUsageFlags.NilIsVariant)) methodBuilder.Append("default");
            else if (Info.Type is GodotEnumType enumType)
            {
                if (Default.Value.VariantType is not Variant.Type.Int)
                {
                    if (enumType.DefaultEnumValue is not null)
                    {
                        enumType.RenderType(methodBuilder, logger);
                        methodBuilder.Append('.').Append(enumType.DefaultEnumValue);
                        logger.Add($"The default value of the enum method argument {Info.GodotName} is not integer ({Default.Value.VariantType}), {enumType.GodotTypeName}.{enumType.DefaultEnumValue} is used instead.");
                    }
                    else
                    {
                        methodBuilder.Append("default");
                        logger.Add($"The default value of the enum method argument {Info.GodotName} is not integer ({Default.Value.VariantType}), and the enum type {enumType.GodotTypeName} does not have any defined values, default is used instead.");
                    }
                }
                else
                {
                    var value = Default.Value.AsInt64();
                    var matched = enumType.EnumConstants.FirstOrDefault(x => x.EnumValue == value);
                    if (matched.EnumName is not null)
                    {
                        enumType.RenderType(methodBuilder, logger);
                        methodBuilder.Append('.').Append(matched.EnumName);
                    }
                    else
                    {
                        methodBuilder.Append('(');
                        enumType.RenderType(methodBuilder, logger);
                        methodBuilder.Append('.').Append(value);
                    }
                }
            }
            else
            {
                if (!Info.Type.Accepts.Contains(Default.Value.VariantType))
                {
                    if (Info.Type.Accepts.Contains(Variant.Type.Object))
                    {
                        methodBuilder.Append("null");
                        logger.Add($"The default value type {Default.Value.VariantType} cannot be used for the method argument {Info.GodotName} (only accepts [{string.Join(", ", Info.Type.Accepts)}]), null is used instead.");
                    }
                    else
                    {
                        methodBuilder.Append("default");
                        logger.Add($"The default value type {Default.Value.VariantType} cannot be used for the method argument {Info.GodotName} (only accepts [{string.Join(", ", Info.Type.Accepts)}]), default is used instead.");
                    }
                }
                else
                    methodBuilder.Append(
                        Default.Value.VariantType switch
                        {
                            Variant.Type.Nil => Info.Type.Accepts.Contains(Variant.Type.Object) ? "null" : "default",
                            Variant.Type.Bool => Default.Value.AsBool().ToString().ToLower(),
                            Variant.Type.Int => Default.Value.AsInt64().ToString(),
                            Variant.Type.Float => Default.Value.AsDouble().ToString(CultureInfo.InvariantCulture),
                            Variant.Type.String => $"\"{Default.Value.AsString()}\"",
                            Variant.Type.Object => "null",
                            _ => throw new UnreachableException()
                        }
                    );
            }
        }
    }

    private record GodotClassPropertyInfo(
        GodotName GodotPropertyName,
        CSharpName CSharpPropertyName,
        GodotType GodotPropertyType,
        GodotFunctionInfo Setter,
        GodotFunctionInfo Getter
    )
    {
        public override string ToString() =>
            (Getter, Setter) switch
            {
                (null, null) => $"{GodotPropertyType} {CSharpPropertyName} {{ ! }}",
                (_, null) => $"{GodotPropertyType} {CSharpPropertyName} {{ get; }}",
                (null, _) => $"{GodotPropertyType} {CSharpPropertyName} {{ set; }}",
                (_, _) => $"{GodotPropertyType} {CSharpPropertyName} {{ get; set; }}",
            };

        public void RenderProperty(StringBuilder propertyBuilder, GenerationLogger logger)
        {
            using var _ = logger.BeginScope(GodotPropertyName);
            propertyBuilder.Append($"{__}public new ");
            if (GodotPropertyType is GodotAnnotatedVariantType { VariantType: Variant.Type.Nil }) propertyBuilder.Append("Variant");
            else GodotPropertyType.RenderType(propertyBuilder, logger);
            propertyBuilder.Append(' ').AppendLine(CSharpPropertyName);
            propertyBuilder.AppendLine($$"""{{__}}{""");
            if (Getter is not null)
            {
                propertyBuilder.Append($"{__ + __}get => Get(GDExtensionPropertyName.{CSharpPropertyName})");
                GodotPropertyType.RenderVariantToCSharp(propertyBuilder, logger);
                propertyBuilder.AppendLine(";");
            }

            if (Setter is not null)
            {
                propertyBuilder.Append($"{__ + __}set => Set(GDExtensionPropertyName.{CSharpPropertyName}, ");
                GodotPropertyType.RenderCSharpToVariant("value", propertyBuilder);
                propertyBuilder.AppendLine(");");
            }

            propertyBuilder.AppendLine($$"""{{__}}}""");
        }
    }

    private record GodotFunctionInfo(
        GodotName GodotFunctionName,
        CSharpName CSharpFunctionName,
        GodotPropertyInfo ReturnValue,
        long MethodId,
        MethodFlags Flags
    )
    {
        public List<GodotMethodArgument> FunctionArguments { get; } = [];
        public override string ToString() => $"[{Flags}] {ReturnValue.Type} {CSharpFunctionName}({string.Join(", ", FunctionArguments)})";

        public void RenderMethod(StringBuilder methodBuilder, GenerationLogger logger)
        {
            using var _ = logger.BeginScope(GodotFunctionName);
            methodBuilder.Append($"{__}public new ");
            var isStatic = Flags.HasFlag(MethodFlags.Static);
            var isVirtual = Flags.HasFlag(MethodFlags.Virtual);
            if (isStatic) methodBuilder.Append("static ");
            if (isVirtual) methodBuilder.Append("virtual ");
            ReturnValue.Type.RenderType(methodBuilder, logger);
            methodBuilder.Append(' ').Append(CSharpFunctionName);
            RenderFunctionArguments(methodBuilder, logger);
            methodBuilder.AppendLine(" => ").Append(__ + __);
            if (isStatic) methodBuilder.Append("ClassDB.ClassCallStatic(NativeName, ");
            else methodBuilder.Append("Call(");
            methodBuilder.Append($"GDExtensionMethodName.{CSharpFunctionName}, [");

            var isFirst = true;
            foreach (var methodArgument in FunctionArguments)
            {
                if (isFirst) isFirst = false;
                else methodBuilder.Append(", ");
                methodArgument.RenderCSharpToVariant(methodBuilder);
            }

            methodBuilder.Append("])");
            ReturnValue.Type.RenderVariantToCSharp(methodBuilder, logger);
            methodBuilder.AppendLine(";");
        }

        public void RenderSignal(StringBuilder signalBuilder, GenerationLogger logger)
        {
            using var _ = logger.BeginScope(GodotFunctionName);
            var signalName = $"{CSharpFunctionName}Signal";
            var signalDelegateName = $"{signalName}Handler";
            var signalNameCamelCase = signalName.ToCamelCase();
            var backingDelegateName = $"_{signalNameCamelCase}";
            var backingCallableName = $"_{signalNameCamelCase}Callable";

            signalBuilder.Append($"{__}public new delegate ");
            ReturnValue.Type.RenderType(signalBuilder, logger);
            signalBuilder.Append($" {signalDelegateName}");
            RenderFunctionArguments(signalBuilder, logger);
            signalBuilder.AppendLine(";");
            signalBuilder.Append(
                $$"""
                  {{__}}private {{signalDelegateName}} {{backingDelegateName}};
                  {{__}}private Callable {{backingCallableName}};
                  {{__}}public event {{signalDelegateName}} {{signalName}}
                  {{__}}{
                  {{__ + __}}add
                  {{__ + __}}{
                  {{__ + __ + __}}if ({{backingDelegateName}} is null)
                  {{__ + __ + __}}{
                  {{__ + __ + __ + __}}{{backingCallableName}} = Callable.From(
                  """
            );

            RenderVariantTypeArgument(signalBuilder);

            signalBuilder
                .AppendLine(" => ")
                .Append($"{__ + __ + __ + __ + __}{backingDelegateName}?.Invoke(");

            var isFirst = true;
            foreach (var methodArgument in FunctionArguments)
            {
                if (isFirst) isFirst = false;
                else signalBuilder.Append(", ");
                methodArgument.RenderVariantToCSharp(signalBuilder, logger);
            }

            signalBuilder.AppendLine("));");

            signalBuilder.AppendLine(
                $$"""
                  {{__ + __ + __ + __}}Connect(GDExtensionSignalName.{{CSharpFunctionName}}, {{backingCallableName}});
                  {{__ + __ + __}}}
                  {{__ + __ + __}}{{backingDelegateName}} += value;
                  {{__ + __}}}
                  {{__ + __}}remove
                  {{__ + __}}{
                  {{__ + __ + __}}{{backingDelegateName}} -= value;
                  {{__ + __ + __}}if ({{backingDelegateName}} is not null) return;
                  {{__ + __ + __}}Disconnect(GDExtensionSignalName.{{CSharpFunctionName}}, {{backingCallableName}});
                  {{__ + __ + __}}{{backingCallableName}} = default;
                  {{__ + __}}}
                  {{__}}}
                  """
            );
        }

        private void RenderFunctionArguments(StringBuilder builder, GenerationLogger logger)
        {
            using var _ = logger.BeginScope(GodotFunctionName);
            builder.Append('(');
            var isFirst = true;
            foreach (var argument in FunctionArguments)
            {
                if (isFirst) isFirst = false;
                else builder.Append(", ");
                argument.RenderFunctionArgument(builder, logger);
            }

            builder.Append(')');
        }

        private void RenderVariantTypeArgument(StringBuilder builder)
        {
            builder.Append('(');
            var isFirst = true;
            foreach (var argument in FunctionArguments)
            {
                if (isFirst) isFirst = false;
                else builder.Append(", ");
                argument.RenderVariant(builder);
            }

            builder.Append(')');
        }
    }

    private class GodotTypeMap
    {
        public Dictionary<GodotName, GodotNamedType> Types { get; } = [];
        public Dictionary<Variant.Type, string> VariantTypeToGodotName { get; } = [];
        public GodotVariantType Variant { get; } = new();
        public Dictionary<GodotType, GodotAnnotatedArrayType> PreregisteredArrayTypes { get; } = [];
        public Dictionary<(GodotType, GodotType), GodotAnnotatedDictionaryType> PreregisteredDictionaryTypes { get; } = [];
        public Dictionary<GodotNamedType, Dictionary<GodotName, GodotEnumType>> PreregisteredEnumTypes { get; } = [];
        public Dictionary<NormalizedEnumConstantsString, Dictionary<GodotType, Dictionary<EnumName, GodotEnumType>>> PreregisteredEnumTypesByName { get; } = [];
        public Dictionary<GodotName, GodotEnumType> GlobalScopeEnumTypes { get; } = [];

        public bool TryGetVariantType(Variant.Type variantTypeEnum, out GodotAnnotatedVariantType variantType)
        {
            if (!VariantTypeToGodotName.TryGetValue(variantTypeEnum, out var variantTypeName))
            {
                variantType = null;
                return false;
            }

            if (!Types.TryGetValue(variantTypeName, out var type))
            {
                variantType = null;
                return false;
            }

            variantType = (GodotAnnotatedVariantType)type;
            return true;
        }

        public IEnumerable<GodotClassType> SelectTypes(params ClassDB.ApiType[] apiTypes) =>
            Types.Values.OfType<GodotClassType>().Where(x => apiTypes.Contains(x.ApiType));
    }

    #endregion
}