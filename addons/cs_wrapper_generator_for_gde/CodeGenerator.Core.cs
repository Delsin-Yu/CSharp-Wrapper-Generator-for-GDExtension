using System;
using System.Collections.Generic;
using System.Linq;
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
        IReadOnlyDictionary<string, ClassInfo> gdeTypeMap,
        ICollection<string> godotBuiltinClassNames

    )
    {
        var codeBuilder = new StringBuilder();

        // We have three ways for generating wrappers. 
        
        switch (GetBaseType(gdeTypeInfo))
        {
            case BaseType.Other:
                GenerateCodeForOther(codeBuilder, gdeTypeInfo, gdeTypeMap, godotSharpTypeNameMap, godotBuiltinClassNames);
                break;
            case BaseType.Node:
                GenerateCodeForNode(codeBuilder, gdeTypeInfo, gdeTypeMap, godotSharpTypeNameMap, godotBuiltinClassNames);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return (gdeTypeInfo.TypeName, codeBuilder.ToString());
    }
    
    private enum BaseType
    {
        Other,
        Node
    }

    private static BaseType GetBaseType(ClassInfo classInfo)
    {
        if (ContainsParent(classInfo, nameof(Node))) return BaseType.Node;
        return BaseType.Other;
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

    private const string TAB1 = "    ";
    private const string TAB2 = TAB1 + TAB1;
    private const string TAB3 = TAB2 + TAB1;
    private const string TAB4 = TAB2 + TAB2;
    private const string TAB5 = TAB4 + TAB1;
    private const string TAB6 = TAB3 + TAB3;
    private const string NAMESPACE = "GDExtension.Wrappers";

    private static void GenerateCodeForNode(
        StringBuilder codeBuilder,
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, ClassInfo> gdeTypeMap,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        ICollection<string> godotBuiltinClassNames
    )
    {
        var displayTypeName = godotSharpTypeNameMap.GetValueOrDefault(gdeTypeInfo.TypeName, gdeTypeInfo.TypeName);
        var displayParentTypeName = godotSharpTypeNameMap.GetValueOrDefault(gdeTypeInfo.ParentType.TypeName, gdeTypeInfo.ParentType.TypeName);
        
        codeBuilder.AppendLine(
            $$"""
              using Godot;

              namespace {{NAMESPACE}};

              public partial class {{displayTypeName}} : {{displayParentTypeName}}
              {
              
              {{TAB1}}public static {{displayTypeName}} Construct()
              {{TAB1}}{
              {{TAB2}}var instance = ClassDB.Instantiate("{{gdeTypeInfo.TypeName}}").As<{{displayParentTypeName}}>();
              {{TAB2}}instance.SetScript(ResourceLoader.Load("{{GeneratorMain.GetWrapperPath(displayTypeName)}}"));
              {{TAB2}}return instance.GetScript().As<{{displayTypeName}}>();
              {{TAB1}}}
              
              """
        );

        GenerateMembers(
            codeBuilder,
            gdeTypeInfo,
            gdeTypeMap,
            godotSharpTypeNameMap,
            godotBuiltinClassNames,
            string.Empty
        );
        codeBuilder.Append('}');
    }

    private static void GenerateCodeForOther(
        StringBuilder codeBuilder,
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, ClassInfo> gdeTypeMap,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        ICollection<string> godotBuiltinClassNames
    )
    {
        var displayTypeName = godotSharpTypeNameMap.GetValueOrDefault(gdeTypeInfo.TypeName, gdeTypeInfo.TypeName);
        var displayParentTypeName = godotSharpTypeNameMap.GetValueOrDefault(gdeTypeInfo.ParentType.TypeName, gdeTypeInfo.ParentType.TypeName);
        
        const string backingName = "_backing";
        const string backingArgument = "backing";
        const string constructMethodName = "Construct";
        var baseType = GetParentGDERootParent(gdeTypeInfo, godotBuiltinClassNames);

        var isRootWrapper = gdeTypeInfo.ParentType.TypeName == baseType || godotBuiltinClassNames.Contains(gdeTypeInfo.ParentType.TypeName);

        baseType = godotSharpTypeNameMap.GetValueOrDefault(baseType, baseType);
        
        if (isRootWrapper)
        {
            codeBuilder.AppendLine(
                $$"""
                  using System;
                  using Godot;

                  namespace {{NAMESPACE}};

                  public class {{displayTypeName}} : IDisposable
                  {
                  
                  {{TAB1}}protected virtual {{baseType}} {{constructMethodName}}() =>
                  {{TAB2}}({{baseType}})ClassDB.Instantiate("{{gdeTypeInfo.TypeName}}");
                  
                  {{TAB1}}protected readonly {{baseType}} {{backingName}};
                  
                  {{TAB1}}public {{displayTypeName}}() => {{backingName}} = {{constructMethodName}}();
                  
                  {{TAB1}}public {{displayTypeName}}({{baseType}} {{backingArgument}}) => {{backingName}} = {{backingArgument}};
                  
                  {{TAB1}}public void Dispose() => {{backingName}}.Dispose();
                  
                  """
            );
        }
        else
        {
            codeBuilder.AppendLine(
                $$"""
                  using Godot;

                  namespace {{NAMESPACE}};

                  public class {{displayTypeName}} : {{displayParentTypeName}}
                  {

                  {{TAB1}}protected override {{baseType}} {{constructMethodName}}() =>
                  {{TAB2}}({{baseType}})ClassDB.Instantiate("{{gdeTypeInfo.TypeName}}");

                  """
            );
        }

        GenerateMembers(
            codeBuilder,
            gdeTypeInfo,
            gdeTypeMap,
            godotSharpTypeNameMap,
            godotBuiltinClassNames,
            $"{backingName}."
        );
        
        codeBuilder.Append('}');
    }


    private static void GenerateMembers(
        StringBuilder codeBuilder,
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, ClassInfo> gdeTypeMap,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        ICollection<string> godotBuiltinClassNames,
        string backingName
    )
    {
        var propertyInfoList = CollectPropertyInfo(gdeTypeInfo);
        ConstructEnums(propertyInfoList, codeBuilder, gdeTypeInfo);
        ConstructSignals(codeBuilder, gdeTypeMap, godotSharpTypeNameMap, godotBuiltinClassNames, gdeTypeInfo,backingName);
        ConstructProperties(propertyInfoList, godotSharpTypeNameMap, codeBuilder, backingName);
        ConstructMethods(gdeTypeInfo, godotSharpTypeNameMap, gdeTypeMap, godotBuiltinClassNames, propertyInfoList, codeBuilder, backingName);
    }


    private static IReadOnlyList<PropertyInfo> CollectPropertyInfo(ClassInfo gdeTypeInfo)
    {
        var propertyList = ClassDB.ClassGetPropertyList(gdeTypeInfo.TypeName, true);

        var propertyInfoList = new List<PropertyInfo>();
        
        foreach (var propertyDictionary in propertyList)
        {
            var propertyInfo = new PropertyInfo(propertyDictionary);
            propertyInfoList.Add(propertyInfo);
            propertyDictionary.Dispose();
        }

        return propertyInfoList;
    }
    
    private static void ConstructEnums(
        IReadOnlyList<PropertyInfo> propertyInfos,
        StringBuilder codeBuilder,
        ClassInfo gdeTypeInfo
        )
    {
        var enumList = ClassDB.ClassGetEnumList(gdeTypeInfo.TypeName, true);
        if (enumList.Length != 0)
        {
            codeBuilder.AppendLine("""
                                   #region Enums
                                   
                                   """
                                   );
        }
        
        foreach (var enumName in enumList)
        {
            var enumFormatName = EscapeAndFormatName(enumName);

            if (propertyInfos.Any(x => x.GetPropertyName() == enumFormatName))
            {
                enumFormatName += "Enum";
            }
            
            codeBuilder.Append($$"""
                                 {{TAB1}}public enum {{enumFormatName}}
                                 {{TAB1}}{

                                 """);

            var enumConstants = ClassDB.ClassGetEnumConstants(gdeTypeInfo.TypeName, enumName, true);

            foreach (var enumConstant in enumConstants)
            {
                var enumIntValue = ClassDB.ClassGetIntegerConstant(gdeTypeInfo.TypeName, enumConstant);
                var enumValueFormatName = EscapeAndFormatName(enumConstant);
                var index = enumValueFormatName.IndexOf(enumFormatName, StringComparison.Ordinal);
                if (index != -1) enumValueFormatName = enumValueFormatName.Remove(index, enumFormatName.Length);
                codeBuilder.AppendLine($"{TAB2}{enumValueFormatName} = {enumIntValue},");
            }
            
            codeBuilder
                .AppendLine($"{TAB1}}}")
                .AppendLine();
        }

        if (enumList.Length != 0)
        {
            codeBuilder.AppendLine("""
                                   #endregion

                                   """
            );
        }
    }

    private static void ConstructSignals(
        StringBuilder codeBuilder,
        IReadOnlyDictionary<string, ClassInfo> gdeTypeMap,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        ICollection<string> builtinTypes,
        ClassInfo gdeTypeInfo,
        string backingName
    )
    {
        var signalList = ClassDB.ClassGetSignalList(gdeTypeInfo.TypeName, true);
        
        if (signalList.Count != 0)
        {
            codeBuilder.AppendLine("""
                                   #region Signals

                                   """
            );
        }
        
        foreach (var signalInfoDictionary in signalList)
        {
            var signalInfo = new MethodInfo(signalInfoDictionary);

            var returnValueName = GetReturnValueName(gdeTypeMap, signalInfo);

            var signalName = signalInfo.GetMethodName();
            var signalDelegateName = $"{signalName}Handler";
            var signalNameCamelCase = ToCamelCase(signalName);
            var backingDelegateName = $"_{signalNameCamelCase}_backing";
            var backingCallableName = $"_{signalNameCamelCase}_backing_callable";

            codeBuilder.Append($"{TAB1}public delegate {returnValueName} {signalDelegateName}(");
            
            BuildupMethodArguments(codeBuilder, signalInfo.Arguments);

            codeBuilder
                .AppendLine(");")
                .AppendLine();


            const string callableName = nameof(Callable);
            
            codeBuilder.Append(
                $$"""
                  {{TAB1}}private {{signalDelegateName}}? {{backingDelegateName}};
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
            const string unmanagedPostfix = "_unmanaged";

            static string UnmanagedArg(int index) => 
                $"{argPrefix}{index}{unmanagedPostfix}";
                       
            static string Arg(int index) => 
                $"{argPrefix}{index}";
            
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
                var unmanagedArgName = UnmanagedArg(index);
                var convertedArgName = Arg(index);
                var argumentType = argumentInfo.GetTypeName();
                argumentType = godotSharpTypeNameMap.GetValueOrDefault(argumentType, argumentType);
                if (gdeTypeMap.TryGetValue(argumentType, out var gdeType))
                {
                    GenerateVariantToWrapperCode(
                        TAB6,
                        gdeType,
                        builtinTypes,
                        godotSharpTypeNameMap,
                        codeBuilder,
                        unmanagedArgName,
                        convertedArgName
                    );
                }
                else
                {
                    codeBuilder.AppendLine(
                        $"{TAB6}var {convertedArgName} = {unmanagedArgName}.As<{argumentType}>();"
                    );
                }
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
            
            signalInfoDictionary.Dispose();
        }
        
        if (signalList.Count != 0)
        {
            codeBuilder.AppendLine("""
                                   #endregion

                                   """
            );
        }
    }
    
    private readonly struct PropertyInfo
    {
        public readonly Variant.Type Type = Variant.Type.Nil;
        public readonly string NativeName;
        public readonly string ClassName;
        public readonly PropertyHint Hint = PropertyHint.None;
        public readonly string HintString;
        public readonly PropertyUsageFlags Usage = PropertyUsageFlags.Default;

        public PropertyInfo(Dictionary dictionary)
        {
            using var nameInfo = dictionary["name"];
            using var classNameInfo = dictionary["class_name"];
            using var typeInfo = dictionary["type"];
            using var hintInfo = dictionary["hint"];
            using var hintStringInfo = dictionary["hint_string"];
            using var usageInfo = dictionary["usage"];
            
            Type = typeInfo.As<Variant.Type>();;
            NativeName = nameInfo.AsString();
            ClassName = classNameInfo.AsString(); 
            Hint = hintInfo.As<PropertyHint>();
            HintString = hintStringInfo.AsString();
            Usage = usageInfo.As<PropertyUsageFlags>();
        }

        public bool IsGroupOrSubgroup => Usage.HasFlag(PropertyUsageFlags.Group) || Usage.HasFlag(PropertyUsageFlags.Subgroup);
        public bool IsVoid => Type is Variant.Type.Nil;
        
        public string GetTypeName() => VariantToTypeName(Type, ClassName);
        public string GetPropertyName() => EscapeAndFormatName(NativeName);
        public string GetArgumentName() => EscapeAndFormatName(NativeName, true);
    }
    
    private static void ConstructProperties(
        IReadOnlyList<PropertyInfo> propertyInfos,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        StringBuilder stringBuilder,
        string backing
    )
    {
        if (propertyInfos.Count != 0)
        {
            stringBuilder.AppendLine("""
                                     #region Properties

                                     """
            );
        }
        
        foreach (var propertyInfo in propertyInfos)
        {
            if (propertyInfo.IsGroupOrSubgroup) continue;

            var typeName = propertyInfo.GetTypeName();

            godotSharpTypeNameMap.GetValueOrDefault(typeName, typeName);

            stringBuilder
                .AppendLine($"{TAB1}public {typeName} {propertyInfo.GetPropertyName()}")
                .AppendLine($"{TAB1}{{")
                .AppendLine($"""{TAB2}get => ({typeName}){backing}Get("{propertyInfo.NativeName}");""")
                .AppendLine($"""{TAB2}set => {backing}Set("{propertyInfo.NativeName}", Variant.From(value));""")
                .AppendLine($"{TAB1}}}")
                .AppendLine();
        }
        
        if (propertyInfos.Count != 0)
        {
            stringBuilder.AppendLine("""
                                     #endregion

                                     """
            );
        }
    }

    private readonly struct MethodInfo
    {
        public readonly string NativeName;
        public readonly PropertyInfo ReturnValue;
        public readonly MethodFlags Flags;
        public readonly int Id = 0;
        public readonly PropertyInfo[] Arguments;
        public readonly Variant[] DefaultArguments;

        public MethodInfo(Dictionary dictionary)
        {
            using var nameInfo = dictionary["name"];
            using var argsInfo = dictionary["args"];
            using var defaultArgsInfo = dictionary["default_args"];
            using var flagsInfo = dictionary["flags"];
            using var idInfo = dictionary["id"];
            using var returnInfo = dictionary["return"];

            NativeName = nameInfo.AsString();
            ReturnValue = new(returnInfo.As<Dictionary>());
            Flags = flagsInfo.As<MethodFlags>();
            Id = idInfo.AsInt32();
            Arguments = argsInfo.As<Array<Dictionary>>().Select(x => new PropertyInfo(x)).ToArray();
            DefaultArguments = defaultArgsInfo.As<Array<Variant>>().ToArray();
        }
        
        public string GetMethodName() => EscapeAndFormatName(NativeName);
    }
    
    private static void ConstructMethods(
        ClassInfo gdeTypeInfo,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        IReadOnlyDictionary<string, ClassInfo> gdeTypeMap,
        ICollection<string> builtinTypeNames,
        IReadOnlyList<PropertyInfo> propertyInfos,
        StringBuilder stringBuilder,
        string backing
    )
    {
        var methodInfoList = ClassDB
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
                    return propertyInfos.Any(
                        propertyInfo =>
                        {
                            var propertyNativeName = propertyInfo.NativeName;
                            if (methodNativeName.Contains(propertyNativeName))
                            {
                                var index = methodNativeName.IndexOf(propertyNativeName, StringComparison.Ordinal);
                                var spiltResult = methodNativeName.Remove(index, propertyNativeName.Length);
                                if (spiltResult is "set_" or "get_") return false;
                            }

                            var propertyNativeNameEscaped = EscapeNameRegex().Replace(propertyNativeName, "_");
                            if (methodNativeName.Contains(propertyNativeNameEscaped))
                            {
                                var index = methodNativeName.IndexOf(propertyNativeNameEscaped, StringComparison.Ordinal);
                                var spiltResult = methodNativeName.Remove(index, propertyNativeNameEscaped.Length);
                                if (spiltResult is "set_" or "get_") return false;
                            }

                            return true;
                        }
                    );
                }
            )
            .ToArray();

        if (methodInfoList.Length != 0)
        {
            stringBuilder.AppendLine("""
                                     #region Methods

                                     """
            );
        }

        
        foreach (var methodInfo in methodInfoList)
        {
            var methodNativeName = methodInfo.NativeName;
            var returnValueName = GetReturnValueName(gdeTypeMap, methodInfo);

            stringBuilder
                .Append($"{TAB1}public ")
                .Append(returnValueName)
                .Append(' ')
                .Append(methodInfo.GetMethodName())
                .Append('(');
            
            BuildupMethodArguments(stringBuilder, methodInfo.Arguments);

            stringBuilder.Append(") => ");

            if (!methodInfo.ReturnValue.IsVoid && gdeTypeMap.TryGetValue(methodInfo.ReturnValue.ClassName, out var returnTypeInfo))
            {
                stringBuilder.Append("new(");
            }
            
            stringBuilder
                .Append(backing)
                .Append("Call(\"")
                .Append(methodNativeName)
                .Append('"');

            if (methodInfo.Arguments.Length > 0)
            {
                stringBuilder.Append(", ");
                BuildupMethodCallArguments(stringBuilder, methodInfo.Arguments, gdeTypeMap, godotSharpTypeNameMap, builtinTypeNames);
            }

            stringBuilder.Append(')');

            if (!methodInfo.ReturnValue.IsVoid)
            {
                if (gdeTypeMap.TryGetValue(methodInfo.ReturnValue.ClassName, out returnTypeInfo))
                {
                    var interopType = GetParentGDERootParent(returnTypeInfo, builtinTypeNames);
                    interopType = godotSharpTypeNameMap.GetValueOrDefault(interopType, interopType);
                    stringBuilder.Append($".As<{interopType}>())");
                }
                else
                {
                    stringBuilder.Append($".As<{methodInfo.ReturnValue.GetTypeName()}>()");
                }
            }
            
            stringBuilder.AppendLine(";").AppendLine();
        }
        
                
        if (methodInfoList.Length != 0)
        {
            stringBuilder.AppendLine("""
                                     #endregion

                                     """
            );
        }
    }

    private static string GetReturnValueName(IReadOnlyDictionary<string, ClassInfo> gdeTypeMap, MethodInfo methodInfo)
    {
        var returnValueName = methodInfo.ReturnValue.GetTypeName();
        if (gdeTypeMap.TryGetValue(returnValueName, out var returnTypeInfo))
        {
            returnValueName = returnTypeInfo.ParentType.TypeName switch
            {
                nameof(Node) => $"{NAMESPACE}.{returnValueName}",
                nameof(RefCounted) => $"{NAMESPACE}.{returnValueName}",
                _ => returnValueName
            };
        }

        return returnValueName;
    }

    private static void GenerateVariantToWrapperCode(
        string tab,
        ClassInfo classInfo,
        ICollection<string> builtinTypes,
        IReadOnlyDictionary<string, string> godotSharpTypeNameMap,
        StringBuilder builder,
        string variantArgumentName,
        string targetArgumentName
    )
    {
        var targetType = classInfo.TypeName;
        var backingType = GetParentGDERootParent(classInfo, builtinTypes);
        backingType = godotSharpTypeNameMap.GetValueOrDefault(backingType, backingType);
        var backingName = $"{variantArgumentName}_backingType";
        builder.AppendLine($"{tab}var {backingName} = {variantArgumentName}.As<{backingType}>();");
        switch (GetBaseType(classInfo) )
        {
            case BaseType.Other:
                builder.AppendLine($"{tab}var {targetArgumentName} = new {targetType}({backingName})");
                break;
            case BaseType.Node:
                builder.Append($"""
                                {tab}{backingName}.SetScript(ResourceLoader.Load("{GeneratorMain.GetWrapperPath(targetType)}"));
                                {tab}var {targetArgumentName} = {backingName}.GetScript().As<{targetType}>();
                                
                                """);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static string GetParentGDERootParent(ClassInfo gdeTypeInfo, ICollection<string> builtinTypes)
    {
        while (true)
        {
            var parentType = gdeTypeInfo.ParentType;
            if (builtinTypes.Contains(parentType.TypeName)) return parentType.TypeName;
            gdeTypeInfo = parentType;
        }
    }

    private static void BuildupMethodArguments(StringBuilder stringBuilder, PropertyInfo[] propertyInfos)
    {
        for (var i = 0; i < propertyInfos.Length; i++)
        {
            var propertyInfo = propertyInfos[i];
            stringBuilder
                .Append(propertyInfo.GetTypeName())
                .Append(' ')
                .Append(propertyInfo.GetArgumentName());

            if (i != propertyInfos.Length - 1)
            {
                stringBuilder.Append(", ");
            }
        }
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
            
            if (gdeTypeMap.TryGetValue(propertyInfo.GetTypeName(), out var gdeClassInfo))
            {
                var bassType = GetParentGDERootParent(gdeClassInfo, builtinTypes);
                bassType = godotsharpTypeMap.GetValueOrDefault(bassType, bassType);
                stringBuilder.Append($"({bassType})");
            }
            
            stringBuilder.Append(propertyInfo.GetArgumentName());
            
            if (i != propertyInfos.Length - 1)
            {
                stringBuilder.Append(", ");
            }
        }
    }

    public static string VariantToTypeName(Variant.Type type, string className) =>
        type switch
        {
            Variant.Type.Aabb => "Aabb",
            Variant.Type.Basis => "Basis",
            Variant.Type.Callable => "Callable",
            Variant.Type.Color => "Color",
            Variant.Type.NodePath => "NodePath",
            Variant.Type.Plane => "Plane",
            Variant.Type.Projection => "Projection",
            Variant.Type.Quaternion => "Quaternion",
            Variant.Type.Rect2 => "Rect2",
            Variant.Type.Rect2I => "Rect2I",
            Variant.Type.Rid => "Rid",
            Variant.Type.Signal => "Signal",
            Variant.Type.StringName => "StringName",
            Variant.Type.Transform2D => "Transform2D",
            Variant.Type.Transform3D => "Transform3D",
            Variant.Type.Vector2 => "Vector2",
            Variant.Type.Vector2I => "Vector2I",
            Variant.Type.Vector3 => "Vector3",
            Variant.Type.Vector3I => "Vector3I",
            Variant.Type.Vector4 => "Vector4",
            Variant.Type.Vector4I => "Vector4I",
            Variant.Type.Nil => "void",
            Variant.Type.PackedByteArray => "byte[]",
            Variant.Type.PackedInt32Array => "int[]",
            Variant.Type.PackedInt64Array => "long",
            Variant.Type.PackedFloat32Array => "float[]",
            Variant.Type.PackedFloat64Array => "double[]",
            Variant.Type.PackedStringArray => "string[]",
            Variant.Type.PackedVector2Array => "Vector2[]",
            Variant.Type.PackedVector3Array => "Vector3[]",
            Variant.Type.PackedColorArray => "Color[]",
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

    private static readonly HashSet<string> _csKeyword =
    [
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
        "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
        "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
        "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
        "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc",
        "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
        "unsafe", "ushort", "using", "virtual", "void", "volatile", "while",
        "string", "object", "var", "dynamic", "yield", "add", "alias", "ascending", "async", "await", "by",
        "descending", "equals", "from", "get", "global", "group", "into", "join", "let", "nameof", "on", "orderby",
        "partial", "remove", "select", "set", "when", "where", "yield"
    ];
    
    public static string EscapeAndFormatName(string sourceName, bool camelCase = false)
    {
        var pascalCaseName = EscapeNameRegex()
            .Replace(sourceName, "_")
            .ToPascalCase();

        if (camelCase)
        {
            pascalCaseName = ToCamelCase(pascalCaseName);
        }
        if (_csKeyword.Contains(pascalCaseName))
        {
            pascalCaseName = $"@{pascalCaseName}";
        }
        return pascalCaseName;
    }

    public static string ToCamelCase(string sourceName)
    {
        if (string.IsNullOrWhiteSpace(sourceName)) return sourceName;
        return sourceName[..1].ToLowerInvariant() + sourceName[1..];
    }
}