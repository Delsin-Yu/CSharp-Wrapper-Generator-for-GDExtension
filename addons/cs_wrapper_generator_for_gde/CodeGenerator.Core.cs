using System;
using System.Collections.Generic;
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
        ICollection<string> godotBuiltinClassNames

    )
    {
        var codeBuilder = new StringBuilder();

        if (ContainsParent(gdeTypeInfo, nameof(Resource))) GenerateCodeForResource(codeBuilder, gdeTypeInfo, godotSharpTypeNameMap, godotBuiltinClassNames);
        else if (ContainsParent(gdeTypeInfo, nameof(Node))) GenerateCodeForNode(codeBuilder, gdeTypeInfo, godotSharpTypeNameMap);
        else GenerateCodeForRefCounted(codeBuilder, gdeTypeInfo, godotSharpTypeNameMap, godotBuiltinClassNames);

        return ($"{gdeTypeInfo.TypeName}.gdextension.cs", codeBuilder.ToString());
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

    private const string IDNT = "    ";
    private const string NAMESPACE_RES = "GDExtension.ResourcesWrappers";
    private const string NAMESPACE_RC = "GDExtension.RefCountedWrappers";
    private const string NAMESPACE_NODE = "GDExtension.NodeWrappers";

    private static void GenerateCodeForNode(
        StringBuilder codeBuilder,
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap
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

        ConstructProperties(gdeTypeInfo, godotSharpTypeNameMap, codeBuilder, string.Empty);

        codeBuilder.Append('}');
    }

    private static void GenerateCodeForRefCounted(
        StringBuilder codeBuilder,
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        ICollection<string> godotBuiltinClassNames
    )
    {
        var displayTypeName = godotSharpTypeNameMap.GetValueOrDefault(gdeTypeInfo.TypeName, gdeTypeInfo.TypeName);
        var displayParentTypeName = godotSharpTypeNameMap.GetValueOrDefault(gdeTypeInfo.ParentType.TypeName, gdeTypeInfo.ParentType.TypeName);
        
        const string backingName = "_backing";
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
                  
                  {{IDNT}}protected virtual {{refCountedName}} {{constructMethodName}}() =>
                  {{IDNT}}{{IDNT}}({{refCountedName}})ClassDB.Instantiate("{{gdeTypeInfo.TypeName}}");
                  
                  {{IDNT}}protected readonly {{refCountedName}} {{backingName}};
                  
                  {{IDNT}}public {{displayTypeName}}() => {{backingName}} = {{constructMethodName}}();
                  
                  {{IDNT}}public void Dispose() => {{backingName}}.Dispose();

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

                  {{IDNT}}protected override {{refCountedName}} {{constructMethodName}}() =>
                  {{IDNT}}{{IDNT}}({{refCountedName}})ClassDB.Instantiate("{{gdeTypeInfo.TypeName}}");

                  """
            );
        }

        ConstructProperties(gdeTypeInfo, godotSharpTypeNameMap, codeBuilder, $"{backingName}.");
        
        codeBuilder.Append('}');
    }

    
    private static void GenerateCodeForResource(
        StringBuilder codeBuilder,
        ClassInfo gdeTypeInfo,
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
                  {{IDNT}}protected readonly {{resourceName}} {{backingName}};

                  {{IDNT}}public {{displayTypeName}}({{resourceName}} {{backingArgument}})
                  {{IDNT}}{
                  {{IDNT}}{{IDNT}}{{backingName}} = {{backingArgument}};
                  {{IDNT}}}

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
                  
                  {{IDNT}}public {{displayTypeName}}({{resourceName}} {{backingArgument}}) : base({{backingArgument}}) { }

                  """
            );
        }

        ConstructProperties(gdeTypeInfo, godotSharpTypeNameMap, codeBuilder, $"{backingName}.");
        
        codeBuilder.Append('}');
    }

    private static void ConstructProperties(
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        StringBuilder stringBuilder,
        string backing
    )
    {
        var propertyList = ClassDB.ClassGetPropertyList(gdeTypeInfo.TypeName, true);

        foreach (var propertyDictionary in propertyList)
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
            // var hintValue = hintInfo.As<PropertyHint>();
            // var hintStringValue = hintStringInfo.AsString();
            var usageValue = usageInfo.As<PropertyUsageFlags>();

            if ((usageValue & PropertyUsageFlags.Group) != 0) continue;
            if ((usageValue & PropertyUsageFlags.Subgroup) != 0) continue;

            var typeName = VariantToTypeName(typeValue, classNameValue);

            godotSharpTypeNameMap.GetValueOrDefault(typeName, typeName);

            stringBuilder
                .AppendLine($"{IDNT}public {typeName} {EscapeAndFormatName(nameValue)}")
                .AppendLine($"{IDNT}{{")
                .AppendLine($"""{IDNT}{IDNT}get => ({typeName}){backing}Get("{nameValue}");""")
                .AppendLine($"""{IDNT}{IDNT}set => {backing}Set("{nameValue}", Variant.From(value));""")
                .AppendLine($"{IDNT}}}")
                .AppendLine();
            
            propertyDictionary.Dispose();
        }
    }

    public static string VariantToTypeName(Variant.Type type, string className) =>
        type switch
        {
            Variant.Type.Aabb => "Godot.Aabb",
            Variant.Type.Basis => "Godot.Basis",
            Variant.Type.Callable => "Godot.Callable",
            Variant.Type.Color => "Godot.Color",
            Variant.Type.NodePath => "Godot.NodePath",
            Variant.Type.Plane => "Godot.Plane",
            Variant.Type.Projection => "Godot.Projection",
            Variant.Type.Quaternion => "Godot.Quaternion",
            Variant.Type.Rect2 => "Godot.Rect2",
            Variant.Type.Rect2I => "Godot.Rect2I",
            Variant.Type.Rid => "Godot.Rid",
            Variant.Type.Signal => "Godot.Signal",
            Variant.Type.StringName => "Godot.StringName",
            Variant.Type.Transform2D => "Godot.Transform2D",
            Variant.Type.Transform3D => "Godot.Transform3D",
            Variant.Type.Vector2 => "Godot.Vector2",
            Variant.Type.Vector2I => "Godot.Vector2I",
            Variant.Type.Vector3 => "Godot.Vector3",
            Variant.Type.Vector3I => "Godot.Vector3I",
            Variant.Type.Vector4 => "Godot.Vector4",
            Variant.Type.Vector4I => "Godot.Vector4I",
            Variant.Type.Nil => "Godot.Variant",
            Variant.Type.PackedByteArray => "byte[]",
            Variant.Type.PackedInt32Array => "int[]",
            Variant.Type.PackedInt64Array => "long",
            Variant.Type.PackedFloat32Array => "float[]",
            Variant.Type.PackedFloat64Array => "double[]",
            Variant.Type.PackedStringArray => "string[]",
            Variant.Type.PackedVector2Array => "Godot.Vector2[]",
            Variant.Type.PackedVector3Array => "Godot.Vector3[]",
            Variant.Type.PackedColorArray => "Godot.Color[]",
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
    
    public static string EscapeAndFormatName(string sourceName) => 
        EscapeNameRegex()
            .Replace(sourceName, "_")
            .ToPascalCase();
}