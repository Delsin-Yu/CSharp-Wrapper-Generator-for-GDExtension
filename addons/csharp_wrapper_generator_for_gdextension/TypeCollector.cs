#if TOOLS
// ReSharper disable InvertIf
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable TypeWithSuspiciousEqualityIsUsedInRecord.Local

// ReSharper disable SuggestVarOrType_BuiltInTypes
// ReSharper disable SuggestVarOrType_Elsewhere

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using GodotName = string;
using CSharpName = string;
using GodotDictionary = Godot.Collections.Dictionary;

namespace GDExtensionAPIGenerator;

public partial class WrapperGeneratorMain
{
    private static partial class TypeCollector
    {

        public static void CreateClassDiagram(out GodotClassType[] gdExtensionTypes)
        {
            var constructedTypes = new GodotTypeMap();

            PopulateGodotVariantTypes(constructedTypes);
            PopulateGodotClassTypes(constructedTypes);

            PopulateGlobalScopeEnumTypes(constructedTypes);
            PopulateGodotClassMembers(constructedTypes);

            gdExtensionTypes = constructedTypes.Types.Values
                .OfType<GodotClassType>()
                .Where(x => x.ApiType is ClassDB.ApiType.Extension or ClassDB.ApiType.EditorExtension)
                .ToArray();
        }

        private static void PopulateGodotClassTypes(GodotTypeMap godotTypeMap)
        {
            Dictionary<GodotName, CSharpName> godotTypeNameToCSharpTypeNameMap = typeof(GodotObject).Assembly
                .GetTypes()
                .Select(x => (Attribute: x.GetCustomAttributesData().FirstOrDefault(y => y.AttributeType == typeof(GodotClassNameAttribute)), Type: x))
                .Where(x => x.Attribute != null)
                .Select(x => (GodotTypeName: x.Attribute.ConstructorArguments[0].Value!.ToString(), CSharpTypeName: x.Type.Name))
                .GroupBy(x => x.GodotTypeName)
                .ToDictionary(
                    x => x.Key,
                    x =>
                        x.Count() > 1
                            ? x.First(y => !y.CSharpTypeName.Contains("Instance")).CSharpTypeName
                            : x.First().CSharpTypeName
                );


            foreach (GodotName godotClassName in ClassDB.GetClassList().AsSpan())
            {
                CSharpName csharpTypeName = godotTypeNameToCSharpTypeNameMap.GetValueOrDefault(godotClassName, godotClassName);
                if (godotClassName == "Object") continue;
                godotTypeMap.Types.Add(
                    godotClassName,
                    new GodotClassType(
                        godotClassName,
                        csharpTypeName,
                        ClassDB.ClassGetApiType(godotClassName),
                        ClassDB.CanInstantiate(godotClassName)
                    )
                );
            }

            foreach (var godotType in godotTypeMap.Types.Values.OfType<GodotClassType>())
            {
                var parentClass = ClassDB.GetParentClass(godotType.GodotTypeName);
                godotType.ParentType = godotTypeMap.Types[parentClass];
            }
        }

