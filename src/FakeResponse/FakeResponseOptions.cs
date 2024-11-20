namespace FakeResponse;

public static class FakeResponseOptions
{
    public static Global Global { get; } = new Global();
}

public sealed class Global
{
    public bool IsProductionEnvironment { get; set; } = true;

    public string HeaderName { get; set; } = string.Empty;
}