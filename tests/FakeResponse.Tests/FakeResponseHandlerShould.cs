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

    [Theory]
    [InlineData("https://example.com/path", UriKind.Absolute)]
    [InlineData("/path", UriKind.Relative)]
    public async Task ReturnFakeResponseWhenHeaderValueMatches(string uri, UriKind uriKind)
    {
        _context.Request.Headers["name"] = "value";

        var configuration = new FakeResponseConfiguration(
            Header: ("name", "value"),
            Path: string.Empty,
            QueryParameters: [],
            DynamicQueryParameters: [],
            StatusCode: HttpStatusCode.Gone,
            Content: "content");

        var sut = new FakeResponseHandler(_httpContextAccessor, configuration)
        {
            InnerHandler = new TestHandler()
        };

        var invoker = new HttpMessageInvoker(sut);

        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(uri, uriKind)
        };

        var response = await invoker.SendAsync(request, default);

        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
        content.Should().Be("content");
    }

    [Theory]
    [InlineData("https://example.com/path", UriKind.Absolute)]
    [InlineData("/path", UriKind.Relative)]
    public async Task ReturnFakeResponseWhenPathMatches(string uri, UriKind uriKind)
    {
        _context.Request.Headers["name"] = "value";

        var configuration = new FakeResponseConfiguration(
            Header: ("name", "value"),
            Path: "/path",
            QueryParameters: [],
            DynamicQueryParameters: [],
            StatusCode: HttpStatusCode.Gone,
            Content: string.Empty);

        var sut = new FakeResponseHandler(_httpContextAccessor, configuration)
        {
            InnerHandler = new TestHandler()
        };

        var invoker = new HttpMessageInvoker(sut);

        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(uri, uriKind)
        };

        var response = await invoker.SendAsync(request, default);

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Theory]
    [InlineData("https://example.com?fakeQueryParam=fakeValue&anotherQueryParam=testValue", UriKind.Absolute)]
    [InlineData("?fakeQueryParam=fakeValue&anotherQueryParam=testValue", UriKind.Relative)]
    public async Task ReturnFakeResponseWhenQueryParametersMatch(string uri, UriKind uriKind)
    {
        _context.Request.Headers["name"] = "value";

        Dictionary<string, string> queryParameters = [];

        queryParameters.Add("fakeQueryParam", "fakeValue");
        queryParameters.Add("anotherQueryParam", "testValue");

        var configuration = new FakeResponseConfiguration(
            Header: ("name", "value"),
            Path: string.Empty,
            QueryParameters: queryParameters,
            DynamicQueryParameters: [],
            StatusCode: HttpStatusCode.Gone,
            Content: string.Empty);

        var sut = new FakeResponseHandler(_httpContextAccessor, configuration)
        {
            InnerHandler = new TestHandler()
        };

        var invoker = new HttpMessageInvoker(sut);

        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(uri, uriKind)
        };

        var response = await invoker.SendAsync(request, default);

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Theory]
    [InlineData("https://example.com?dynamicParamOne=email eq test@gmail.com&dynamicParamTwo=somethingTHISsomething", UriKind.Absolute)]
    [InlineData("?dynamicParamOne=email eq test@gmail.com&dynamicParamTwo=somethingTHISsomething", UriKind.Relative)]
    public async Task ReturnFakeResponseWhenDynamicQueryParametersMatch(string uri, UriKind uriKind)
    {
        _context.Request.Headers["name"] = "value";

        Dictionary<string, Func<string, bool>> dynamicQueryParameters = [];

        dynamicQueryParameters.Add("dynamicParamOne", (qp) => qp.StartsWith("email eq "));
        dynamicQueryParameters.Add("dynamicParamTwo", (qp) => qp.Contains("THIS"));

        var configuration = new FakeResponseConfiguration(
            Header: ("name", "value"),
            Path: string.Empty,
            QueryParameters: [],
            DynamicQueryParameters: [],
            StatusCode: HttpStatusCode.Gone,
            Content: string.Empty);

        var sut = new FakeResponseHandler(_httpContextAccessor, configuration)
        {
            InnerHandler = new TestHandler()
        };

        var invoker = new HttpMessageInvoker(sut);

        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(uri, uriKind)
        };

        var response = await invoker.SendAsync(request, default);

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task ReturnRealResponseWhenHeadersAreNull()
    {
        _httpContextAccessor.HttpContext.Returns(null as HttpContext);

        var configuration = new FakeResponseConfiguration(
            Header: ("name", "value"),
            Path: string.Empty,
            QueryParameters: [],
            DynamicQueryParameters: [],
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
    public async Task ReturnRealResponseWhenRequestUriIsNull()
    {
        _context.Request.Headers["name"] = "value";

        var configuration = new FakeResponseConfiguration(
            Header: ("name", "value"),
            Path: "/path/for/fakeResponse",
            QueryParameters: [],
            DynamicQueryParameters: [],
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
    public async Task ReturnRealResponseWhenHeaderValueDoesNotMatch()
    {
        _context.Request.Headers["name"] = "anotherValue";

        var configuration = new FakeResponseConfiguration(
            Header: ("name", "value"),
            Path: string.Empty,
            QueryParameters: [],
            DynamicQueryParameters: [],
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
    public async Task ReturnRealResponseWhenPathDoesNotMatch()
    {
        _context.Request.Headers["name"] = "value";

        var configuration = new FakeResponseConfiguration(
            Header: ("name", "value"),
            Path: "/path/for/fakeResponse",
            QueryParameters: [],
            DynamicQueryParameters: [],
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
    public async Task ReturnRealResponseWhenQueryParametersDoNotMatch()
    {
        _context.Request.Headers["name"] = "value";

        Dictionary<string, string> queryParameters = [];

        queryParameters.Add("fakeQueryParam", "fakeValue");

        var configuration = new FakeResponseConfiguration(
            Header: ("name", "value"),
            Path: string.Empty,
            QueryParameters: queryParameters,
            DynamicQueryParameters: [],
            StatusCode: HttpStatusCode.Gone,
            Content: string.Empty);

        var sut = new FakeResponseHandler(_httpContextAccessor, configuration)
        {
            InnerHandler = new TestHandler()
        };

        var invoker = new HttpMessageInvoker(sut);

        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri("https://example.com?test=value")
        };

        var response = await invoker.SendAsync(request, default);

        response.StatusCode.Should().Be(HttpStatusCode.UnavailableForLegalReasons);
    }

    [Fact]
    public async Task ReturnRealResponseWhenDynamicQueryParametersDoNotMatch()
    {
        _context.Request.Headers["name"] = "value";

        Dictionary<string, Func<string, bool>> dynamicQueryParameters = [];

        dynamicQueryParameters.Add("dynamicParamOne", (qp) => qp.StartsWith("email eq "));

        var configuration = new FakeResponseConfiguration(
            Header: ("name", "value"),
            Path: string.Empty,
            QueryParameters: [],
            DynamicQueryParameters: dynamicQueryParameters,
            StatusCode: HttpStatusCode.Gone,
            Content: string.Empty);

        var sut = new FakeResponseHandler(_httpContextAccessor, configuration)
        {
            InnerHandler = new TestHandler()
        };

        var invoker = new HttpMessageInvoker(sut);

        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri("https://example.com?test=value")
        };

        var response = await invoker.SendAsync(request, default);

        response.StatusCode.Should().Be(HttpStatusCode.UnavailableForLegalReasons);
    }
}

public sealed class TestHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(new HttpResponseMessage(HttpStatusCode.UnavailableForLegalReasons));
}