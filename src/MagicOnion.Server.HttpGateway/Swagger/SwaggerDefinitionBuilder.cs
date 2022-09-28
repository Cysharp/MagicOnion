using MagicOnion.Server.HttpGateway.Swagger.Schemas;
using MagicOnion.Server;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MagicOnion.Server.HttpGateway.Swagger;

public class SwaggerDefinitionBuilder
{
    readonly SwaggerOptions options;
    readonly HttpContext httpContext;
    readonly IReadOnlyList<MethodHandler> handlers;

    ILookup<Tuple<string, string>, XmlCommentStructure> xDocLookup;

    public SwaggerDefinitionBuilder(SwaggerOptions options, HttpContext httpContext, IEnumerable<MethodHandler> handlers)
    {
        this.options = options;
        this.httpContext = httpContext;
        this.handlers = handlers.Where(x => x.MethodType == Grpc.Core.MethodType.Unary).ToArray();
    }

    public byte[] BuildSwaggerJson()
    {
        try
        {
            if (options.XmlDocumentPath != null && !File.Exists(options.XmlDocumentPath))
            {
                xDocLookup = null;
            }
            else
            {
                xDocLookup = (options.XmlDocumentPath != null)
                    ? BuildXmlMemberCommentStructure(options.XmlDocumentPath)
                    : null;
            }

            var doc = new SwaggerDocument();
            doc.info = options.Info;
            doc.host = (options.CustomHost != null) ? options.CustomHost(httpContext) : httpContext.Request.Headers["Host"][0];
            doc.basePath = options.ApiBasePath;
            doc.schemes = (options.ForceSchemas.Length == 0) ? new[] { httpContext.Request.IsHttps ? "https" : httpContext.Request.Scheme } : options.ForceSchemas;
            doc.paths = new Dictionary<string, PathItem>();
            doc.definitions = new Dictionary<string, Schema>();

            // tags.
            var xmlServiceName = (xDocLookup != null)
                ? BuildXmlTypeSummary(options.XmlDocumentPath)
                : null;

            doc.tags = handlers
                .Select(x => x.ServiceName)
                .Distinct()
                .Select(x =>
                {
                    string desc = null;
                    if (xmlServiceName != null)
                    {
                        xmlServiceName.TryGetValue(x, out desc);
                    }
                    return new Tag()
                    {
                        name = x,
                        description = desc
                    };
                })
                .ToArray();

            // Unary only
            foreach (var item in handlers)
            {
                XmlCommentStructure xmlComment = null;
                if (xDocLookup != null)
                {
                    xmlComment = xDocLookup[Tuple.Create(item.ServiceName, item.MethodInfo.Name)].FirstOrDefault();
                }

                var parameters = BuildParameters(doc.definitions, xmlComment, item.MethodInfo);
                var operation = new Operation
                {
                    tags = new[] { item.ServiceName },
                    summary = (xmlComment != null) ? xmlComment.Summary : "",
                    description = (xmlComment != null) ? xmlComment.Remarks : "",
                    parameters = parameters,
                    responses = new Dictionary<string, Response>
                    {
                        {"default", new Response { description = "done operation"} },
                    }
                };

                doc.paths.Add("/" + item.ToString(), new PathItem { post = operation }); // everything post.
            }

            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms, new UTF8Encoding(false)))
            {
                var serializer = new JsonSerializer()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = IgnoreEmptyEnumerablesResolver.Instance // omit empty collection.
                };
                serializer.Serialize(sw, doc);

