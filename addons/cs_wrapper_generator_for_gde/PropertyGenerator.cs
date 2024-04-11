using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Godot;
using Godot.Collections;
using Godot.SourceGenerators;

namespace GDExtensionAPIGenerator;

internal static partial class GeneratorMain
{
    public static string GetTypeName(Variant.Type type, string className) =>
        type switch
        {
            Variant.Type.Aabb => KnownTypeNames.GodotAabb,
            Variant.Type.Basis => KnownTypeNames.GodotBasis,
            Variant.Type.Callable => KnownTypeNames.GodotCallable,
            Variant.Type.Color => KnownTypeNames.GodotColor,
            Variant.Type.NodePath => KnownTypeNames.GodotNodePath,
            Variant.Type.Plane => KnownTypeNames.GodotPlane,
            Variant.Type.Projection => KnownTypeNames.GodotProjection,
            Variant.Type.Quaternion => KnownTypeNames.GodotQuaternion,
            Variant.Type.Rect2 => KnownTypeNames.GodotRect2,
            Variant.Type.Rect2I => KnownTypeNames.GodotRect2I,
            Variant.Type.Rid => KnownTypeNames.GodotRid,
            Variant.Type.Signal => KnownTypeNames.GodotSignal,
            Variant.Type.StringName => KnownTypeNames.GodotStringName,
            Variant.Type.Transform2D => KnownTypeNames.GodotTransform2D,
            Variant.Type.Transform3D => KnownTypeNames.GodotTransform3D,
            Variant.Type.Vector2 => KnownTypeNames.GodotVector2,
            Variant.Type.Vector2I => KnownTypeNames.GodotVector2I,
            Variant.Type.Vector3 => KnownTypeNames.GodotVector3,
            Variant.Type.Vector3I => KnownTypeNames.GodotVector3I,
            Variant.Type.Vector4 => KnownTypeNames.GodotVector4,
            Variant.Type.Vector4I => KnownTypeNames.GodotVector4I,
            Variant.Type.Nil => KnownTypeNames.GodotVariant,
            Variant.Type.PackedByteArray => KnownTypeNames.GodotPackedByteArray,
            Variant.Type.PackedInt32Array => KnownTypeNames.GodotPackedInt32Array,
            Variant.Type.PackedInt64Array => KnownTypeNames.GodotPackedInt64Array,
            Variant.Type.PackedFloat32Array => KnownTypeNames.GodotPackedFloat32Array,
            Variant.Type.PackedFloat64Array => KnownTypeNames.GodotPackedFloat64Array,
            Variant.Type.PackedStringArray => KnownTypeNames.GodotPackedStringArray,
            Variant.Type.PackedVector2Array => KnownTypeNames.GodotPackedVector2Array,
            Variant.Type.PackedVector3Array => KnownTypeNames.GodotPackedVector3Array,
            Variant.Type.PackedColorArray => KnownTypeNames.GodotPackedColorArray,
            Variant.Type.Bool => "bool",
            Variant.Type.Int => "long",
            Variant.Type.Float => "double",
            Variant.Type.String => "string",
            Variant.Type.Object => className,
            Variant.Type.Dictionary => KnownTypeNames.GodotDictionary,
            Variant.Type.Array => KnownTypeNames.GodotArray,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

    [GeneratedRegex(@"(?<major>\d+)\.?(?<minor>\d+)?\.?(?<patch>\d+)?")]
    public static partial Regex GetRegex();

    internal static IReadOnlyDictionary<string, string> GetGodotSharpTypeNameMap()
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

    internal static class PropertyGenerator
    {
        public static bool TryGenerate(
            IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
            Dictionary propertyDictionary,
            [NotNullWhen(true)] out string? generatedProperty
        )
        {
            using var nameInfo = propertyDictionary["name"];
            using var classNameInfo = propertyDictionary["class_name"];
            using var typeInfo = propertyDictionary["type"];
            using var hintInfo = propertyDictionary["hint"];
            using var hintStringInfo = propertyDictionary["hint_string"];
            using var usageInfo = propertyDictionary["usage"];

            var nameValue = nameInfo.AsString();
            var classNameValue = classNameInfo.AsString();
            var typeValue = typeInfo.As<Variant.Type>();
            var hintValue = hintInfo.As<PropertyHint>();
            var hintStringValue = hintStringInfo.AsString();
            var usageValue = usageInfo.As<PropertyUsageFlags>();

            generatedProperty = null;

            if ((usageValue & PropertyUsageFlags.Group) != 0) return false;
            if ((usageValue & PropertyUsageFlags.Subgroup) != 0) return false;

            var typeName = GetTypeName(typeValue, classNameValue);

            if (godotSharpTypeNameMap.TryGetValue(typeName, out var mapped))
            {
                typeName = mapped;
            }

            generatedProperty =
                $$"""
                  public {{typeName}} {{nameValue.ToPascalCase()}}
                  {
                      get => ({{typeName}})Get("{{nameValue}}");
                      set => Set("{{nameValue}}", Variant.From(value));
                  }
                  """;
            return true;
        }
    }
}