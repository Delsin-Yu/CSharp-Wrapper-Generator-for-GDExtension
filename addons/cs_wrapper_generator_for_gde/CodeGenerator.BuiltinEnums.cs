using System;
using System.Collections.Concurrent;
using System.Linq;
using Godot;

namespace GDExtensionAPIGenerator;

internal static partial class CodeGenerator
{
    private static void PopulateBuiltinEnumTypes(ConcurrentDictionary<string, string> enumNameToConstantMap)
    {
        var types = typeof(GodotObject).Assembly.GetTypes();
        
        foreach (var enumType in types.Where(x => x.IsEnum))
        {
            var enumName = enumType.Name;
            if (enumType.ReflectedType == (typeof(Variant)))
            {
                enumName = $"{nameof(Variant)}.{enumName}";
            }
            foreach (var enumValue in Enum.GetNames(enumType).AsSpan())
            {
                enumNameToConstantMap.AddOrUpdate(enumValue, enumName, (s, s1) => null);
            }
        }
    }
}