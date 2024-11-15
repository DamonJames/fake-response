using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace FakeResponse;

internal sealed class FakeResponseHandler(
    IHttpContextAccessor httpContextAccessor,
    FakeResponseConfiguration fakeResponseConfiguration) :
    FakeResponseHandlerBase<FakeResponseConfiguration>(httpContextAccessor, fakeResponseConfiguration)
{
    private readonly FakeResponseConfiguration _fakeResponseConfiguration = fakeResponseConfiguration;

    protected override Task<HttpResponseMessage> FakeResponse() =>
        Task.FromResult(new HttpResponseMessage(_fakeResponseConfiguration.StatusCode));
}

internal sealed class FakeResponseHandler<TFakeContent>(
    IHttpContextAccessor httpContextAccessor,
    FakeResponseConfiguration<TFakeContent> fakeResponseConfiguration) :
    FakeResponseHandlerBase<FakeResponseConfiguration<TFakeContent>>(httpContextAccessor, fakeResponseConfiguration)
{
    private readonly FakeResponseConfiguration<TFakeContent> _fakeResponseConfiguration = fakeResponseConfiguration;

    protected override Task<HttpResponseMessage> FakeResponse() =>
        Task.FromResult(
            new HttpResponseMessage(_fakeResponseConfiguration.StatusCode)
            {
                Content = new StringContent(JsonSerializer.Serialize(_fakeResponseConfiguration.Content))
            });
}

internal abstract class FakeResponseHandlerBase<TFakeResponseConfiguration>(
    IHttpContextAccessor httpContextAccessor,
    IFakeResponseConfiguration fakeResponseConfiguration) : DelegatingHandler
    where TFakeResponseConfiguration : IFakeResponseConfiguration
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IFakeResponseConfiguration _fakeResponseConfiguration = fakeResponseConfiguration;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var headers = _httpContextAccessor.HttpContext?.Request?.Headers;

        if (headers == null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        // TODO: Add check for path if it exists in configuration

        if (FakeHeaderExistsAndValueMatches(headers))
        {
            return await FakeResponse();
        }

        return await base.SendAsync(request, cancellationToken);
    }

    protected abstract Task<HttpResponseMessage> FakeResponse();

    private bool FakeHeaderExistsAndValueMatches(IHeaderDictionary headers)
    {
        if (string.IsNullOrWhiteSpace(_fakeResponseConfiguration.Value))
        {
            return false;
        }

        if (!headers.TryGetValue(FakeResponseOptions.Global.HeaderName, out var fakeHeaderValue))
        {
            return false;
        }

        string? fakeHeaderString = fakeHeaderValue;

        if (string.IsNullOrEmpty(fakeHeaderString))
        {
            return false;
        }

        var splitHeaderValues = fakeHeaderString.Split(',');

        if (splitHeaderValues.Length < 1)
        {
            return false;
        }

        if (splitHeaderValues.Contains(_fakeResponseConfiguration.Value))
        {
            return true;
        }

        return false;
    }
}