        private static void PopulateGodotVariantTypes(GodotTypeMap godotTypeMap)
        {
            AddType("Nil", Variant.Type.Nil, "void");
            AddType("Bool", Variant.Type.Bool, "bool");
            AddType("Int", Variant.Type.Int, "long");
            AddType("Float", Variant.Type.Float, "double");
            AddType("String", Variant.Type.String, "string");
            AddType("Vector2", Variant.Type.Vector2, nameof(Vector2));
            AddType("Vector2i", Variant.Type.Vector2I, nameof(Vector2I));
            AddType("Rect2", Variant.Type.Rect2, nameof(Rect2));
            AddType("Rect2i", Variant.Type.Rect2I, nameof(Rect2I));
            AddType("Vector3", Variant.Type.Vector3, nameof(Vector3));
            AddType("Vector3i", Variant.Type.Vector3I, nameof(Vector3I));
            AddType("Transform2D", Variant.Type.Transform2D, nameof(Transform2D));
            AddType("Vector4", Variant.Type.Vector4, nameof(Vector4));
            AddType("Vector4i", Variant.Type.Vector4I, nameof(Vector4I));
            AddType("Plane", Variant.Type.Plane, nameof(Plane));
            AddType("Quaternion", Variant.Type.Quaternion, nameof(Quaternion));
            AddType("AABB", Variant.Type.Aabb, nameof(Aabb));
            AddType("Basis", Variant.Type.Basis, nameof(Basis));
            AddType("Transform3D", Variant.Type.Transform3D, nameof(Transform3D));
            AddType("Projection", Variant.Type.Projection, nameof(Projection));
            AddType("Color", Variant.Type.Color, nameof(Color));
            AddType("StringName", Variant.Type.StringName, nameof(StringName));
            AddType("NodePath", Variant.Type.NodePath, nameof(NodePath));
            AddType("Rid", Variant.Type.Rid, nameof(Rid));
            AddType("Callable", Variant.Type.Callable, nameof(Callable));
            AddType("Signal", Variant.Type.Signal, nameof(Signal));
            AddType("Dictionary", Variant.Type.Dictionary, "Godot.Collections.Dictionary");
            AddType("Array", Variant.Type.Array, "Godot.Collections.Array");
            AddType("PackedByteArray", Variant.Type.PackedByteArray, "byte[]");
            AddType("PackedInt32Array", Variant.Type.PackedInt32Array, "int[]");
            AddType("PackedInt64Array", Variant.Type.PackedInt64Array, "long[]");
            AddType("PackedFloat32Array", Variant.Type.PackedFloat32Array, "float[]");
            AddType("PackedFloat64Array", Variant.Type.PackedFloat64Array, "double[]");
            AddType("PackedStringArray", Variant.Type.PackedStringArray, "string[]");
            AddType("PackedVector2Array", Variant.Type.PackedVector2Array, "Vector2[]");
            AddType("PackedVector3Array", Variant.Type.PackedVector3Array, "Vector3[]");
            AddType("PackedColorArray", Variant.Type.PackedColorArray, "Color[]");
            AddType("PackedVector4Array", Variant.Type.PackedVector4Array, "Vector4[]");
            AddType("Object", Variant.Type.Object, nameof(GodotObject));
            return;

            void AddType(GodotName godotTypeName, Variant.Type variantType, string csharpTypeName)
            {
                godotTypeMap.VariantTypeToGodotName.Add(variantType, godotTypeName);
                godotTypeMap.Types.Add(godotTypeName, new GodotAnnotatedVariantType(godotTypeName, csharpTypeName, variantType));
            }
        }

        private static void PopulateGlobalScopeEnumTypes(GodotTypeMap godotTypeMap)
        {
            AddBuiltinEnum<Variant.Type>("Variant.Type", "Type", false, godotTypeMap.Variant);
            AddBuiltinEnum<Error>("Error", "Error", false, null);
            AddBuiltinEnum<Projection.Planes>("Planes", "Planes", false, null);

            return;

            void AddBuiltinEnum<TEnum>(string key, string type, bool isBitField, GodotType ownerType) where TEnum : struct, Enum
            {
                var godotEnumType = new GodotEnumType(type, type, ownerType, isBitField);
                godotTypeMap.GlobalScopeEnumTypes.Add(key, godotEnumType);
                var enumNames = Enum.GetNames<TEnum>();
                var enumValues = Enum.GetValues<TEnum>();
                for (int i = 0; i < enumNames.Length; i++)
                {
                    var enumName = enumNames[i];
                    var enumValue = (long)Convert.ChangeType(enumValues.GetValue(i), typeof(long))!;
                    if (i == 0)
                    {
                        godotEnumType.DefaultEnumValue = enumName;
                    }
                    godotEnumType.EnumConstants.Add((enumName, enumValue));
                }
            }
        }

