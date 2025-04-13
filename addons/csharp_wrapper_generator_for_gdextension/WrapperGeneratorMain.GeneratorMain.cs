#if TOOLS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using GodotName = string;
using CSharpName = string;
using GodotDictionary = Godot.Collections.Dictionary;
// ReSharper disable InvertIf
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable TypeWithSuspiciousEqualityIsUsedInRecord.Local

// ReSharper disable SuggestVarOrType_BuiltInTypes
// ReSharper disable SuggestVarOrType_Elsewhere


namespace GDExtensionAPIGenerator;

public partial class WrapperGeneratorMain
{
    private static partial class GeneratorMain
    {
        public static void Generate(bool includeTests)
        {
            GD.Print("Generating wrapper code...");
            CreateClassDiagram(out var gdExtensionTypes);
            GD.Print("Finished");
        }

        private static void CreateClassDiagram(out GodotClassType[] gdExtensionTypes)
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
                if(godotClassName == "Object") continue;
                godotTypeMap.Types.Add(godotClassName, new GodotClassType(godotClassName, csharpTypeName, ClassDB.ClassGetApiType(godotClassName)));
            }

            foreach (var godotType in godotTypeMap.Types.Values.OfType<GodotClassType>())
            {
                var parentClass = ClassDB.GetParentClass(godotType.GodotTypeName);
                godotType.ParentType = godotTypeMap.Types[parentClass];
            }
        }
        
        private static void PopulateGodotVariantTypes(GodotTypeMap godotTypeMap)
        {
            AddType(nameof(Variant.Type.Nil), Variant.Type.Nil);
            AddType(nameof(Variant.Type.Bool), Variant.Type.Bool);
            AddType(nameof(Variant.Type.Int), Variant.Type.Int);
            AddType(nameof(Variant.Type.Float), Variant.Type.Float);
            AddType(nameof(Variant.Type.String), Variant.Type.String);
            AddType(nameof(Variant.Type.Vector2), Variant.Type.Vector2);
            AddType("Vector2i", Variant.Type.Vector2I);
            AddType(nameof(Variant.Type.Rect2), Variant.Type.Rect2);
            AddType("Rect2i", Variant.Type.Rect2I);
            AddType(nameof(Variant.Type.Vector3), Variant.Type.Vector3);
            AddType("Vector3i", Variant.Type.Vector3I);
            AddType(nameof(Variant.Type.Transform2D), Variant.Type.Transform2D);
            AddType(nameof(Variant.Type.Vector4), Variant.Type.Vector4);
            AddType("Vector4i", Variant.Type.Vector4I);
            AddType(nameof(Variant.Type.Plane), Variant.Type.Plane);
            AddType(nameof(Variant.Type.Quaternion), Variant.Type.Quaternion);
            AddType("AABB", Variant.Type.Aabb);
            AddType(nameof(Variant.Type.Basis), Variant.Type.Basis);
            AddType(nameof(Variant.Type.Transform3D), Variant.Type.Transform3D);
            AddType(nameof(Variant.Type.Projection), Variant.Type.Projection);
            AddType(nameof(Variant.Type.Color), Variant.Type.Color);
            AddType(nameof(Variant.Type.StringName), Variant.Type.StringName);
            AddType(nameof(Variant.Type.NodePath), Variant.Type.NodePath);
            AddType(nameof(Variant.Type.Rid), Variant.Type.Rid);
            AddType(nameof(Variant.Type.Callable), Variant.Type.Callable);
            AddType(nameof(Variant.Type.Signal), Variant.Type.Signal);
            AddType(nameof(Variant.Type.Dictionary), Variant.Type.Dictionary);
            AddType(nameof(Variant.Type.Array), Variant.Type.Array);
            AddType(nameof(Variant.Type.PackedByteArray), Variant.Type.PackedByteArray);
            AddType(nameof(Variant.Type.PackedInt32Array), Variant.Type.PackedInt32Array);
            AddType(nameof(Variant.Type.PackedInt64Array), Variant.Type.PackedInt64Array);
            AddType(nameof(Variant.Type.PackedFloat32Array), Variant.Type.PackedFloat32Array);
            AddType(nameof(Variant.Type.PackedFloat64Array), Variant.Type.PackedFloat64Array);
            AddType(nameof(Variant.Type.PackedStringArray), Variant.Type.PackedStringArray);
            AddType(nameof(Variant.Type.PackedVector2Array), Variant.Type.PackedVector2Array);
            AddType(nameof(Variant.Type.PackedVector3Array), Variant.Type.PackedVector3Array);
            AddType(nameof(Variant.Type.PackedColorArray), Variant.Type.PackedColorArray);
            AddType(nameof(Variant.Type.PackedVector4Array), Variant.Type.PackedVector4Array);
            godotTypeMap.VariantTypeToGodotName.Add(Variant.Type.Object, "Object");
            godotTypeMap.Types.Add("Object", new GodotAnnotatedVariantType("Object", nameof(GodotObject), Variant.Type.Object));
            return;

            void AddType(GodotName godotTypeName, Variant.Type variantType)
            {
                godotTypeMap.VariantTypeToGodotName.Add(variantType, godotTypeName);
                godotTypeMap.Types.Add(godotTypeName, new GodotAnnotatedVariantType(godotTypeName, variantType.ToString(), variantType));
            }
        }
        
        private static void PopulateGlobalScopeEnumTypes(GodotTypeMap godotTypeMap)
        {
            godotTypeMap.GlobalScopeEnumTypes.Add("Variant.Type", new("Variant.Type", "Variant.Type"));
            godotTypeMap.GlobalScopeEnumTypes.Add("Error", new("Error", "Error"));
        }
        
        private static void PopulateGodotClassMembers(GodotTypeMap godotTypeMap)
        {
            foreach (var godotClassType in godotTypeMap.SelectTypes(ClassDB.ApiType.Core, ClassDB.ApiType.Editor, ClassDB.ApiType.Extension, ClassDB.ApiType.EditorExtension))
            {
                var enumNames = ClassDB.ClassGetEnumList(godotClassType.GodotTypeName, true);
                foreach (var enumName in enumNames)
                {
                    var enumType = new GodotUserDefinedEnumType(enumName, enumName.ToPascalCase(), ClassDB.IsClassEnumBitfield(godotClassType.GodotTypeName, enumName, true));
                    foreach (var enumConstant in ClassDB.ClassGetEnumConstants(godotClassType.GodotTypeName, enumName, true))
                    {
                        var enumValue = ClassDB.ClassGetIntegerConstant(godotClassType.GodotTypeName, enumConstant);
                        enumType.EnumConstants.Add((enumConstant, enumValue));
                    }
                    
                    godotClassType.Enums.Add(enumType);
                    if(!godotTypeMap.PreregisteredEnumTypes.TryGetValue(godotClassType, out var preregisteredEnums))
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
                else if(godotTypeMap.GlobalScopeEnumTypes.TryGetValue(className, out var matchedGlobalScopeEnumType))
                    propertyType = matchedGlobalScopeEnumType;
                else if(godotTypeMap.TryGetVariantType(type, out var variantTypeAsEnumFallback))
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
        
        
        #region Models

        [DebuggerTypeProxy(typeof(GodotClassTypeDebugView))]
        private record GodotClassType(
            GodotName GodotTypeName,
            CSharpName CSharpTypeName,
            ClassDB.ApiType ApiType) : GodotNamedType(GodotTypeName, CSharpTypeName)
        {
            public GodotNamedType ParentType { get; set; }
            public List<GodotFunctionInfo> Methods { get; } = [];
            public List<GodotFunctionInfo> Signals { get; } = [];
            public List<GodotClassPropertyInfo> Properties { get; } = [];
            public List<GodotUserDefinedEnumType> Enums { get; } = [];

            public override string ToString() => $"Class<{GodotTypeName}>";

            private class GodotClassTypeDebugView(GodotClassType godotClassType)
            {
                public List<GodotFunctionInfo> Methods => godotClassType.Methods;
                public List<GodotFunctionInfo> Signals => godotClassType.Signals;
                public List<GodotClassPropertyInfo> Properties => godotClassType.Properties;
                public List<GodotUserDefinedEnumType> Enums => godotClassType.Enums;
                // ReSharper disable once InconsistentNaming
                public GodotNamedType _ParentType => godotClassType.ParentType;
            }
        }

        private record GodotAnnotatedVariantType(
            GodotName GodotTypeName,
            CSharpName CSharpTypeName,
            Variant.Type VariantType) : GodotNamedType(GodotTypeName, CSharpTypeName)
        {
            public override string ToString() =>
                VariantType switch
                {
                    Variant.Type.Nil => "void",
                    _ => VariantType.ToString().ToCamelCase()
                };
        }

        private record GodotVariantType() : GodotNamedType("variant", nameof(Variant))
        {
            public override string ToString() => "Variant";
        }

        private record GodotUserDefinedEnumType(GodotName GodotTypeName, CSharpName CSharpTypeName, bool IsBitField) : GodotEnumType(GodotTypeName, CSharpTypeName)
        {
            public override string ToString() => IsBitField ? $"Flags<{GodotTypeName}>" : $"Enum<{GodotTypeName}>";

            public List<(string EnumName, long EnumValue)> EnumConstants { get; } = [];
        }

        private record UserUndefinedEnumType(string EnumDefine, GodotAnnotatedVariantType BackedType) : GodotType
        {
            public override string ToString() => $"UnDefEnum<{BackedType}/*{EnumDefine}*/>";
        }

        private record GodotEnumType(GodotName GodotTypeName, CSharpName CSharpTypeName) : GodotNamedType(GodotTypeName, CSharpTypeName)
        {
            public override string ToString() => $"BuiltInEnum<{GodotTypeName}>";
        }
        
        private record GodotNamedType(GodotName GodotTypeName, CSharpName CSharpTypeName) : GodotType
        {
            public override string ToString() => GodotTypeName;
        }

        private record GodotAnnotatedDictionaryType(GodotType KeyType, GodotType ValueType) : GodotType
        {
            public override string ToString() => $"Dictionary<{KeyType}, {ValueType}>";
        }
        
        private record GodotAnnotatedArrayType(GodotType ElementType) : GodotType
        {
            public override string ToString() => $"Array<{ElementType}>";
        }

        private record GodotType;

        private record GodotMultiType(GodotType[] Types) : GodotType
        {
            public override string ToString() => $"<{string.Join<GodotType>(", ", Types)}>";
        }
        
        private record GodotPropertyInfo(
            string GodotName,
            string CSharpName,
            GodotType Type,
            PropertyHint Hint,
            string HintString,
            PropertyUsageFlags Usage)
        {
            public override string ToString() => $"{Type} {CSharpName}";
        }

        private readonly record struct GodotMethodArgument(GodotPropertyInfo Info, Variant? Default)
        {
            public override string ToString() => Default == null ? Info.ToString() : $"{Info} = {Default}";
        }
        
        private record GodotClassPropertyInfo(GodotName GodotPropertyName, CSharpName CSharpPropertyName, GodotType GodotPropertyType, GodotFunctionInfo Setter, GodotFunctionInfo Getter)
        {
            public override string ToString() =>
                (Getter, Setter) switch
                {
                    (_, null) => $"{GodotPropertyType} {CSharpPropertyName} {{ get; }}",
                    (null, _) => $"{GodotPropertyType} {CSharpPropertyName} {{ set; }}",
                    (_, _) => $"{GodotPropertyType} {CSharpPropertyName} {{ get; set; }}",
                };
        }

        private record GodotFunctionInfo(
            GodotName GodotFunctionName,
            CSharpName CSharpFunctionName,
            GodotPropertyInfo ReturnValue,
            long MethodId,
            MethodFlags Flags)
        {
            public List<GodotMethodArgument> FunctionArguments { get; } = [];
            public override string ToString() => $"[{Flags}] {ReturnValue.Type} {CSharpFunctionName}({string.Join(", ", FunctionArguments)})";
        }

        private class GodotTypeMap
        {
            public Dictionary<GodotName, GodotNamedType> Types { get; } = [];
            public Dictionary<Variant.Type, string> VariantTypeToGodotName { get; } = [];
            public GodotVariantType Variant { get; } = new();
            public Dictionary<GodotType, GodotAnnotatedArrayType> PreregisteredArrayTypes { get; } = [];
            public Dictionary<(GodotType, GodotType), GodotAnnotatedDictionaryType> PreregisteredDictionaryTypes { get; } = [];
            public Dictionary<GodotNamedType, Dictionary<GodotName, GodotUserDefinedEnumType>> PreregisteredEnumTypes { get; } = [];
            public Dictionary<GodotName, GodotEnumType> GlobalScopeEnumTypes { get; } = [];
            
            public bool TryGetVariantType(Variant.Type variantTypeEnum, out GodotAnnotatedVariantType variantType)
            {
                if(!VariantTypeToGodotName.TryGetValue(variantTypeEnum, out var variantTypeName))
                {
                    variantType = null;
                    return false;
                }
                
                if (!Types.TryGetValue(variantTypeName, out var type))
                {
                    variantType = null;
                    return false;
                }

                variantType = (GodotAnnotatedVariantType)type;
                return true;
            }

            public IEnumerable<GodotClassType> SelectTypes(params ClassDB.ApiType[] apiTypes) => 
                Types.Values.OfType<GodotClassType>().Where(x => apiTypes.Contains(x.ApiType));
        }

        #endregion
    }
}
#endif