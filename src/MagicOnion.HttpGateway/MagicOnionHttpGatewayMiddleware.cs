using Grpc.Core;
using MagicOnion.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicOnion.HttpGateway
{
    public class MagicOnionHttpGatewayMiddleware
    {
        readonly RequestDelegate next;
        readonly IDictionary<string, MethodHandler> handlers;
        readonly Channel channel;

        public MagicOnionHttpGatewayMiddleware(RequestDelegate next, IReadOnlyList<MethodHandler> handlers, Channel channel)
        {
            this.next = next;
            this.handlers = handlers.ToDictionary(x => "/" + x.ToString());
            this.channel = channel;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                var path = httpContext.Request.Path.Value;

                MethodHandler handler;
                if (!handlers.TryGetValue(path, out handler))
                {
                    await next(httpContext);
                    return;
                }

                // from form...
                object deserializedObject;
                if (httpContext.Request.ContentType == "application/x-www-form-urlencoded")
                {
                    //object parameters
                    var args = new List<object>();
                    var typeArgs = new List<Type>();
                    foreach (var p in handler.MethodInfo.GetParameters())
                    {
                        typeArgs.Add(p.ParameterType);

                        StringValues stringValues;
                        if (httpContext.Request.Form.TryGetValue(p.Name, out stringValues))
                        {
                            if (p.ParameterType == typeof(string))
                            {
                                args.Add((string)stringValues);
                            }
                            else if (p.ParameterType.IsEnum)
                            {
                                args.Add(Enum.Parse(p.ParameterType, (string)stringValues));
                            }
                            else
                            {
                                var collectionType = GetCollectionType(p.ParameterType);
                                if (stringValues.Count == 1 || collectionType == null)
                                {
                                    var values = (string)stringValues;
                                    if (p.ParameterType == typeof(DateTime) || p.ParameterType == typeof(DateTimeOffset))
                                    {
                                        values = "\"" + values + "\"";
                                    }

                                    args.Add(JsonConvert.DeserializeObject(values, p.ParameterType));
                                }
                                else
                                {
                                    string serializeTarget;
                                    if (collectionType == typeof(string))
                                    {
                                        serializeTarget = "[" + string.Join(", ", stringValues.Select(x => JsonConvert.SerializeObject(x))) + "]"; // escape serialzie
                                    }
                                    else if (collectionType.IsEnum || collectionType == typeof(DateTime) || collectionType == typeof(DateTimeOffset))
                                    {
                                        serializeTarget = "[" + string.Join(", ", stringValues.Select(x => "\"" + x + "\"")) + "]";
                                    }
                                    else
                                    {
                                        serializeTarget = "[" + (string)stringValues + "]";
                                    }

                                    args.Add(JsonConvert.DeserializeObject(serializeTarget, p.ParameterType));
                                }
                            }
                        }
                        else
                        {
                            if (p.HasDefaultValue)
                            {
                                args.Add(p.DefaultValue);
                            }
                            else
                            {
                                args.Add(null);
                            }
                        }
                    }

                    deserializedObject = MagicOnionMarshallers.InsantiateDynamicArgumentTuple(typeArgs.ToArray(), args.ToArray());
                }
                else
                {
                    string body;
                    using (var sr = new StreamReader(httpContext.Request.Body, Encoding.UTF8))
                    {
                        body = sr.ReadToEnd();
                    }

                    if (handler.RequestType == typeof(byte[]) && string.IsNullOrWhiteSpace(body))
                    {
                        body = "[]";
                    }
                    deserializedObject = Newtonsoft.Json.JsonConvert.DeserializeObject(body, handler.RequestType);
                }

                // JSON to C# Object to ZeroFormatter
                var requestObject = handler.BoxedSerialize(deserializedObject);

                var method = new Method<byte[], byte[]>(MethodType.Unary, handler.ServiceName, handler.MethodInfo.Name, MagicOnionMarshallers.ByteArrayMarshaller, MagicOnionMarshallers.ByteArrayMarshaller);

                var invoker = new DefaultCallInvoker(channel);
                var rawResponse = await invoker.AsyncUnaryCall(method, null, default(CallOptions), requestObject);

                // ZeroFormatter -> Object -> Json
                var obj = handler.BoxedDeserialize(rawResponse);
                var v = JsonConvert.SerializeObject(obj, new[] { new Newtonsoft.Json.Converters.StringEnumConverter() });
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsync(v);
            }
            catch (Exception ex)
            {
                httpContext.Response.StatusCode = 500;
                await httpContext.Response.WriteAsync(ex.ToString());
            }
        }

        Type GetCollectionType(Type type)
        {
            if (type.IsArray) return type.GetElementType();

            if (type.IsGenericType)
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
    }
}