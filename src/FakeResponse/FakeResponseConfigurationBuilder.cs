using System.Net;

namespace FakeResponse;

public sealed class FakeResponseConfigurationBuilder<TFakeContent>
{
    private string _value = string.Empty;
    private string? _path;
    private HttpStatusCode _statusCode = HttpStatusCode.OK;
    private TFakeContent? _content;

    public FakeResponseConfigurationBuilder<TFakeContent> ForHeaderValue(string value)
    {
        _value = value;
        return this;
    }

    public FakeResponseConfigurationBuilder<TFakeContent> ForPath(string path)
    {
        _path = path;
        return this;
    }

    public FakeResponseConfigurationBuilder<TFakeContent> ReturnStatus(HttpStatusCode statusCode)
    {
        _statusCode = statusCode;
        return this;
    }

    public FakeResponseConfigurationBuilder<TFakeContent> ReturnContent(TFakeContent content)
    {
        _content = content;
        return this;
    }

    public FakeResponseConfiguration<TFakeContent> Build() =>
        new(_value, _path, _statusCode, _content);
}

public sealed class FakeResponseConfigurationBuilder
{
    private string _value = string.Empty;
    private string? _path;
    private HttpStatusCode _statusCode = HttpStatusCode.OK;

    public FakeResponseConfigurationBuilder ForHeaderValue(string value)
    {
        _value = value;
        return this;
    }

    public FakeResponseConfigurationBuilder ForPath(string path)
    {
        _path = path;
        return this;
    }

    public FakeResponseConfigurationBuilder ReturnStatus(HttpStatusCode statusCode)
    {
        _statusCode = statusCode;
        return this;
    }

    public FakeResponseConfiguration Build() =>
        new(_value, _path, _statusCode);
}