                sw.Flush();
                return ms.ToArray();
            }
        }
        catch (Exception ex)
        {
            return Encoding.UTF8.GetBytes(ex.ToString());
        }
    }

    Schemas.Parameter[] BuildParameters(IDictionary<string, Schema> definitions, XmlCommentStructure xmlComment, MethodInfo method)
    {
        var parameterInfos = method.GetParameters();
        var parameters = parameterInfos
            .Select(x =>
            {
                var parameterXmlComment = UnwrapTypeName(x.ParameterType);
                if (xmlComment != null)
                {
                    xmlComment.Parameters.TryGetValue(x.Name, out parameterXmlComment);
                    parameterXmlComment = UnwrapTypeName(x.ParameterType) + " " + parameterXmlComment;
                }

                var defaultValue = x.DefaultValue;
                if (defaultValue != null && x.ParameterType.GetTypeInfo().IsEnum)
                {
                    defaultValue = defaultValue.ToString();
                }

                var collectionType = GetCollectionType(x.ParameterType);
                var items = collectionType != null
                    ? new PartialSchema { type = ToSwaggerDataType(collectionType) }
                    : null;

                string defaultObjectExample = null;
                object[] enums = null;
                if (x.ParameterType.GetTypeInfo().IsEnum || (collectionType != null && collectionType.GetTypeInfo().IsEnum))
                {
                    var enumType = (x.ParameterType.GetTypeInfo().IsEnum) ? x.ParameterType : collectionType;

                    var enumValues = Enum.GetNames(enumType);

                    if (collectionType != null)
                    {
                        // Current Swagger-UI's enum array selector is too buggy...
                        //items.@enum = enumValues;
                        defaultObjectExample = string.Join("\r\n", Enum.GetNames(collectionType));
                    }
                    else
                    {
                        enums = enumValues;
                    }
                }

                var swaggerDataType = ToSwaggerDataType(x.ParameterType);
                Schema refSchema = null;
                if (swaggerDataType == "object")
                {
                    BuildSchema(definitions, x.ParameterType);
                    refSchema = new Schema { @ref = BuildSchema(definitions, x.ParameterType) };
                    var unknownObj = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(x.ParameterType);
                    defaultObjectExample = JsonConvert.SerializeObject(unknownObj, new[] { new Newtonsoft.Json.Converters.StringEnumConverter() });
                    swaggerDataType = "string"; // object can not attach formData.
                }

                return new Schemas.Parameter
                {
                    name = x.Name,
                    @in = "formData",
                    type = swaggerDataType,
                    description = parameterXmlComment,
                    required = !x.IsOptional && !x.ParameterType.IsNullable(), // OpenAPI 3 has a separate definition for nullable, but for OpenAPI 2, there is only required.
                    @default = defaultObjectExample ?? ((x.IsOptional) ? defaultValue : null),
                    items = items,
                    @enum = enums,
                    collectionFormat = "multi", // csv or multi
                    schema = refSchema
                };
            })
            .ToArray();

        return parameters;
    }

    string BuildSchema(IDictionary<string, Schema> definitions, Type type)
    {
        var fullName = type.FullName;
        if (fullName == null) return ""; // safety(TODO:IDictionary<> is not supported)

        Schema schema;
        if (definitions.TryGetValue(fullName, out schema)) return "#/definitions/" + fullName;

        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);

        var props = properties.Cast<MemberInfo>().Concat(fields)
            .OrderBy(x => x.Name)
            .Select(x =>
            {
                var memberType = GetMemberType(x);
                var swaggerDataType = ToSwaggerDataType(memberType);

                if (swaggerDataType == "object")
                {
                    return new
                    {
                        Name = x.Name,
                        Schema = new Schema
                        {
                            @ref = BuildSchema(definitions, memberType)
                        }
                    };
                }
                else
                {
                    Schema items = null;
                    if (swaggerDataType == "array")
                    {
                        var collectionType = GetCollectionType(memberType);
                        var dataType = ToSwaggerDataType(collectionType);
                        if (dataType == "object")
                        {
                            items = new Schema
                            {
                                @ref = BuildSchema(definitions, collectionType)
                            };
                        }
                        else
                        {
                            if (collectionType.GetTypeInfo().IsEnum)
                            {
                                items = new Schema
                                {
                                    type = "string",
                                    @enum = Enum.GetNames(collectionType)
                                };
                            }
                            else
                            {
                                items = new Schema { type = ToSwaggerDataType(collectionType) };
                            }
                        }
                    }

                    IList<object> schemaEnum = null;
                    if (memberType.GetTypeInfo().IsEnum)
                    {
                        schemaEnum = Enum.GetNames(memberType);
                    }

                    return new
                    {
                        Name = x.Name,
                        Schema = new Schema
                        {
                            type = swaggerDataType,
                            description = UnwrapTypeName(memberType),
                            @enum = schemaEnum,
                            items = items
                        }
                    };
                }
            })
            .ToDictionary(x => x.Name, x => x.Schema);

        schema = new Schema
        {
            type = "object",
            properties = props,
        };

        definitions.Add(fullName, schema);
        return "#/definitions/" + fullName;
    }

    static Type GetMemberType(MemberInfo memberInfo)
    {
        var f = memberInfo as FieldInfo;
        if (f != null) return f.FieldType;
        var p = memberInfo as PropertyInfo;
        if (p != null) return p.PropertyType;
        throw new Exception();
    }

    static Type GetCollectionType(Type type)
    {
        if (type.IsArray) return type.GetElementType();

        if (type.GetTypeInfo().IsGenericType)
        {
            var genTypeDef = type.GetGenericTypeDefinition();
            if (genTypeDef == typeof(IEnumerable<>)
                || genTypeDef == typeof(ICollection<>)
                || genTypeDef == typeof(IList<>)
                || genTypeDef == typeof(List<>)
                || genTypeDef == typeof(IReadOnlyCollection<>)
                || genTypeDef == typeof(IReadOnlyList<>))
            {
                return genTypeDef.GetGenericArguments()[0];
            }
        }

        return null; // not collection
    }

    static ILookup<Tuple<string, string>, XmlCommentStructure> BuildXmlMemberCommentStructure(string xmlDocumentPath)
    {
        var file = File.ReadAllText(xmlDocumentPath);
        var xDoc = XDocument.Parse(file);
        var xDocLookup = xDoc.Descendants("member")
            .Where(x => x.Attribute("name").Value.StartsWith("M:"))
            .Select(x =>
            {
                var match = Regex.Match(x.Attribute("name").Value, @"(\w+)\.(\w+)?(\(.+\)|$)");

                var summary = ((string)x.Element("summary")) ?? "";
                var returns = ((string)x.Element("returns")) ?? "";
                var remarks = ((string)x.Element("remarks")) ?? "";
                var parameters = x.Elements("param")
                    .Select(e => Tuple.Create(e.Attribute("name").Value, e))
                    .Distinct(new Item1EqualityCompaerer<string, XElement>())
                    .ToDictionary(e => e.Item1, e => e.Item2.Value.Trim());

                return new XmlCommentStructure
                {
                    ClassName = match.Groups[1].Value,
                    MethodName = match.Groups[2].Value,
                    Summary = summary.Trim(),
                    Remarks = remarks.Trim(),
                    Parameters = parameters,
                    Returns = returns.Trim()
                };
            })
            .ToLookup(x => Tuple.Create(x.ClassName, x.MethodName));

        return xDocLookup;
    }

    static IDictionary<string, string> BuildXmlTypeSummary(string xmlDocumentPath)
    {
        var file = File.ReadAllText(xmlDocumentPath);
        var xDoc = XDocument.Parse(file);
        var xDocLookup = xDoc.Descendants("member")
            .Where(x => x.Attribute("name").Value.StartsWith("T:"))
            .Select(x =>
            {
                var match = Regex.Match(x.Attribute("name").Value, @"(\w+)\.(\w+)?(\(.+\)|$)");

                var summary = ((string)x.Element("summary")) ?? "";
                return new { name = match.Groups[2].Value, summary = summary.Trim() };
            })
            .ToDictionary(x => x.name, x => x.summary);

        return xDocLookup;
    }

    static string ToSwaggerDataType(Type type)
    {
        if (GetCollectionType(type) != null)
        {
            return "array";
        }

        if (type.IsNullable())
        {
            type = Nullable.GetUnderlyingType(type);
        }

        if (type.GetTypeInfo().IsEnum || type == typeof(DateTime) || type == typeof(DateTimeOffset))
        {
            return "string";
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean:
                return "boolean";
            case TypeCode.Decimal:
            case TypeCode.Single:
            case TypeCode.Double:
                return "number";
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
                return "integer";
            case TypeCode.Char:
            case TypeCode.String:
                return "string";
            default:
                return "object";
        }
    }

    static string UnwrapTypeName(Type t)
    {
        if (t == typeof(void)) return "void";
        if (!t.GetTypeInfo().IsGenericType) return t.Name;

        var innerFormat = string.Join(", ", t.GetGenericArguments().Select(x => UnwrapTypeName(x)));
        return Regex.Replace(t.GetGenericTypeDefinition().Name, @"`.+$", "") + "&lt;" + innerFormat + "&gt;";
    }

    class Item1EqualityCompaerer<T1, T2> : EqualityComparer<Tuple<T1, T2>>
    {
        public override bool Equals(Tuple<T1, T2> x, Tuple<T1, T2> y)
        {
            return x.Item1.Equals(y.Item1);
        }

        public override int GetHashCode(Tuple<T1, T2> obj)
        {
            return obj.Item1.GetHashCode();
        }
    }

    class XmlCommentStructure
    {
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public string Summary { get; set; }
        public string Remarks { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public string Returns { get; set; }
    }
}

// http://stackoverflow.com/questions/34903151/how-to-omit-empty-collections-when-serializing-with-json-net
public class IgnoreEmptyEnumerablesResolver : DefaultContractResolver
{
    public static readonly IgnoreEmptyEnumerablesResolver Instance = new IgnoreEmptyEnumerablesResolver();

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (property.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
        {
            property.ShouldSerialize = instance =>
            {
                IEnumerable enumerable = null;

                // this value could be in a public field or public property
                switch (member.MemberType)
                {
                    case MemberTypes.Property:
                        enumerable = instance
                            .GetType()
                            .GetProperty(member.Name)
                            .GetValue(instance, null) as IEnumerable;
                        break;
                    case MemberTypes.Field:
                        enumerable = instance
                            .GetType()
                            .GetField(member.Name)
                            .GetValue(instance) as IEnumerable;
                        break;
                    default:
                        break;

                }

                if (enumerable != null)
                {
                    // check to see if there is at least one item in the Enumerable
                    return enumerable.GetEnumerator().MoveNext();
                }
                else
                {
                    // if the list is null, we defer the decision to NullValueHandling
                    return true;
                }

            };
        }

        return property;
    }
}