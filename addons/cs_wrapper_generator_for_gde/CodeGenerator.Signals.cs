using System.Collections.Generic;
using System.Text;
using Godot;

namespace GDExtensionAPIGenerator;

internal static partial class CodeGenerator
{
    private static void ConstructSignals(
        ICollection<string> occupiedNames,
        IReadOnlyList<MethodInfo> signalList,
        StringBuilder codeBuilder,
        IReadOnlyDictionary<string, ClassInfo> gdeTypeMap,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        ICollection<string> builtinTypes,
        string backingName
    )
    {
        if (signalList.Count != 0)
        {
            codeBuilder.AppendLine(
                """
                #region Signals

                """
            );
        }

        foreach (var signalInfo in signalList)
        {
            var returnValueName = signalInfo.ReturnValue.GetTypeName();

            var signalName = signalInfo.GetMethodName();

            if (occupiedNames.Contains(signalName))
            {
                signalName += "Signal";
            }
            else
            {
                occupiedNames.Add(signalName);
            }

            var signalDelegateName = $"{signalName}Handler";
            var signalNameCamelCase = ToCamelCase(signalName);
            var backingDelegateName = $"_{signalNameCamelCase}_backing";
            var backingCallableName = $"_{signalNameCamelCase}_backing_callable";

            codeBuilder.Append($"{TAB1}public delegate {returnValueName} {signalDelegateName}(");

            BuildupMethodArguments(codeBuilder, signalInfo.Arguments, godotSharpTypeNameMap);

            codeBuilder
                .AppendLine(");")
                .AppendLine();

            const string callableName = nameof(Callable);

            codeBuilder.Append(
                $$"""
                  {{TAB1}}private {{signalDelegateName}} {{backingDelegateName}};
                  {{TAB1}}private {{callableName}} {{backingCallableName}};
                  {{TAB1}}public event {{signalDelegateName}} {{signalName}}
                  {{TAB1}}{
                  {{TAB2}}add
                  {{TAB2}}{
                  {{TAB3}}if({{backingDelegateName}} == null)
                  {{TAB3}}{
                  {{TAB4}}{{backingCallableName}} = {{callableName}}.From
                  """
            );

            var argumentsLength = signalInfo.Arguments.Length;

            if (argumentsLength > 0) codeBuilder.Append('<');

            for (var i = 0; i < argumentsLength; i++)
            {
                codeBuilder.Append(nameof(Variant));

                if (i != argumentsLength - 1)
                {
                    codeBuilder.Append(", ");
                }
            }

            if (argumentsLength > 0) codeBuilder.Append('>');

            codeBuilder.Append(
                $"""
                 (
                 {TAB5}(
                 """
            );

            const string argPrefix = "arg";
            const string variantPostfix = "_variant";

            static string UnmanagedArg(int index) => $"{argPrefix}{index}{variantPostfix}";

            static string Arg(int index) => $"{argPrefix}{index}";

            for (var i = 0; i < argumentsLength; i++)
            {
                codeBuilder.Append(UnmanagedArg(i));

                if (i != argumentsLength - 1)
                {
                    codeBuilder.Append(", ");
                }
            }

            codeBuilder.AppendLine(
                $$"""
                  ) =>
                  {{TAB5}}{
                  """
            );

            for (var index = 0; index < signalInfo.Arguments.Length; index++)
            {
                var argumentInfo = signalInfo.Arguments[index];
                var variantArgName = UnmanagedArg(index);
                var convertedArgName = Arg(index);
                var argumentType = argumentInfo.GetTypeName();
                argumentType = godotSharpTypeNameMap.GetValueOrDefault(argumentType, argumentType);
                if (gdeTypeMap.ContainsKey(argumentType))
                    codeBuilder.AppendLine($"{TAB6}var {convertedArgName} = {STATIC_HELPER_CLASS}.{VariantToInstanceMethodName}<{argumentType}>({variantArgName}.As<GodotObject>());");
                else
                    codeBuilder.AppendLine($"{TAB6}var {convertedArgName} = {variantArgName}.As<{argumentType}>();");
            }

            codeBuilder.Append($"{TAB6}{backingDelegateName}?.Invoke(");

            for (var i = 0; i < argumentsLength; i++)
            {
                codeBuilder.Append(Arg(i));

                if (i != argumentsLength - 1)
                {
                    codeBuilder.Append(", ");
                }
            }

            codeBuilder.AppendLine(");");

            codeBuilder.AppendLine(
                    $$"""
                      {{TAB5}}}
                      {{TAB4}});
                      {{TAB4}}{{backingName}}Connect("{{signalInfo.NativeName}}", {{backingCallableName}});
                      {{TAB3}}}
                      {{TAB3}}{{backingDelegateName}} += value;
                      {{TAB2}}}
                      {{TAB2}}remove
                      {{TAB2}}{
                      {{TAB3}}{{backingDelegateName}} -= value;
                      {{TAB3}}
                      {{TAB3}}if({{backingDelegateName}} == null)
                      {{TAB3}}{
                      {{TAB4}}{{backingName}}Disconnect("{{signalInfo.NativeName}}", {{backingCallableName}});
                      {{TAB4}}{{backingCallableName}} = default;
                      {{TAB3}}}
                      {{TAB2}}}
                      {{TAB1}}}
                      """
                )
                .AppendLine();
        }

        if (signalList.Count != 0)
        {
            codeBuilder.AppendLine(
                """
                #endregion

                """
            );
        }
    }
}