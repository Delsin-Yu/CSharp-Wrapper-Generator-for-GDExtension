using System.Collections.Generic;
using System.Text;

namespace GDExtensionAPIGenerator;

internal static partial class CodeGenerator
{
    private static void ConstructProperties(
        ICollection<string> occupiedNames,
        IReadOnlyList<PropertyInfo> propertyInfos,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        StringBuilder stringBuilder,
        string backing
    )
    {
        if (propertyInfos.Count != 0)
        {
            stringBuilder.AppendLine(
                """
                #region Properties

                """
            );
        }

        foreach (var propertyInfo in propertyInfos)
        {
            if (propertyInfo.IsGroupOrSubgroup) continue;

            var typeName = propertyInfo.GetTypeName();

            godotSharpTypeNameMap.GetValueOrDefault(typeName, typeName);
            
            var propertyName = propertyInfo.GetPropertyName();

            if (occupiedNames.Contains(propertyName))
            {
                propertyName += "Property";
            }
            else
            {
                occupiedNames.Add(propertyName);
            }

            stringBuilder.AppendLine($"""
                                      /*
                                      {propertyInfo}
                                      */
                                      """
                                      );

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
            }
            else
            {
                stringBuilder
                    .AppendLine($"{TAB1}public {typeName} {propertyName}")
                    .AppendLine($"{TAB1}{{")
                    .AppendLine($"""{TAB2}get => ({typeName}){backing}Get("{propertyInfo.NativeName}");""")
                    .AppendLine($"""{TAB2}set => {backing}Set("{propertyInfo.NativeName}", Variant.From(value));""")
                    .AppendLine($"{TAB1}}}")
                    .AppendLine();
            }
        }

        if (propertyInfos.Count != 0)
        {
            stringBuilder.AppendLine(
                """
                #endregion

                """
            );
        }
    }
}