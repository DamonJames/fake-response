using FluentAssertions;
using System.Net;

namespace FakeResponse.Tests;

public sealed class FakeResponseConfigurationBuilderShould
{
    [Fact]
    public void ReturnExpectedConfiguration()
    {
        var builder = new FakeResponseConfigurationBuilder();

        builder.ForHeader("name", "value")
            .ForPath("path")
            .ReturnStatus(HttpStatusCode.NotExtended)
            .ReturnContent("content");

        var result = builder.Build();

        result.Header.Should().Be(("name", "value"));
        result.Path.Should().Be("path");
        result.StatusCode.Should().Be(HttpStatusCode.NotExtended);
        result.Content.Should().Be("content");
    }
}