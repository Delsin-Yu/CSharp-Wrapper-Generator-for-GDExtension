using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GDExtensionAPIGenerator;

internal static partial class CodeGenerator
{
    private static IReadOnlyList<PropertyInfo> CollectPropertyInfo(ClassInfo gdeTypeInfo) =>
        ClassDB
            .ClassGetPropertyList(gdeTypeInfo.TypeName, true)
            .Select(
                propertyDictionary =>
                {
                    var propertyInfo = new PropertyInfo(propertyDictionary);
                    propertyDictionary.Dispose();
                    return propertyInfo;
                }
            )
            .ToArray();

    private static IReadOnlyList<MethodInfo> CollectMethodInfo(ClassInfo gdeTypeInfo, IReadOnlyList<PropertyInfo> propertyInfos) =>
        ClassDB
            .ClassGetMethodList(gdeTypeInfo.TypeName, true)
            .Select(
                x =>
                {
                    var methodInfo = new MethodInfo(x);
                    x.Dispose();
                    return methodInfo;
                }
            )
            .Where(
                methodInfo =>
                {

                    var methodNativeName = methodInfo.NativeName;

                    if (methodNativeName.Length <= 4) return true;

#if GODOT4_4_OR_GREATER
                    if (propertyInfos.Any(propertyInfo => propertyInfo.IsProperty(methodNativeName)))
                    {
                        return false;
                    }
#else
                    if (methodNativeName.StartsWith("get_") || methodNativeName.StartsWith(("set_")))
                    {
                        var trimmedNativeName = methodNativeName[4..];
                        if (propertyInfos.Any(
                                propertyInfo =>
                                {
                                    if (string.Equals(propertyInfo.NativeName, trimmedNativeName, StringComparison.OrdinalIgnoreCase))
                                        return true;
                                    if (string.Equals(propertyInfo.NativeName.Replace('/', '_'), trimmedNativeName, StringComparison.OrdinalIgnoreCase))
                                        return true;
                                    return false;
                                }
                            )) return false;
                    }
#endif
                    return true;
                }
            )
            .ToArray();

    private static IReadOnlyList<MethodInfo> CollectSignalInfo(ClassInfo gdeTypeInfo) =>
        ClassDB
            .ClassGetSignalList(gdeTypeInfo.TypeName, true)
            .Select(
                signalInfoDictionary =>
                {
                    var signalInfo = new MethodInfo(signalInfoDictionary);
                    signalInfoDictionary.Dispose();
                    return signalInfo;
                }
            )
            .ToArray();

    private static IReadOnlyList<string> CollectEnumInfo(ClassInfo gdeTypeInfo) =>
        ClassDB
            .ClassGetEnumList(gdeTypeInfo.TypeName, true)
            .Select(x => EscapeAndFormatName(x))
            .ToArray();
}