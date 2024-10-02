public enum ScenarioType
{
    All,
    CI, // Run Unary, StreamingHub
    CIFull, // Run Unary, StreamingHub, PingpongStreamingHub

    Unary,
    UnaryComplex,
    UnaryLargePayload1K,
    UnaryLargePayload2K,
    UnaryLargePayload4K,
    UnaryLargePayload8K,
    UnaryLargePayload16K,
    UnaryLargePayload32K,
    UnaryLargePayload64K,

    StreamingHub,
    StreamingHubValueTask,
    StreamingHubComplex,
    StreamingHubComplexValueTask,
    StreamingHubLargePayload1K,
    StreamingHubLargePayload2K,
    StreamingHubLargePayload4K,
    StreamingHubLargePayload8K,
    StreamingHubLargePayload16K,
    StreamingHubLargePayload32K,
    StreamingHubLargePayload64K,

    PingpongStreamingHub,
    PingpongCachedStreamingHub,
}
