using System.Net;

namespace FakeResponse;

public sealed record FakeResponseConfiguration(
    (string name, string value) Header,
    string Path,
    HttpStatusCode StatusCode,
    string Content);