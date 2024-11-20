using FluentAssertions;
using System.Net;

namespace FakeResponse.Tests;

public sealed class FakeResponseConfigurationBuilderShould
{
    [Fact]
    public void ReturnExpectedConfiguration()
    {
        var builder = new FakeResponseConfigurationBuilder();

        builder.ForHeaderValue("TestHeader")
            .ForPath("TestPath")
            .ReturnStatus(HttpStatusCode.NotExtended);

        var result = builder.Build();

        result.Value.Should().Be("TestHeader");
        result.Path.Should().Be("TestPath");
        result.StatusCode.Should().Be(HttpStatusCode.NotExtended);
    }

    [Fact]
    public void ReturnExpectedConfigurationWithoutPath()
    {
        var builder = new FakeResponseConfigurationBuilder();

        builder.ForHeaderValue("TestHeader")
            .ReturnStatus(HttpStatusCode.NotExtended);

        var result = builder.Build();

        result.Value.Should().Be("TestHeader");
        result.Path.Should().Be(null);
        result.StatusCode.Should().Be(HttpStatusCode.NotExtended);
    }

    [Fact]
    public void ReturnExpectedConfigurationWithContent()
    {
        var builder = new FakeResponseConfigurationBuilder<TestClass>();

        var content = new TestClass();

        builder.ForHeaderValue("TestHeader")
            .ReturnContent(content)
            .ReturnStatus(HttpStatusCode.NotExtended);

        var result = builder.Build();

        result.Value.Should().Be("TestHeader");
        result.Content.Should().Be(content);
        result.StatusCode.Should().Be(HttpStatusCode.NotExtended);
    }
}

internal sealed class TestClass { }