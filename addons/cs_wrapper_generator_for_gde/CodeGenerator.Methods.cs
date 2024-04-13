using System.Collections.Generic;
using System.Text;

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
        string backing
    )
    {
        if (methodInfoList.Count != 0)
        {
            stringBuilder.AppendLine(
                """
                #region Methods

                """
            );
        }


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

            stringBuilder
                .Append($"{TAB1}public ")
                .Append(returnValueName)
                .Append(' ')
                .Append(methodName)
                .Append('(');

            BuildupMethodArguments(stringBuilder, methodInfo.Arguments);

            stringBuilder.Append(") => ");

            if (!methodInfo.ReturnValue.IsVoid && 
                gdeTypeMap.TryGetValue(methodInfo.ReturnValue.ClassName, out var returnTypeInfo))
            {
                stringBuilder.Append($"{returnTypeInfo.TypeName}.{VariantToInstanceMethodName}(");
            }

            stringBuilder
                .Append(backing)
                .Append("Call(\"")
                .Append(methodNativeName)
                .Append('"');

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

            stringBuilder.Append(')');

            if (!methodInfo.ReturnValue.IsVoid)
            {
                if (gdeTypeMap.TryGetValue(methodInfo.ReturnValue.ClassName, out returnTypeInfo))
                {
                    stringBuilder.Append(')');
                }
                else
                {
                    stringBuilder.Append($".As<{methodInfo.ReturnValue.GetTypeName()}>()");
                }
            }

            stringBuilder.AppendLine(";").AppendLine();
        }


        if (methodInfoList.Count != 0)
        {
            stringBuilder.AppendLine(
                """
                #endregion

                """
            );
        }
    }
}