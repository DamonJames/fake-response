using System.Net;

namespace FakeResponse;

public sealed record FakeResponseConfiguration<TFakeContent>(
    string Value,
    string? Path,
    HttpStatusCode StatusCode,
    TFakeContent? Content) : IFakeResponseConfiguration;

public sealed record FakeResponseConfiguration(
    string Value,
    string? Path,
    HttpStatusCode StatusCode) : IFakeResponseConfiguration;

internal interface IFakeResponseConfiguration
{
    string Value { get; }

    string? Path { get; }

    HttpStatusCode StatusCode { get; }
}