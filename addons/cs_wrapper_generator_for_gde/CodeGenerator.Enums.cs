using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Godot;

namespace GDExtensionAPIGenerator;

internal static partial class CodeGenerator
{
    private static void ConstructEnums(
        ICollection<string> occupiedNames,
        IReadOnlyList<string> enumList,
        StringBuilder codeBuilder,
        ClassInfo gdeTypeInfo,
        ConcurrentDictionary<string, string> enumNameToConstantMap
    )
    {
        if (enumList.Count == 0)
        {
            return;
        }
        
        codeBuilder.AppendLine(
            """
            #region Enums

            """
        );
        

        foreach (var enumName in enumList)
        {
            var enumFormatName = EscapeAndFormatName(enumName);

            if (occupiedNames.Contains(enumFormatName))
            {
                enumFormatName += "Enum";
            }
            else
            {
                occupiedNames.Add(enumFormatName);
            }

            codeBuilder.Append(
                $$"""
                  {{TAB1}}public enum {{enumFormatName}} : long
                  {{TAB1}}{

                  """
            );

            var enumConstants = ClassDB.ClassGetEnumConstants(gdeTypeInfo.TypeName, enumName, true);

            foreach (var enumConstant in enumConstants)
            {
     
                var enumIntValue = ClassDB.ClassGetIntegerConstant(gdeTypeInfo.TypeName, enumConstant);

                var formatEnumConstant = EscapeAndFormatName(enumConstant);
                var index = formatEnumConstant.ToUpperInvariant().IndexOf(enumFormatName.ToUpperInvariant(), StringComparison.Ordinal);
                if (index != -1) formatEnumConstant = formatEnumConstant.Remove(index, enumFormatName.Length);

                formatEnumConstant = EscapeAndFormatName(formatEnumConstant);
                enumNameToConstantMap.TryAdd(formatEnumConstant, enumFormatName);
                codeBuilder.AppendLine($"{TAB2}{formatEnumConstant} = {enumIntValue},");
            }

            codeBuilder
                .AppendLine($"{TAB1}}}")
                .AppendLine();
        }

        codeBuilder.AppendLine(
            """
            #endregion

            """
        );
    }
}