        private static void PopulateGodotClassMembers(GodotTypeMap godotTypeMap)
        {
            foreach (var godotClassType in godotTypeMap.SelectTypes(ClassDB.ApiType.Core, ClassDB.ApiType.Editor, ClassDB.ApiType.Extension, ClassDB.ApiType.EditorExtension))
            {
                var enumNames = ClassDB.ClassGetEnumList(godotClassType.GodotTypeName, true);
                foreach (var enumName in enumNames)
                {
                    var enumType = new GodotEnumType(enumName, enumName.ToPascalCase(), godotClassType, ClassDB.IsClassEnumBitfield(godotClassType.GodotTypeName, enumName, true));
                    var isFirst = true;
                    foreach (var enumConstant in ClassDB.ClassGetEnumConstants(godotClassType.GodotTypeName, enumName, true))
                    {
                        var enumValue = ClassDB.ClassGetIntegerConstant(godotClassType.GodotTypeName, enumConstant);
                        var enumConstantName = GodotEnumType.FormatEnumName(enumName, enumConstant);
                        enumType.EnumConstants.Add((enumConstantName, enumValue));
                        
                        if (isFirst)
                        {
                            isFirst = false;
                            enumType.DefaultEnumValue = enumConstantName;
                        }
                    }

                    godotClassType.Enums.Add(enumType);
                    if (!godotTypeMap.PreregisteredEnumTypes.TryGetValue(godotClassType, out var preregisteredEnums))
                    {
                        preregisteredEnums = [];
                        godotTypeMap.PreregisteredEnumTypes.Add(godotClassType, preregisteredEnums);
                    }

                    preregisteredEnums.Add(enumName, enumType);
                }
            }

            foreach (var godotClassType in godotTypeMap.SelectTypes(ClassDB.ApiType.Extension, ClassDB.ApiType.EditorExtension))
            {
                var methodDefinitions = ClassDB.ClassGetMethodList(godotClassType.GodotTypeName, true);
                foreach (var methodDefinition in methodDefinitions)
                {
                    var methodInfo = CreateFunctionInfo(godotTypeMap, methodDefinition);
                    godotClassType.Methods.Add(methodInfo);
                }
            }

            foreach (var godotClassType in godotTypeMap.SelectTypes(ClassDB.ApiType.Extension, ClassDB.ApiType.EditorExtension))
            {
                var propertyDefinitions = ClassDB.ClassGetPropertyList(godotClassType.GodotTypeName, true);
                foreach (var propertyDefinition in propertyDefinitions)
                {
                    var propertyInfo = CreatePropertyInfo(propertyDefinition, godotTypeMap);
                    var getter = ClassDB.ClassGetPropertyGetter(godotClassType.GodotTypeName, propertyInfo.GodotName);
                    var setter = ClassDB.ClassGetPropertySetter(godotClassType.GodotTypeName, propertyInfo.GodotName);
                    var getterMethod = godotClassType.Methods.FirstOrDefault(x => x.GodotFunctionName == getter);
                    if (getterMethod != null) godotClassType.Methods.Remove(getterMethod);
                    var setterMethod = godotClassType.Methods.FirstOrDefault(x => x.GodotFunctionName == setter);
                    if (setterMethod != null) godotClassType.Methods.Remove(setterMethod);
                    godotClassType.Properties.Add(new(propertyInfo.GodotName, propertyInfo.GodotName.ToPascalCase(), propertyInfo.Type, getterMethod, setterMethod));
                }
            }

            foreach (var godotClassType in godotTypeMap.SelectTypes(ClassDB.ApiType.Extension, ClassDB.ApiType.EditorExtension))
            {
                var signalDefinitions = ClassDB.ClassGetSignalList(godotClassType.GodotTypeName, true);
                foreach (var signalDefinition in signalDefinitions)
                {
                    var signalInfo = CreateFunctionInfo(godotTypeMap, signalDefinition);
                    godotClassType.Signals.Add(signalInfo);
                }
            }
        }

