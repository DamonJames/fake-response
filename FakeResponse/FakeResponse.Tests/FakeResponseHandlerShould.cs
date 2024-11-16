using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Net;

namespace FakeResponse.Tests;

public sealed class FakeResponseHandlerShould
{
    private readonly DefaultHttpContext _context;

    private FakeResponseConfiguration _configuration = new(
        Value: "TestValue",
        Path: null,
        StatusCode: HttpStatusCode.Gone);

    private readonly HttpMessageInvoker _invoker;

    public FakeResponseHandlerShould()
    {
        FakeResponseOptions.Global.HeaderName = "TestHeader";

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();

        _context = new DefaultHttpContext();

        httpContextAccessor.HttpContext.Returns(_context);

        var sut = new FakeResponseHandler(httpContextAccessor, _configuration)
        {
            InnerHandler = new TestHandler()
        };

        _invoker = new HttpMessageInvoker(sut);
    }

    [Fact]
    public async Task ReturnFakeResponse()
    {
        _context.Request.Headers["TestHeader"] = "TestValue";

        var response = await _invoker.SendAsync(new HttpRequestMessage(), default);

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task ReturnRealResponse()
    {
        _context.Request.Headers["TestHeader"] = "AnotherValue";

        var response = await _invoker.SendAsync(new HttpRequestMessage(), default);

        response.StatusCode.Should().Be(HttpStatusCode.UnavailableForLegalReasons);
    }
}

public sealed class TestHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(new HttpResponseMessage(HttpStatusCode.UnavailableForLegalReasons));
}
