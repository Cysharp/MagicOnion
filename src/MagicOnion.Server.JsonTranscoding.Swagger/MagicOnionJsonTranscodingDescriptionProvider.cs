using System.Reflection;
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
                            ModelMetadata = new MagicOnionJsonRequestResponseModelMetadata(ModelMetadataIdentity.ForType(metadata.ResponseType), metadata.Method.Metadata.Parameters),
                            StatusCode = 200,
                        },
                        new ApiResponseType()
                        {
                            ApiResponseFormats =
                            {
                                new ApiResponseFormat() { MediaType = "application/json" },
                            },
                            ModelMetadata = new MagicOnionJsonRequestResponseModelMetadata(ModelMetadataIdentity.ForType(new { Code = default(int), Detail = default(string) }.GetType()), metadata.Method.Metadata.Parameters),
                            StatusCode = 500,
                        }
                    },
                    RelativePath = metadata.RoutePath,
                    ParameterDescriptions =
                    {
                        new ApiParameterDescription() { Source = BindingSource.Body, ModelMetadata = new MagicOnionJsonRequestResponseModelMetadata(ModelMetadataIdentity.ForType(metadata.RequestType), metadata.Method.Metadata.Parameters)},
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

internal class MagicOnionJsonRequestResponseModelMetadata : ModelMetadata
{
    public IReadOnlyList<ParameterInfo> Parameters { get; }

    public MagicOnionJsonRequestResponseModelMetadata(ModelMetadataIdentity identity, IReadOnlyList<ParameterInfo> parameters) : base(identity)
    {
        Parameters = parameters;
        IsBindingAllowed = true;
        Properties = new ModelPropertyCollection(Array.Empty<ModelMetadata>());
        ModelBindingMessageProvider = new DefaultModelBindingMessageProvider();
        AdditionalValues = new Dictionary<object, object>();
    }

    public override IReadOnlyDictionary<object, object> AdditionalValues { get; }
    public override ModelPropertyCollection Properties { get; }
    public override string? BinderModelName { get; }
    public override Type? BinderType { get; }
    public override BindingSource? BindingSource { get; }
    public override bool ConvertEmptyStringToNull { get; }
    public override string? DataTypeName { get; }
    public override string? Description { get; }
    public override string? DisplayFormatString { get; }
    public override string? DisplayName { get; }
    public override string? EditFormatString { get; }
    public override ModelMetadata? ElementMetadata { get; }
    public override IEnumerable<KeyValuePair<EnumGroupAndName, string>>? EnumGroupedDisplayNamesAndValues { get; }
    public override IReadOnlyDictionary<string, string>? EnumNamesAndValues { get; }
    public override bool HasNonDefaultEditFormat { get; }
    public override bool HtmlEncode { get; }
    public override bool HideSurroundingHtml { get; }
    public override bool IsBindingAllowed { get; }
    public override bool IsBindingRequired { get; }
    public override bool IsEnum { get; }
    public override bool IsFlagsEnum { get; }
    public override bool IsReadOnly { get; }
    public override bool IsRequired { get; }
    public override ModelBindingMessageProvider ModelBindingMessageProvider { get; }
    public override int Order { get; }
    public override string? Placeholder { get; }
    public override string? NullDisplayText { get; }
    public override IPropertyFilterProvider? PropertyFilterProvider { get; }
    public override bool ShowForDisplay { get; }
    public override bool ShowForEdit { get; }
    public override string? SimpleDisplayProperty { get; }
    public override string? TemplateHint { get; }
    public override bool ValidateChildren { get; }
    public override IReadOnlyList<object> ValidatorMetadata { get; } = Array.Empty<object>();
    public override Func<object, object?>? PropertyGetter { get; }
    public override Action<object, object?>? PropertySetter { get; }
}