        private static GodotFunctionInfo CreateFunctionInfo(GodotTypeMap godotTypeMap, GodotDictionary methodDefinition)
        {
            var methodName = methodDefinition["name"].AsString();
            var flags = (MethodFlags)methodDefinition["flags"].AsInt64();
            var id = methodDefinition["id"].AsInt64();
            var returnValue = CreatePropertyInfo(methodDefinition["return"].AsGodotDictionary(), godotTypeMap);

            var methodInfo = new GodotFunctionInfo(
                methodName,
                methodName.ToPascalCase(),
                returnValue,
                id,
                flags
            );

            var args = methodDefinition["args"].AsGodotArray<GodotDictionary>();

            var defaultArgs = methodDefinition["default_args"].AsGodotArray();
            var argsLength = args.Count;
            var defaultArgsLength = defaultArgs.Count;
            var defaultArgsStartIndex = argsLength - defaultArgsLength;

            for (var index = 0; index < argsLength; index++)
            {
                var methodArgumentInfo = args[index];
                var argument = CreatePropertyInfo(methodArgumentInfo, godotTypeMap);
                if (index >= defaultArgsStartIndex)
                {
                    var defaultValue = defaultArgs[index - defaultArgsStartIndex];
                    methodInfo.FunctionArguments.Add(new(argument, defaultValue));
                }
                else
                {
                    methodInfo.FunctionArguments.Add(new(argument, null));
                }
            }

            return methodInfo;
        }


        private static GodotPropertyInfo CreatePropertyInfo(GodotDictionary propertyInfo, GodotTypeMap godotTypeMap)
        {
            var name = propertyInfo["name"].AsString();
            var className = propertyInfo["class_name"].AsString();
            var type = (Variant.Type)propertyInfo["type"].AsInt64();
            var hint = (PropertyHint)propertyInfo["hint"].AsInt64();
            var hintString = propertyInfo["hint_string"].AsString();
            var usage = (PropertyUsageFlags)propertyInfo["usage"].AsInt64();

            var propertyType = GetGodotTypeByPropertyDefinition(godotTypeMap, usage, className, type, hint, hintString);

            return new(name, name.ToCamelCase(), propertyType, hint, hintString, usage);
        }

