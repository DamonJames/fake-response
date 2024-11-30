using System.Net;

namespace FakeResponse;

public sealed class FakeResponseConfigurationBuilder
{
    private (string, string) _header = (string.Empty, string.Empty);
    private string _path = string.Empty;
    private HttpStatusCode _statusCode = HttpStatusCode.OK;
    private string _content = string.Empty;

    public FakeResponseConfigurationBuilder ForHeader(string name, string value)
    {
        _header = (name, value);
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

    public FakeResponseConfigurationBuilder ReturnContent(string content)
    {
        _content = content;
        return this;
    }

    public FakeResponseConfiguration Build() =>
        new(_header, _path, _statusCode, _content);
}