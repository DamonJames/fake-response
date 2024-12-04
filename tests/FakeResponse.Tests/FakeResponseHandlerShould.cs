using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Net;

namespace FakeResponse.Tests;

public sealed class FakeResponseHandlerShould
{
    private readonly DefaultHttpContext _context;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public FakeResponseHandlerShould()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();

        _context = new DefaultHttpContext();

        _httpContextAccessor.HttpContext.Returns(_context);
    }

    [Fact]
    public async Task ReturnFakeResponseWhenHeaderValueMatches()
    {
        _context.Request.Headers["name"] = "value";

        var configuration = new FakeResponseConfiguration(
            Header: ("name", "value"),
            Path: string.Empty,
            StatusCode: HttpStatusCode.Gone,
            Content: "content");

        var sut = new FakeResponseHandler(_httpContextAccessor, configuration)
        {
            InnerHandler = new TestHandler()
        };

        var invoker = new HttpMessageInvoker(sut);

        var response = await invoker.SendAsync(new HttpRequestMessage(), default);

        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
        content.Should().Be("content");
    }

    [Fact]
    public async Task ReturnFakeResponseWhenHeaderValueMatchesAndAbsolutePathDefined()
    {
        _context.Request.Headers["name"] = "value";

        var configuration = new FakeResponseConfiguration(
            Header: ("name", "value"),
            Path: "/path/for/fakeResponse",
            StatusCode: HttpStatusCode.Gone,
            Content: string.Empty);

        var sut = new FakeResponseHandler(_httpContextAccessor, configuration)
        {
            InnerHandler = new TestHandler()
        };

        var invoker = new HttpMessageInvoker(sut);

        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri("https://example.com/path/for/fakeResponse", UriKind.Absolute)
        };

        var response = await invoker.SendAsync(request, default);

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task ReturnFakeResponseWhenHeaderValueMatchesAndRelativePathDefined()
    {
        _context.Request.Headers["name"] = "value";

        var configuration = new FakeResponseConfiguration(
            Header: ("name", "value"),
            Path: "/path/for/fakeResponse",
            StatusCode: HttpStatusCode.Gone,
            Content: string.Empty);

        var sut = new FakeResponseHandler(_httpContextAccessor, configuration)
        {
            InnerHandler = new TestHandler()
        };

        var invoker = new HttpMessageInvoker(sut);

        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri("/path/for/fakeResponse", UriKind.Relative)
        };

        var response = await invoker.SendAsync(request, default);

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task ReturnRealResponseWhenPathProvidedButRequestUriIsNull()
    {
        _context.Request.Headers["name"] = "value";

        var configuration = new FakeResponseConfiguration(
            Header: ("name", "value"),
            Path: "/path/for/fakeResponse",
            StatusCode: HttpStatusCode.Gone,
            Content: string.Empty);

        var sut = new FakeResponseHandler(_httpContextAccessor, configuration)
        {
            InnerHandler = new TestHandler()
        };

        var invoker = new HttpMessageInvoker(sut);

        var request = new HttpRequestMessage()
        {
            RequestUri = null
        };

        var response = await invoker.SendAsync(request, default);

        response.StatusCode.Should().Be(HttpStatusCode.UnavailableForLegalReasons);
    }

    [Fact]
    public async Task ReturnRealResponseWhenHeaderValueMatchesButPathDoesNot()
    {
        _context.Request.Headers["name"] = "value";

        var configuration = new FakeResponseConfiguration(
            Header: ("name", "value"),
            Path: "/path/for/fakeResponse",
            StatusCode: HttpStatusCode.Gone,
            Content: string.Empty);

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
        _context.Request.Headers["name"] = "anotherValue";

        var configuration = new FakeResponseConfiguration(
            Header: ("name", "value"),
            Path: string.Empty,
            StatusCode: HttpStatusCode.Gone,
            Content: string.Empty);

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
            Header: ("name", "value"),
            Path: string.Empty,
            StatusCode: HttpStatusCode.Gone,
            Content: string.Empty);

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