using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicOnion.CodeAnalysis
{
    public class MethodCollector
    {
        static readonly SymbolDisplayFormat binaryWriteFormat = new SymbolDisplayFormat(
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.ExpandNullable,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly);

        static readonly SymbolDisplayFormat typeNameFormat = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes);

        readonly string csProjPath;

        INamedTypeSymbol[] targetTypes;

 

        public MethodCollector(string csProjPath, IEnumerable<string> conditinalSymbols)
        {
            this.csProjPath = csProjPath;
            var compilation = RoslynExtensions.GetCompilationFromProject(csProjPath, conditinalSymbols.ToArray()).GetAwaiter().GetResult();

            targetTypes = compilation.GetNamedTypeSymbols()
                .Where(x =>
                {
                    // TODO:Collect IService
                    if (x.DeclaredAccessibility == Accessibility.Public) return true;
                    return false;
                })
                .ToArray();
        }

        void Init()
        {
            //enumContainer = new List<CodeGenerator.EnumType>();
            //objectContainer = new List<CodeGenerator.ObjectSegmentType>();
            //structContainer = new List<CodeGenerator.ObjectSegmentType>();
            //genericTypeContainer = new List<CodeGenerator.GenericType>();
            //unionTypeContainer = new List<CodeGenerator.UnionType>();
            //alreadyCollected = new HashSet<Tuple<string, bool, bool>>();
        }

        public InterfaceDefintion[] Visit()
        {
            Init(); // cleanup field.
            return null;


            //foreach (var item in targetTypes[TypeKind.Enum])
            //{
            //    CollectEnum(item, true, false);
            //}
            //foreach (var item in targetTypes[TypeKind.Class])
            //{
            //    if (item.GetAttributes().FindAttributeShortName(UnionAttributeShortName) != null)
            //    {
            //        CollectUnion(item);
            //    }
            //    else
            //    {
            //        CollectObjectSegment(item, false, false);
            //    }
            //}
            //foreach (var item in targetTypes[TypeKind.Interface])
            //{
            //    CollectUnion(item);
            //}
            //foreach (var item in targetTypes[TypeKind.Struct])
            //{
            //    CollectObjectSegment(item, false, false);
            //}

            //enumGenerators = enumContainer
            //    .GroupBy(x => x.FullName)
            //    .Select(x =>
            //    {
            //        var nullableEnum = x.Any(y => y.IncludeNullable);
            //        var generateEqualityComparer = x.Any(y => y.IsGenerateEqualityComparer);

            //        var c = x.First();
            //        c.IncludeNullable = nullableEnum;
            //        c.IsGenerateEqualityComparer = generateEqualityComparer;
            //        return c;
            //    })
            //   .GroupBy(x => x.Namespace)
            //   .OrderBy(x => x.Key)
            //   .Select(x => new EnumGenerator
            //   {
            //       Namespace = namespaceRoot + ".DynamicObjectSegments" + ((x.Key != null) ? ("." + x.Key) : ""),
            //       Types = x.ToArray()
            //   })
            //   .ToArray();

            //objectGenerators = objectContainer
            //   .GroupBy(x => x.Namespace)
            //   .Select(x => new ObjectGenerator
            //   {
            //       Namespace = namespaceRoot + ".DynamicObjectSegments" + ((x.Key != null) ? ("." + x.Key) : ""),
            //       Types = x.ToArray(),
            //   })
            //   .ToArray();

            //unionGenerators = unionTypeContainer
            //   .GroupBy(x => x.Namespace)
            //   .Select(x => new UnionGenerator
            //   {
            //       Namespace = namespaceRoot + ".DynamicObjectSegments" + ((x.Key != null) ? ("." + x.Key) : ""),
            //       Types = x.ToArray(),
            //   })
            //   .ToArray();

            //structGenerators = structContainer
            //   .GroupBy(x => x.Namespace)
            //   .Select(x => new StructGenerator
            //   {
            //       Namespace = namespaceRoot + ".DynamicObjectSegments" + ((x.Key != null) ? ("." + x.Key) : ""),
            //       Types = x.ToArray(),
            //   })
            //   .ToArray();



            //genericTypes = genericTypeContainer.Distinct().OrderBy(x => x).ToArray();
        }


        void CollectObjectSegment(INamedTypeSymbol type, bool fromNullable, bool asKey)
        {
            //if (type == null)
            //{
            //    return;
            //}
            //if (!alreadyCollected.Add(Tuple.Create(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), fromNullable, asKey)))
            //{
            //    return;
            //}
            //if (KnownFormatterSpec.IsPrimitive(type))
            //{
            //    return;
            //}
            //if (type.GetAttributes().FindAttributeShortName(UnionAttributeShortName) != null)
            //{
            //    return;
            //}

            //if (type.GetAttributes().FindAttributeShortName(DynamicUnionAttributeShortName) != null)
            //{
            //    return;
            //}
            //if (type.TypeKind == TypeKind.Enum)
            //{
            //    CollectEnum(type, fromNullable, asKey);
            //    return;
            //}
            //if (allowCustomTypes.Contains(type.ToDisplayString()))
            //{
            //    return;
            //}

            //if (type.IsGenericType)
            //{
            //    var genericType = type.ConstructUnboundGenericType();
            //    var genericTypeString = genericType.ToDisplayString();

            //    if (genericTypeString == "T?")
            //    {
            //        CollectObjectSegment(type.TypeArguments[0] as INamedTypeSymbol, true, asKey);
            //        return;
            //    }
            //    else if (allowCustomTypes.Contains(genericTypeString))
            //    {
            //        foreach (var t in type.TypeArguments)
            //        {
            //            CollectObjectSegment(t as INamedTypeSymbol, fromNullable, asKey);
            //        }
            //        return;
            //    }
            //    else if (genericTypeString == "System.Collections.Generic.IList<>"
            //          || genericTypeString == "System.Collections.Generic.IDictionary<,>"
            //          || genericTypeString == "System.Collections.Generic.Dictionary<,>"
            //          || genericTypeString == "ZeroFormatter.ILazyDictionary<,>"
            //          || genericTypeString == "System.Collections.Generic.IReadOnlyList<>"
            //          || genericTypeString == "System.Collections.Generic.ICollection<>"
            //          || genericTypeString == "System.Collections.Generic.IEnumerable<>"
            //          || genericTypeString == "System.Collections.Generic.ISet<>"
            //          || genericTypeString == "ZeroFormatter.ILazyReadOnlyDictionary<,>"
            //          || genericTypeString == "System.Collections.ObjectModel.ReadOnlyCollection<>"
            //          || genericTypeString == "System.Linq.ILookup<,>"
            //          || genericTypeString == "ZeroFormatter.ILazyLookup<,>"
            //          || genericTypeString.StartsWith("System.Collections.Generic.KeyValuePair")
            //          || genericTypeString.StartsWith("ZeroFormatter.KeyTuple"))
            //    {
            //        var elementTypes = string.Join(", ", type.TypeArguments.Select(x => x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
            //        var isDictionaryKey = false;

            //        if (genericTypeString == "System.Collections.Generic.IList<>")
            //        {
            //            genericTypeContainer.Add(new GenericType { TypeKind = GenericTypeKind.List, ElementTypes = elementTypes });
            //        }
            //        else if (genericTypeString == "System.Collections.Generic.IReadOnlyList<>")
            //        {
            //            genericTypeContainer.Add(new GenericType { TypeKind = GenericTypeKind.ReadOnlyList, ElementTypes = elementTypes });
            //        }
            //        else if (genericTypeString == "System.Collections.Generic.IDictionary<,>" || genericTypeString == "System.Collections.Generic.Dictionary<,>")
            //        {
            //            isDictionaryKey = true;
            //            genericTypeContainer.Add(new GenericType { TypeKind = GenericTypeKind.Dictionary, ElementTypes = elementTypes });
            //        }
            //        else if (genericTypeString == "ZeroFormatter.ILazyDictionary<,>")
            //        {
            //            isDictionaryKey = true;
            //            genericTypeContainer.Add(new GenericType { TypeKind = GenericTypeKind.LazyDictionary, ElementTypes = elementTypes });
            //        }
            //        else if (genericTypeString == "ZeroFormatter.ILazyReadOnlyDictionary<,>")
            //        {
            //            isDictionaryKey = true;
            //            genericTypeContainer.Add(new GenericType { TypeKind = GenericTypeKind.LazyReadOnlyDictionary, ElementTypes = elementTypes });
            //        }
            //        else if (genericTypeString == "ZeroFormatter.ILazyLookup<,>")
            //        {
            //            isDictionaryKey = true;
            //            genericTypeContainer.Add(new GenericType { TypeKind = GenericTypeKind.LazyLookup, ElementTypes = elementTypes });
            //        }
            //        else if (genericTypeString == "System.Linq.ILookup<,>")
            //        {
            //            isDictionaryKey = true;
            //            genericTypeContainer.Add(new GenericType { TypeKind = GenericTypeKind.Lookup, ElementTypes = elementTypes });
            //        }
            //        else if (genericTypeString.StartsWith("ZeroFormatter.KeyTuple"))
            //        {
            //            genericTypeContainer.Add(new GenericType { TypeKind = GenericTypeKind.KeyTuple, ElementTypes = elementTypes });
            //        }
            //        else if (genericTypeString.StartsWith("System.Collections.Generic.KeyValuePair"))
            //        {
            //            genericTypeContainer.Add(new GenericType { TypeKind = GenericTypeKind.KeyValuePair, ElementTypes = elementTypes });
            //        }

            //        else if (genericTypeString.StartsWith("System.Collections.Generic.ICollection<>"))
            //        {
            //            genericTypeContainer.Add(new GenericType { TypeKind = GenericTypeKind.InterfaceCollection, ElementTypes = elementTypes });
            //        }
            //        else if (genericTypeString.StartsWith("System.Collections.Generic.IEnumerable<>"))
            //        {
            //            genericTypeContainer.Add(new GenericType { TypeKind = GenericTypeKind.Enumerable, ElementTypes = elementTypes });
            //        }
            //        else if (genericTypeString.StartsWith("System.Collections.ObjectModel.ReadOnlyCollection<>"))
            //        {
            //            genericTypeContainer.Add(new GenericType { TypeKind = GenericTypeKind.ReadOnlyCollection, ElementTypes = elementTypes });
            //        }

            //        var argIndex = 0;
            //        foreach (var t in type.TypeArguments)
            //        {
            //            if (isDictionaryKey && argIndex == 0)
            //            {
            //                CollectObjectSegment(t as INamedTypeSymbol, fromNullable, true);
            //            }
            //            else
            //            {
            //                CollectObjectSegment(t as INamedTypeSymbol, fromNullable, asKey);
            //            }
            //            argIndex++;
            //        }
            //        return;
            //    }
            //    else if (type.AllInterfaces.Any(x => (x.IsGenericType ? x.ConstructUnboundGenericType().ToDisplayString() : "") == "System.Collections.Generic.ICollection<>"))
            //    {
            //        var elementTypes = string.Join(", ", type.TypeArguments.Select(x => x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))) + ", " + type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            //        genericTypeContainer.Add(new GenericType { TypeKind = GenericTypeKind.Collection, ElementTypes = elementTypes });

            //        foreach (var t in type.TypeArguments)
            //        {
            //            CollectObjectSegment(t as INamedTypeSymbol, fromNullable, asKey);
            //        }
            //        return;
            //    }
            //}

            //if (disallowInMetadata && type.Locations[0].IsInMetadata)
            //{
            //    return;
            //}

            //if (type.GetAttributes().FindAttributeShortName(ZeroFormattableAttributeShortName) == null)
            //{
            //    throw new Exception($"Type must be marked with ZeroFormattableAttribute. {type.Name}.  Location:{type.Locations[0]}");
            //}

            //if (!type.IsValueType)
            //{
            //    if (!type.Constructors.Any(x => x.Parameters.Length == 0))
            //    {
            //        throw new Exception($"Type must needs parameterless constructor. {type.Name}. Location:{type.Locations[0]}");
            //    }
            //}
            //else
            //{
            //    var indexes = new List<Tuple<int, IPropertySymbol>>();
            //    foreach (var item in type.GetMembers().OfType<IPropertySymbol>())
            //    {
            //        if (item.IsStatic) continue;
            //        if (item.ExplicitInterfaceImplementations.Length != 0) continue;

            //        var indexAttr = item.GetAttributes().FindAttributeShortName(IndexAttributeShortName);
            //        if (indexAttr != null)
            //        {
            //            var index = (int)indexAttr.ConstructorArguments[0].Value;
            //            indexes.Add(Tuple.Create(index, item));
            //        }
            //    }
            //    indexes = indexes.OrderBy(x => x.Item1).ToList();

            //    var expected = 0;
            //    foreach (var item in indexes)
            //    {
            //        if (item.Item1 != expected)
            //        {
            //            throw new Exception($"Struct index must be started with 0 and be sequential. Type: {type.Name}, InvalidIndex: {item.Item1}");
            //        }
            //        expected++;
            //    }

            //    var foundConstructor = false;
            //    var ctors = (type as INamedTypeSymbol)?.Constructors;
            //    foreach (var ctor in ctors)
            //    {
            //        var isMatch = indexes.Select(x => x.Item2.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
            //            .SequenceEqual(ctor.Parameters.Select(x => x.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
            //        if (isMatch)
            //        {
            //            foundConstructor = true;
            //        }
            //    }
            //    if (!foundConstructor && indexes.Count != 0)
            //    {
            //        throw new Exception($"Struct needs full parameter constructor of index property types. Type: {type.Name}");
            //    }
            //}

            //var list = new List<ObjectSegmentType.PropertyTuple>();

            //var definedIndexes = new HashSet<int>();

            //var distinctName = new HashSet<string>();
            //foreach (var property in type.GetAllMembers())
            //{
            //    if (!distinctName.Add(property.Name)) continue;


            //    if (property.IsStatic) continue;

            //    var propSymbol = property as IPropertySymbol;
            //    var fieldSymbol = property as IFieldSymbol;

            //    if ((propSymbol == null && fieldSymbol == null))
            //    {
            //        continue;
            //    }

            //    if (property.DeclaredAccessibility != Accessibility.Public)
            //    {
            //        continue;
            //    }

            //    if (propSymbol != null && propSymbol.ExplicitInterfaceImplementations.Length != 0)
            //    {
            //        continue;
            //    }

            //    var attributes = property.GetAttributes();
            //    if (attributes.FindAttributeShortName(IgnoreAttributeShortName) != null)
            //    {
            //        continue;
            //    }

            //    if (!type.IsValueType)
            //    {
            //        if (!property.IsVirtual)
            //        {
            //            if (property.IsOverride && !property.IsSealed)
            //            {
            //                // okay, it is override property.
            //            }
            //            else
            //            {
            //                throw new Exception($"Public property's accessor must be virtual. {type.Name}.{property.Name}. Location:{type.Locations[0]}");
            //            }
            //        }
            //    }

            //    if (propSymbol != null && propSymbol.FindAttributeIncludeBasePropertyShortName(UnionKeyAttributeShortName) != null)
            //    {
            //        continue;
            //    }

            //    var indexAttr = attributes.FindAttributeShortName(IndexAttributeShortName);
            //    if (indexAttr == null || indexAttr.ConstructorArguments.Length == 0)
            //    {
            //        throw new Exception($"Public property must be marked with IndexAttribute or IgnoreFormatAttribute. {type.Name}.{property.Name}. Location:{type.Locations[0]}");
            //    }

            //    var index = indexAttr.ConstructorArguments[0];
            //    if (index.IsNull)
            //    {
            //        continue; // null is normal compiler error.
            //    }

            //    if (!definedIndexes.Add((int)index.Value))
            //    {
            //        throw new Exception($"IndexAttribute is not allow duplicate number. {type.Name}.{property.Name}, Index:{index.Value} Location:{type.Locations[0]}");
            //    }

            //    if (!type.IsValueType)
            //    {
            //        if (propSymbol == null)
            //        {
            //            throw new Exception($"Class does not allow that field marks IndexAttribute. {type.Name }{property.Name}");
            //        }

            //        if (propSymbol.GetMethod == null || propSymbol.SetMethod == null
            //         || propSymbol.GetMethod.DeclaredAccessibility == Accessibility.Private
            //         || propSymbol.SetMethod.DeclaredAccessibility == Accessibility.Private)
            //        {
            //            throw new Exception($"Public property must needs both public/protected get and set accessor. {type.Name}.{property.Name}. Location:{type.Locations[0]}");
            //        }
            //    }

            //    var memberType = (propSymbol != null) ? propSymbol.Type : fieldSymbol.Type;

            //    if (memberType.TypeKind == TypeKind.Array)
            //    {
            //        var array = memberType as IArrayTypeSymbol;
            //        while (array != null)
            //        {
            //            var t = array.ElementType;
            //            if (!KnownFormatterSpec.AllowArrayType(t))
            //            {
            //                genericTypeContainer.Add(new GenericType { TypeKind = GenericTypeKind.Array, ElementTypes = t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) });
            //            }

            //            var namedType = memberType as INamedTypeSymbol;
            //            if (namedType != null) // if <T> is unnamed type, it can't analyze.
            //            {
            //                // Recursive
            //                CollectObjectSegment(namedType, fromNullable, asKey);
            //                break;
            //            }
            //            array = t as IArrayTypeSymbol;
            //        }
            //    }
            //    else
            //    {
            //        var namedType = memberType as INamedTypeSymbol;
            //        if (namedType != null) // if <T> is unnamed type, it can't analyze.
            //        {
            //            // Recursive
            //            CollectObjectSegment(namedType, fromNullable, asKey);
            //        }
            //    }

            //    var length = KnownFormatterSpec.GetLength(memberType);
            //    var prop = new ObjectSegmentType.PropertyTuple
            //    {
            //        Name = property.Name,
            //        Type = memberType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            //        Index = (int)index.Value,
            //        IsGetProtected = (propSymbol != null) ? propSymbol.GetMethod.DeclaredAccessibility == Accessibility.Protected : false,
            //        IsSetProtected = (propSymbol != null) ? propSymbol.SetMethod.DeclaredAccessibility == Accessibility.Protected : false,
            //        FixedSize = length ?? 0,
            //        IsProperty = propSymbol != null,
            //        IsCacheSegment = !KnownFormatterSpec.IsLazySegment(memberType),
            //        IsFixedSize = (length != null)
            //    };

            //    list.Add(prop);
            //}

            //var segment = new ObjectSegmentType
            //{
            //    Name = type.ToDisplayString(typeNameFormat).Replace(".", "_"),
            //    FullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            //    Namespace = type.ContainingNamespace.IsGlobalNamespace ? null : type.ContainingNamespace.ToDisplayString(),
            //    LastIndex = list.Select(x => x.Index).DefaultIfEmpty(0).Max(),
            //    Properties = list.OrderBy(x => x.Index).ToArray(),
            //};

            //if (type.IsValueType)
            //{
            //    structContainer.Add(segment);
            //}
            //else
            //{
            //    objectContainer.Add(segment);
            //}
        }
    }
}



