using System.Collections.Generic;
using System.Text;
using Godot;

namespace GDExtensionAPIGenerator;

internal static partial class CodeGenerator
{
    private static void ConstructProperties(
        ICollection<string> occupiedNames,
        IReadOnlyList<PropertyInfo> propertyInfos,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        IReadOnlyDictionary<string, ClassInfo> gdeTypeMap,
        StringBuilder stringBuilder,
        string backing
    )
    {
        if (propertyInfos.Count == 0)
        {
            return;
        }

        stringBuilder.AppendLine(
            """
            #region Properties

            """
        );

        foreach (var propertyInfo in propertyInfos)
        {
            if (propertyInfo.IsGroupOrSubgroup) continue;

            var typeName = propertyInfo.GetTypeName();

            typeName = godotSharpTypeNameMap.GetValueOrDefault(typeName, typeName);

            var propertyName = propertyInfo.GetPropertyName();

            if (occupiedNames.Contains(propertyName))
            {
                propertyName += "Property";
            }
            else
            {
                occupiedNames.Add(propertyName);
            }

//             stringBuilder.AppendLine($"""
//                                       /*
//                                       {propertyInfo}
//                                       */
//                                       """
//                                       );

            if (propertyInfo.IsVoid)
            {
                //   get => Get("saved_value") is { VariantType: not Variant.Type.Nil } _result ? _result : (Variant?)null;
                //   set => {backing}Set("{propertyInfo.NativeName}", value is not null ? Variant.From(value) : new Variant());
                stringBuilder
                    .AppendLine($"{TAB1}public Variant? {propertyName}")
                    .AppendLine($"{TAB1}{{")
                    .AppendLine($$"""{{TAB2}}get => {{backing}}Get("{{propertyInfo.NativeName}}") is { VariantType: not Variant.Type.Nil } _result ? _result : (Variant?)null;""")
                    .AppendLine($"""{TAB2}set => {backing}Set("{propertyInfo.NativeName}", value is not null ? Variant.From(value) : new Variant());""")
                    .AppendLine($"{TAB1}}}")
                    .AppendLine();
                
                continue;
            }
            var enumString = propertyInfo.IsEnum && propertyInfo.Type == Variant.Type.Int ? ".As<Int64>()" : string.Empty;
            var castTypeName =  typeName;
            var getter = $"({castTypeName}){backing}Get(\"{propertyInfo.NativeName}\"){enumString}";
            if (propertyInfo.IsArray)
            {
                var typeClass = godotSharpTypeNameMap.GetValueOrDefault(propertyInfo.TypeClass, propertyInfo.TypeClass);
                typeName = typeName.Replace("Godot.GodotObject", typeClass);
                getter = gdeTypeMap.ContainsKey(typeClass) ? $"{STATIC_HELPER_CLASS}.{CastMethodName}<{typeClass}>({getter})" : getter.Replace("Godot.GodotObject", typeClass);
            }
            stringBuilder
                .AppendLine($"{TAB1}public {typeName} {propertyName}")
                .AppendLine($"{TAB1}{{")
                .AppendLine($"""{TAB2}get => {getter};""")
                .AppendLine($"""{TAB2}set => {backing}Set("{propertyInfo.NativeName}", Variant.From(value));""")
                .AppendLine($"{TAB1}}}")
                .AppendLine();
        }

        stringBuilder.AppendLine(
            """
            #endregion

            """
        );
    }
}