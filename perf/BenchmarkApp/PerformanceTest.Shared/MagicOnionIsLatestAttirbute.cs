namespace PerformanceTest.Shared;

public class MagicOnionIsLatestAttirbute(string isLatest) : Attribute
{
    public bool IsLatest { get; } = string.IsNullOrWhiteSpace(isLatest);
}
