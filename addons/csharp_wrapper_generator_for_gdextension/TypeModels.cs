using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Godot;
using GodotName = string;
using CSharpName = string;

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
        bool CanInstantiate) : GodotNamedType(GodotTypeName, CSharpTypeName)
    {
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

        private const string Namespace = "GDExtension.Wrappers";
        private const string BindMethodName = "Bind";
        private const string TypeGDExtensionCacheName = "NativeName";
        private const string WrapperConstructorName = "Instantiate";

        public void RenderClass(StringBuilder classBuilder, ConcurrentBag<string> warnings)
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

                 namespace {Namespace};

                 public 
                 """
            );

            if (!CanInstantiate) classBuilder.Append("abstract ");

            var parentTypeName = ParentType switch
            {
                GodotClassType parentClassType => parentClassType.CSharpTypeName,
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
                     {__}public new static {CSharpTypeName} {WrapperConstructorName}() => {BindMethodName}(ClassDB.Instantiate("{GodotTypeName}").As<GodotObject>());

                     """
                );
            }

            foreach (var enumInfo in Enums)
            {
                enumInfo.RenderEnum(classBuilder, warnings);
                classBuilder.AppendLine();
            }
            
            foreach (var methodInfo in Methods)
            {
                methodInfo.RenderMethod(classBuilder, warnings);
                classBuilder.AppendLine();
            }

            classBuilder
                .AppendLine("}");
        }
    }

    private record GodotAnnotatedVariantType(
        GodotName GodotTypeName,
        CSharpName CSharpTypeName,
        Variant.Type VariantType) : GodotNamedType(GodotTypeName, CSharpTypeName)
    {
        public override HashSet<Variant.Type> Accepts { get; } = VariantType switch {
            Variant.Type.Nil => [],
            Variant.Type.Bool => [VariantType],
            Variant.Type.Int => [VariantType],
            Variant.Type.Float => [VariantType, Variant.Type.Int],
            Variant.Type.String => [VariantType],
            Variant.Type.Object => [VariantType, Variant.Type.Nil],
            _ => [VariantType]
        };
        public override void Render(StringBuilder builder, ConcurrentBag<string> warnings) => builder.Append(CSharpTypeName);

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

        public override void Render(StringBuilder builder, ConcurrentBag<string> warnings)
        {
            BackedType.Render(builder, warnings);
            builder.Append($"/* {EnumDefine} */");
            warnings.Add($"Using an unregistered enum type {EnumDefine}.");
        }

        public override string ToString() => $"UnDefEnum<{BackedType}/*{EnumDefine}*/>";
    }

    private record GodotEnumType(GodotName GodotTypeName, CSharpName CSharpTypeName, GodotType OwnerType, bool IsBitField) : GodotNamedType(GodotTypeName, CSharpTypeName)
    {
        public string DefaultEnumValue { get; set; }
        
         public override string ToString() => IsBitField ? $"Flags<{GodotTypeName}>" : $"Enum<{GodotTypeName}>";

        public List<(string EnumName, long EnumValue)> EnumConstants { get; } = [];

        public override HashSet<Variant.Type> Accepts { get; } = [Variant.Type.Int];

        public override void Render(StringBuilder builder, ConcurrentBag<string> warnings)
        {
            if (OwnerType is null)
            {
                builder.Append(CSharpTypeName);
                return;
            }
            OwnerType.Render(builder, warnings);
            builder.Append('.').Append(CSharpTypeName);
        }

        public void RenderEnum(StringBuilder builder, ConcurrentBag<string> warnings)
        {
            if (IsBitField) builder.Append(__).AppendLine("[Flags]");
            builder.AppendLine(
                $$"""
                  {{__}}public enum {{CSharpTypeName}}
                  {{__}}{
                  {{string.Join('\n', EnumConstants.Select(x => $"{__ + __}{x.EnumName} = {x.EnumValue},"))}}
                  {{__}}}
                  """
            );
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
        public override void Render(StringBuilder builder, ConcurrentBag<string> warnings) => builder.Append(CSharpTypeName);
    }

    private record GodotAnnotatedDictionaryType(GodotType KeyType, GodotType ValueType) : GodotType
    {
        public override string ToString() => $"Dictionary<{KeyType}, {ValueType}>";

        public override HashSet<Variant.Type> Accepts { get; } = [Variant.Type.Dictionary];

        public override void Render(StringBuilder builder, ConcurrentBag<string> warnings)
        {
            builder.Append("Dictionary<");
            KeyType.Render(builder, warnings);
            builder.Append(", ");
            ValueType.Render(builder, warnings);
            builder.Append('>');
        }
    }

    private record GodotAnnotatedArrayType(GodotType ElementType) : GodotType
    {
        public override HashSet<Variant.Type> Accepts { get; } = [Variant.Type.Array];
        
        public override void Render(StringBuilder builder, ConcurrentBag<string> warnings)
        {
            builder.Append("Array<");
            ElementType.Render(builder, warnings);
            builder.Append('>');
        }

        public override string ToString() => $"Array<{ElementType}>";
    }

    private abstract record GodotType
    {
        public abstract HashSet<Variant.Type> Accepts { get; }
        public abstract void Render(StringBuilder builder, ConcurrentBag<string> warnings);
    }

    private record GodotMultiType : GodotType
    {
        public GodotMultiType(GodotType[] Types)
        {
            if (Types.Length == 0) throw new ArgumentException("Types cannot be empty", nameof(Types));
            this.Types = Types;
        }

        // TODO: Impl
        public override HashSet<Variant.Type> Accepts { get; } = [];

        public override void Render(StringBuilder builder, ConcurrentBag<string> warnings)
        {
            // If both types are both GodotClassType, find their common ancestor
            // and render that instead
            if (Types.Length == 1)
            {
                Types[0].Render(builder, warnings);
                return;
            }

            var godotClassCandidate = Types.Select(x => x as GodotClassType).ToArray();
            if (godotClassCandidate.All(x => x != null))
            {
                // TODO: Find the common ancestor
                builder.Append("GodotObject");
            }

            var godotVariantCandidate = Types.Select(x => (GodotType)(x as GodotAnnotatedVariantType) ?? x as GodotVariantType).ToArray();
            if (godotVariantCandidate.All(x => x != null))
            {
                // TODO: Find the common variant type
                builder.Append("Variant");
            }

            builder.Append("Variant");
        }

        public override string ToString() => $"<{string.Join<GodotType>(", ", Types)}>";
        public GodotType[] Types { get; init; }
    }

    private record GodotPropertyInfo(
        string GodotName,
        string CSharpName,
        GodotType Type,
        PropertyHint Hint,
        string HintString,
        PropertyUsageFlags Usage)
    {
        public override string ToString() => $"{Type} {CSharpName}";

        public void Render(StringBuilder methodBuilder, ConcurrentBag<string> warnings)
        {
            if (Usage.HasFlag(PropertyUsageFlags.NilIsVariant)) methodBuilder.Append("Variant");
            else Type.Render(methodBuilder, warnings);
            methodBuilder.Append(' ').Append(EscapeCSharpKeyWords(CSharpName));
        }
    }
    
    [GeneratedRegex("^[A-Za-z][A-Za-z0-9_]*$")]
    private static partial Regex IsValidIdentifierNameRegex();
    
    [GeneratedRegex("[^A-Za-z0-9_]")]
    private static partial Regex ValidIdentifierCharacterRegex();
    
    private static readonly HashSet<string> CSharpKeyWords =
    [
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue",
        "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
        "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is",
        "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return",
        "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof",
        "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while",
    ];

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

        public void Render(StringBuilder methodBuilder, ConcurrentBag<string> warnings)
        {
            Info.Render(methodBuilder, warnings);
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
                        enumType.Render(methodBuilder, warnings);
                        methodBuilder.Append('.').Append(enumType.DefaultEnumValue);
                        warnings.Add($"The default value of the enum method argument {Info.GodotName} is not integer ({Default.Value.VariantType}), {enumType.GodotTypeName}.{enumType.DefaultEnumValue} is used instead.");
                    }
                    else
                    {
                        methodBuilder.Append("default");
                        warnings.Add($"The default value of the enum method argument {Info.GodotName} is not integer ({Default.Value.VariantType}), and the enum type {enumType.GodotTypeName} does not have any defined values, default is used instead.");
                    }
                }
                else
                {
                    var value = Default.Value.AsInt64();
                    var matched = enumType.EnumConstants.FirstOrDefault(x => x.EnumValue == value);
                    if (matched.EnumName is not null)
                    {
                        enumType.Render(methodBuilder, warnings);
                        methodBuilder.Append('.').Append(matched.EnumName);
                    }
                    else
                    {
                        methodBuilder.Append('(');
                        enumType.Render(methodBuilder, warnings);
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
                        warnings.Add($"The default value type {Default.Value.VariantType} cannot be used for the method argument {Info.GodotName} (only accepts [{string.Join(", ", Info.Type.Accepts)}]), null is used instead.");
                    }
                    else
                    {
                        methodBuilder.Append("default");
                        warnings.Add($"The default value type {Default.Value.VariantType} cannot be used for the method argument {Info.GodotName} (only accepts [{string.Join(", ", Info.Type.Accepts)}]), default is used instead.");
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

    private record GodotClassPropertyInfo(GodotName GodotPropertyName, CSharpName CSharpPropertyName, GodotType GodotPropertyType, GodotFunctionInfo Setter, GodotFunctionInfo Getter)
    {
        public override string ToString() =>
            (Getter, Setter) switch
            {
                (_, null) => $"{GodotPropertyType} {CSharpPropertyName} {{ get; }}",
                (null, _) => $"{GodotPropertyType} {CSharpPropertyName} {{ set; }}",
                (_, _) => $"{GodotPropertyType} {CSharpPropertyName} {{ get; set; }}",
            };
    }

    private record GodotFunctionInfo(
        GodotName GodotFunctionName,
        CSharpName CSharpFunctionName,
        GodotPropertyInfo ReturnValue,
        long MethodId,
        MethodFlags Flags)
    {
        public List<GodotMethodArgument> FunctionArguments { get; } = [];
        public override string ToString() => $"[{Flags}] {ReturnValue.Type} {CSharpFunctionName}({string.Join(", ", FunctionArguments)})";

        public void RenderMethod(StringBuilder methodBuilder, ConcurrentBag<string> warnings)
        {
            methodBuilder.Append($"{__}public new ");
            if (Flags.HasFlag(MethodFlags.Static)) methodBuilder.Append("static ");
            if (Flags.HasFlag(MethodFlags.Virtual)) methodBuilder.Append("virtual ");
            ReturnValue.Type.Render(methodBuilder, warnings);
            methodBuilder.Append(' ').Append(CSharpFunctionName);
            RenderArguments(methodBuilder, warnings);
            // TODO: Method Call
            methodBuilder.AppendLine(" => throw new NotImplementedException();");
        }

        private void RenderArguments(StringBuilder builder, ConcurrentBag<string> warnings)
        {
            builder.Append('(');
            var isFirst = true;
            foreach (var argument in FunctionArguments)
            {
                if (isFirst) isFirst = false;
                else builder.Append(", ");
                argument.Render(builder, warnings);
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