using System.Collections.Generic;
using System.Text;
using Godot;

namespace GDExtensionAPIGenerator;

internal static partial class CodeGenerator
{
    private static void ConstructMethods(
        HashSet<string> occupiedNames,
        IReadOnlyList<MethodInfo> methodInfoList,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        IReadOnlyDictionary<string, ClassInfo> gdeTypeMap,
        ICollection<string> builtinTypeNames,
        StringBuilder stringBuilder,
        ClassInfo classInfo,
        string backing
    )
    {
        if (methodInfoList.Count == 0)
        {
            return;
        }
        stringBuilder.AppendLine(
            """
            #region Methods

            """
        );


        foreach (var methodInfo in methodInfoList)
        {
            var methodNativeName = methodInfo.NativeName;
            var returnValueName = methodInfo.ReturnValue.GetTypeName();

            var methodName = methodInfo.GetMethodName();

            if (occupiedNames.Contains(methodName))
            {
                methodName += "Method";
            }
            else
            {
                occupiedNames.Add(methodName);
            }

//             stringBuilder.AppendLine($"""
//                                       /*
//                                       {methodInfo}
//                                       */
//                                       """);
            
            stringBuilder
                .Append($"{TAB1}public ");

            var isVirtual = methodInfo.Flags.HasFlag(MethodFlags.Virtual);
            var isStatic = methodInfo.Flags.HasFlag(MethodFlags.Static);
            
            if (isStatic) stringBuilder.Append("static ");
            if (isVirtual) stringBuilder.Append("virtual ");
            
            stringBuilder
                .Append(returnValueName)
                .Append(' ')
                .Append(methodName)
                .Append('(');

            BuildupMethodArguments(stringBuilder, methodInfo.Arguments, godotSharpTypeNameMap);

            stringBuilder.Append(')');
            
            // TODO: VIRTUAL
            
            stringBuilder.Append(" => ");

            if (!methodInfo.ReturnValue.IsVoid &&
                gdeTypeMap.TryGetValue(methodInfo.ReturnValue.ClassName, out var returnTypeInfo))
            {
                stringBuilder.Append($"{returnTypeInfo.TypeName}.{VariantToInstanceMethodName}(");
            }

            if (isStatic)
            {
                stringBuilder
                    .Append($"{STATIC_HELPER_CLASS}.")
                    .Append("Call(\"")
                    .Append(classInfo.TypeName)
                    .Append("\", \"")
                    .Append(methodNativeName)
                    .Append('"');
            }
            else
            {
                stringBuilder
                    .Append(backing)
                    .Append("Call(\"")
                    .Append(methodNativeName)
                    .Append('"');
            }

            if (methodInfo.Arguments.Length > 0)
            {
                stringBuilder.Append(", ");
                BuildupMethodCallArguments(
                    stringBuilder,
                    methodInfo.Arguments,
                    gdeTypeMap,
                    godotSharpTypeNameMap,
                    builtinTypeNames
                );
            }
            
            // TODO: var isVararg = methodInfo.Flags.HasFlag(MethodFlags.Vararg);

            stringBuilder.Append(')');

            if (!methodInfo.ReturnValue.IsVoid)
            {
                if (gdeTypeMap.TryGetValue(methodInfo.ReturnValue.ClassName, out returnTypeInfo))
                {
                    stringBuilder.Append($".{VariantToGodotObject})");
                }
                else
                {
                    stringBuilder.Append($".As<{methodInfo.ReturnValue.GetTypeName()}>()");
                }
            }

            stringBuilder.AppendLine(";").AppendLine();
        }


        stringBuilder.AppendLine(
            """
            #endregion

            """
        );
    }

    private static void BuildupMethodCallArguments(
        StringBuilder stringBuilder,
        PropertyInfo[] propertyInfos,
        IReadOnlyDictionary<string, ClassInfo> gdeTypeMap,
        IReadOnlyDictionary<string, string> godotsharpTypeMap,
        ICollection<string> builtinTypes
    )
    {
        for (var i = 0; i < propertyInfos.Length; i++)
        {
            var propertyInfo = propertyInfos[i];

            var propertyTypeName = propertyInfo.GetTypeName();
            
            if (gdeTypeMap.TryGetValue(propertyTypeName, out var gdeClassInfo))
            {
                var bassType = GetEngineBaseType(gdeClassInfo, builtinTypes);
                bassType = godotsharpTypeMap.GetValueOrDefault(bassType, bassType);
                stringBuilder.Append($"({bassType})");
            }
            var argumentName = propertyInfo.GetArgumentName();
            
            if (propertyInfo.IsVoid && propertyInfo.Usage.HasFlag(PropertyUsageFlags.NilIsVariant))
            {
                stringBuilder
                    .Append(argumentName)
                    .Append(" ?? new Variant()");
            }
            else if (propertyInfo.IsEnum)
            {
                stringBuilder
                    .Append("Variant.From<")
                    .Append(propertyTypeName)
                    .Append(">(")
                    .Append(argumentName)
                    .Append(')');
            }
            else
            {
                stringBuilder.Append(argumentName);
            }
            
            
            if (i != propertyInfos.Length - 1)
            {
                stringBuilder.Append(", ");
            }
        }
    }
}