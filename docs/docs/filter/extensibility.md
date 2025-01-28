# Filter extensibility

:::info
This feature and document is applies to the server-side only.
:::

The following interfaces are provided for filter extensions. These interfaces are similar to ASP.NET Core MVC filter mechanism.

- `IMagicOnionFilterFactory<T>`
- `IMagicOnionOrderedFilter`
- `IMagicOnionServiceFilter`
- `IStreamingHubFilter`

`MagicOnionFilterAttributes` and `StreamingHubFilterAttribute` implement these interfaces for easy use. You can use these interfaces for more flexible implementation.
