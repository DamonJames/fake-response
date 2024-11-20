using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Net;
using System.Text.Json;

namespace FakeResponse.Tests;

public sealed class FakeResponseHandlerShould
{
    private readonly DefaultHttpContext _context;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public FakeResponseHandlerShould()
    {
        FakeResponseOptions.Global.HeaderName = "TestHeader";

        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();

        _context = new DefaultHttpContext();

        _httpContextAccessor.HttpContext.Returns(_context);
    }

    [Fact]
    public async Task ReturnFakeResponseWhenHeaderValueMatches()
    {
        _context.Request.Headers["TestHeader"] = "TestValue";

        var configuration = new FakeResponseConfiguration(
            Value: "TestValue",
            Path: null,
            StatusCode: HttpStatusCode.Gone);

        var sut = new FakeResponseHandler(_httpContextAccessor, configuration)
        {
            InnerHandler = new TestHandler()
        };

        var invoker = new HttpMessageInvoker(sut);

        var response = await invoker.SendAsync(new HttpRequestMessage(), default);

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task ReturnFakeResponseWithContentWhenHeaderValueMatches()
    {
        _context.Request.Headers["TestHeader"] = "TestValue";

        var configuration = new FakeResponseConfiguration<TestContent>(
            Value: "TestValue",
            Path: null,
            StatusCode: HttpStatusCode.Gone,
            Content: new TestContent());

        var sut = new FakeResponseHandler<TestContent>(_httpContextAccessor, configuration)
        {
            InnerHandler = new TestHandler()
        };

        var invoker = new HttpMessageInvoker(sut);

        var response = await invoker.SendAsync(new HttpRequestMessage(), default);

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
        var content = await response.Content.ReadAsStringAsync();
        var contentResult = JsonSerializer.Deserialize<TestContent>(content);
        contentResult.Should().NotBeNull().And.BeEquivalentTo(new TestContent());
    }

    [Fact]
    public async Task ReturnFakeResponseWhenHeaderValueMatchesAndPathDefined()
    {
        _context.Request.Headers["TestHeader"] = "TestValue";

        var configuration = new FakeResponseConfiguration(
            Value: "TestValue",
            Path: "/path/for/fakeResponse",
            StatusCode: HttpStatusCode.Gone);

        var sut = new FakeResponseHandler(_httpContextAccessor, configuration)
        {
            InnerHandler = new TestHandler()
        };

        var invoker = new HttpMessageInvoker(sut);

        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri("https://example.com/path/for/fakeResponse")
        };

        var response = await invoker.SendAsync(request, default);

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task ReturnRealResponseWhenHeaderValueMatchesButPathDoesNot()
    {
        _context.Request.Headers["TestHeader"] = "TestValue";

        var configuration = new FakeResponseConfiguration(
            Value: "TestValue",
            Path: "/path/for/fakeResponse",
            StatusCode: HttpStatusCode.Gone);

        var sut = new FakeResponseHandler(_httpContextAccessor, configuration)
        {
            InnerHandler = new TestHandler()
        };

        var invoker = new HttpMessageInvoker(sut);

        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri("https://example.com/path/for/realResponse")
        };

        var response = await invoker.SendAsync(request, default);

        response.StatusCode.Should().Be(HttpStatusCode.UnavailableForLegalReasons);
    }

    [Fact]
    public async Task ReturnRealResponseWhenHeaderValueDoesNotMatch()
    {
        _context.Request.Headers["TestHeader"] = "AnotherValue";

        var configuration = new FakeResponseConfiguration(
            Value: "TestValue",
            Path: null,
            StatusCode: HttpStatusCode.Gone);

        var sut = new FakeResponseHandler(_httpContextAccessor, configuration)
        {
            InnerHandler = new TestHandler()
        };

        var invoker = new HttpMessageInvoker(sut);

        var response = await invoker.SendAsync(new HttpRequestMessage(), default);

        response.StatusCode.Should().Be(HttpStatusCode.UnavailableForLegalReasons);
    }

    [Fact]
    public async Task ReturnRealResponseWhenHeadersAreNull()
    {
        _httpContextAccessor.HttpContext.Returns(null as HttpContext);

        var configuration = new FakeResponseConfiguration(
            Value: "TestValue",
            Path: null,
            StatusCode: HttpStatusCode.Gone);

        var sut = new FakeResponseHandler(_httpContextAccessor, configuration)
        {
            InnerHandler = new TestHandler()
        };

        var invoker = new HttpMessageInvoker(sut);

        var response = await invoker.SendAsync(new HttpRequestMessage(), default);

        response.StatusCode.Should().Be(HttpStatusCode.UnavailableForLegalReasons);
    }
}

public sealed class TestHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(new HttpResponseMessage(HttpStatusCode.UnavailableForLegalReasons));
}

public sealed class TestContent
{
    public string Content { get; } = "This is an example content response";
}