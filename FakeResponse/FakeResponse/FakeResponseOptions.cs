namespace FakeResponse;

public static class FakeResponseOptions
{
    public static FakeResponseSettings Global { get; } = new FakeResponseSettings();
}

public sealed class FakeResponseSettings
{
    public bool IsProductionEnvironment { get; set; } = true;

    public string HeaderName { get; set; } = string.Empty;
}