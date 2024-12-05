using System.Net;

namespace FakeResponse;

public sealed record FakeResponseConfiguration(
    (string name, string value) Header,
    string Path,
    Dictionary<string, string> QueryParameters,
    Dictionary<string, Func<string, bool>> DynamicQueryParameters,
    HttpStatusCode StatusCode,
    string Content);