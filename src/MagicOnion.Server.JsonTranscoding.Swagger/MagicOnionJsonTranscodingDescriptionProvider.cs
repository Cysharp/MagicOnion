using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace MagicOnion.Server.JsonTranscoding.Swagger;

internal class MagicOnionJsonTranscodingDescriptionProvider(EndpointDataSource endpointDataSource) : IApiDescriptionProvider
{
    public void OnProvidersExecuting(ApiDescriptionProviderContext context)
    {
        foreach (var endpoint in endpointDataSource.Endpoints)
        {
            if (endpoint.Metadata.SingleOrDefault(x => x is MagicOnionJsonTranscodingMetadata) is MagicOnionJsonTranscodingMetadata metadata)
            {

                context.Results.Add(new ApiDescription()
                {
                    HttpMethod = "POST",
                    ActionDescriptor = new ActionDescriptor()
                    {
                        RouteValues =
                        {
                            ["controller"] = metadata.Method.ServiceName,
                        },
                        EndpointMetadata = [],
                    },
                    SupportedRequestFormats =
                    {
                        new ApiRequestFormat() { MediaType = "application/json" },
                    },
                    SupportedResponseTypes =
                    {
                        new ApiResponseType()
                        {
                            ApiResponseFormats =
                            {
                                new ApiResponseFormat() { MediaType = "application/json" },
                            },
                            ModelMetadata = new MagicOnionJsonRequestResponseModelMetadata(ModelMetadataIdentity.ForType(metadata.ResponseType), metadata.Method.ServiceName, metadata.Method.MethodName, metadata.Method.Metadata.Parameters),
                            StatusCode = 200,
                        },
                        new ApiResponseType()
                        {
                            ApiResponseFormats =
                            {
                                new ApiResponseFormat() { MediaType = "application/json" },
                            },
                            ModelMetadata = new MagicOnionJsonRequestResponseModelMetadata(ModelMetadataIdentity.ForType(typeof(MagicOnionJsonTranscodingErrorResponse)), metadata.Method.ServiceName, metadata.Method.MethodName, metadata.Method.Metadata.Parameters),
                            StatusCode = 500,
                        }
                    },
                    RelativePath = metadata.RoutePath,
                    ParameterDescriptions =
                    {
                        new ApiParameterDescription() { Source = BindingSource.Body, ModelMetadata = new MagicOnionJsonRequestResponseModelMetadata(ModelMetadataIdentity.ForType(metadata.RequestType), metadata.Method.ServiceName, metadata.Method.MethodName, metadata.Method.Metadata.Parameters)},
                    }
                });

            }
        }
    }

    public void OnProvidersExecuted(ApiDescriptionProviderContext context)
    {
    }

    // https://github.com/dotnet/aspnetcore/blob/3f8edf130a14d34024a42ebd678fa3f699ef5916/src/Grpc/JsonTranscoding/src/Microsoft.AspNetCore.Grpc.Swagger/Internal/GrpcJsonTranscodingDescriptionProvider.cs#L32
    // Executes after ASP.NET Core
    public int Order => -900;
}