        private static GodotType GetGodotTypeByPropertyDefinition(GodotTypeMap godotTypeMap, PropertyUsageFlags usage, string className, Variant.Type type, PropertyHint hint, string hintString)
        {
            GodotType propertyType = godotTypeMap.Variant;

            if (hint is PropertyHint.Enum || usage.HasFlag(PropertyUsageFlags.ClassIsEnum))
            {
                var splits = className.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (splits.Length == 2
                    && godotTypeMap.Types.TryGetValue(splits[0], out var matchedEnumOwnerType)
                    && godotTypeMap.PreregisteredEnumTypes.TryGetValue(matchedEnumOwnerType, out var preregisteredEnums)
                    && preregisteredEnums.TryGetValue(splits[1], out var matchedEnumType)) propertyType = matchedEnumType;
                else if (godotTypeMap.GlobalScopeEnumTypes.TryGetValue(className, out var matchedGlobalScopeEnumType))
                    propertyType = matchedGlobalScopeEnumType;
                else if (godotTypeMap.TryGetVariantType(type, out var variantTypeAsEnumFallback))
                    propertyType = new UserUndefinedEnumType(hintString, variantTypeAsEnumFallback);
            }
            else if (type != Variant.Type.Object && godotTypeMap.TryGetVariantType(type, out var variantType))
            {
                propertyType = variantType;
            }
            else if (type is Variant.Type.Object)
            {
                if (godotTypeMap.Types.TryGetValue(className, out var matchedClassType))
                    propertyType = matchedClassType;
                else if (className.Contains(','))
                {
                    var classNameCandidates = className.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    var candidateArray = new GodotType[classNameCandidates.Length];
                    var pass = true;
                    for (int i = 0; i < classNameCandidates.Length; i++)
                    {
                        var candidate = classNameCandidates[i];
                        if (godotTypeMap.Types.TryGetValue(candidate, out var candidateType))
                        {
                            candidateArray[i] = candidateType;
                        }
                        else
                        {
                            pass = false;
                            break;
                        }
                    }

                    if (pass) propertyType = new GodotMultiType(candidateArray);
                    else propertyType = godotTypeMap.Variant;
                }
            }
            else if (type is Variant.Type.Nil && usage.HasFlag(PropertyUsageFlags.NilIsVariant))
            {
                propertyType = godotTypeMap.Variant;
            }
            else if (type == Variant.Type.Array
                     && hint == PropertyHint.ArrayType)
            {
                if (godotTypeMap.Types.TryGetValue(hintString, out var matchedArrayElementType))
                {
                    if (!godotTypeMap.PreregisteredArrayTypes.TryGetValue(matchedArrayElementType, out var arrayType))
                    {
                        arrayType = new(matchedArrayElementType);
                        godotTypeMap.PreregisteredArrayTypes.Add(matchedArrayElementType, arrayType);
                    }

                    propertyType = arrayType;
                }

                var regexMatch = ArrayDataRegex().Match(hintString);
                if (regexMatch.Success
                    && int.TryParse(regexMatch.Groups["arrayVariantType"].Value, out var arrayVariantTypeValue)
                    && int.TryParse(regexMatch.Groups["arrayVariantHint"].Value, out var arrayVariantHintValue))
                {
                    var arrayHintString = regexMatch.Groups["arrayHintString"].Value;
                    var arrayVariantType = (Variant.Type)arrayVariantTypeValue;
                    var arrayVariantHint = (PropertyHint)arrayVariantHintValue;

                    var elementType = GetGodotTypeByPropertyDefinition(godotTypeMap, PropertyUsageFlags.None, string.Empty, arrayVariantType, arrayVariantHint, arrayHintString);

                    if (!godotTypeMap.PreregisteredArrayTypes.TryGetValue(elementType, out var arrayType))
                    {
                        arrayType = new(elementType);
                        godotTypeMap.PreregisteredArrayTypes.Add(elementType, arrayType);
                    }

                    propertyType = arrayType;
                }
            }
            else if (type == Variant.Type.Dictionary
                     && hint == PropertyHint.DictionaryType)
            {
                var regexMatch = DictionaryDataRegex().Match(hintString);
                if (regexMatch.Success
                    && int.TryParse(regexMatch.Groups["keyVariantType"].Value, out var keyVariantTypeValue)
                    && int.TryParse(regexMatch.Groups["keyVariantHint"].Value, out var keyVariantHintValue)
                    && int.TryParse(regexMatch.Groups["valueVariantType"].Value, out var valueVariantTypeValue)
                    && int.TryParse(regexMatch.Groups["valueVariantHint"].Value, out var valueVariantHintValue))
                {
                    var keyHintString = regexMatch.Groups["keyHintString"].Value;
                    var valueHintString = regexMatch.Groups["valueHintString"].Value;
                    var keyVariantType = (Variant.Type)keyVariantTypeValue;
                    var keyVariantHint = (PropertyHint)keyVariantHintValue;
                    var valueVariantType = (Variant.Type)valueVariantTypeValue;
                    var valueVariantHint = (PropertyHint)valueVariantHintValue;

                    var keyType = GetGodotTypeByPropertyDefinition(godotTypeMap, PropertyUsageFlags.None, string.Empty, keyVariantType, keyVariantHint, keyHintString);
                    var valueType = GetGodotTypeByPropertyDefinition(godotTypeMap, PropertyUsageFlags.None, string.Empty, valueVariantType, valueVariantHint, valueHintString);

                    if (!godotTypeMap.PreregisteredDictionaryTypes.TryGetValue((keyType, valueType), out var dictionaryType))
                    {
                        dictionaryType = new(keyType, valueType);
                        godotTypeMap.PreregisteredDictionaryTypes.Add((keyType, valueType), dictionaryType);
                    }

                    propertyType = dictionaryType;
                }
            }

            return propertyType;
        }

        [GeneratedRegex(@"(?<arrayVariantType>\d+)/(?<arrayVariantHint>\d+):(?<arrayHintString>\w+)")]
        private static partial Regex ArrayDataRegex();

        [GeneratedRegex(@"(?<keyVariantType>\d+)/(?<keyVariantHint>\d+):(?<keyHintString>\w+);(?<valueVariantType>\d+)/(?<valueVariantHint>\d+):(?<valueHintString>\w+)")]
        private static partial Regex DictionaryDataRegex();
    }
}
#endif
