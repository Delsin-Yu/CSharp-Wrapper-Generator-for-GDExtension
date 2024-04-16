using System.Collections.Concurrent;
using System.Linq;
using Godot;

namespace GDExtensionAPIGenerator;

internal static partial class CodeGenerator
{
    private static void PopulateBuiltinEnumTypes(ConcurrentDictionary<string, string> enumNameToConstantMap)
    {
        // TODO: Populate builtin Enums

        enumNameToConstantMap.TryAdd("Nil", "Variant.Type");
        enumNameToConstantMap.TryAdd("bool", "Variant.Type");
        enumNameToConstantMap.TryAdd("int", "Variant.Type");
        enumNameToConstantMap.TryAdd("float", "Variant.Type");
        enumNameToConstantMap.TryAdd("String", "Variant.Type");
        enumNameToConstantMap.TryAdd("Vector2", "Variant.Type");
        enumNameToConstantMap.TryAdd("Vector2i", "Variant.Type");
        enumNameToConstantMap.TryAdd("Rect2", "Variant.Type");
        enumNameToConstantMap.TryAdd("Rect2i", "Variant.Type");
        enumNameToConstantMap.TryAdd("Vector3", "Variant.Type");
        enumNameToConstantMap.TryAdd("Vector3i", "Variant.Type");
        enumNameToConstantMap.TryAdd("Transform2D", "Variant.Type");
        enumNameToConstantMap.TryAdd("Vector4", "Variant.Type");
        enumNameToConstantMap.TryAdd("Vector4i", "Variant.Type");
        enumNameToConstantMap.TryAdd("Plane", "Variant.Type");
        enumNameToConstantMap.TryAdd("Quaternion", "Variant.Type");
        enumNameToConstantMap.TryAdd("AABB", "Variant.Type");
        enumNameToConstantMap.TryAdd("Basis", "Variant.Type");
        enumNameToConstantMap.TryAdd("Transform3D", "Variant.Type");
        enumNameToConstantMap.TryAdd("Projection", "Variant.Type");
        enumNameToConstantMap.TryAdd("Color", "Variant.Type");
        enumNameToConstantMap.TryAdd("StringName", "Variant.Type");
        enumNameToConstantMap.TryAdd("NodePath", "Variant.Type");
        enumNameToConstantMap.TryAdd("RID", "Variant.Type");
        enumNameToConstantMap.TryAdd("Object", "Variant.Type");
        enumNameToConstantMap.TryAdd("Callable", "Variant.Type");
        enumNameToConstantMap.TryAdd("Signal", "Variant.Type");
        enumNameToConstantMap.TryAdd("Dictionary", "Variant.Type");
        enumNameToConstantMap.TryAdd("Array", "Variant.Type");
        enumNameToConstantMap.TryAdd("PackedByteArray", "Variant.Type");
        enumNameToConstantMap.TryAdd("PackedInt32Array", "Variant.Type");
        enumNameToConstantMap.TryAdd("PackedInt64Array", "Variant.Type");
        enumNameToConstantMap.TryAdd("PackedFloat32Array", "Variant.Type");
        enumNameToConstantMap.TryAdd("PackedFloat64Array", "Variant.Type");
        enumNameToConstantMap.TryAdd("PackedStringArray", "Variant.Type");
        enumNameToConstantMap.TryAdd("PackedVector2Array", "Variant.Type");
        enumNameToConstantMap.TryAdd("PackedVector3Array", "Variant.Type");
        enumNameToConstantMap.TryAdd("PackedColorArray", "Variant.Type");
    }
}