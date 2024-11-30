using Microsoft.AspNetCore.Http;

namespace FakeResponse;

internal sealed class FakeResponseHandler(
    IHttpContextAccessor httpContextAccessor,
    FakeResponseConfiguration fakeResponseConfiguration) : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly FakeResponseConfiguration _fakeResponseConfiguration = fakeResponseConfiguration;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var headers = _httpContextAccessor.HttpContext?.Request?.Headers;

        if (FakeHeaderMatches(headers) &&
            PathMatches(request))
        {
            var response = new HttpResponseMessage(_fakeResponseConfiguration.StatusCode)
            {
                Content = new StringContent(_fakeResponseConfiguration.Content)
            };

            return await Task.FromResult(response);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private bool PathMatches(HttpRequestMessage request) =>
        string.IsNullOrWhiteSpace(_fakeResponseConfiguration?.Path) ||
        request.RequestUri?.AbsolutePath == _fakeResponseConfiguration.Path;

    private bool FakeHeaderMatches(IHeaderDictionary? headers)
    {
        if (headers == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(_fakeResponseConfiguration.Header.name) ||
            string.IsNullOrWhiteSpace(_fakeResponseConfiguration.Header.value))
        {
            return false;
        }

        if (!headers.TryGetValue(_fakeResponseConfiguration.Header.name, out var fakeHeaderValue))
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

        if (splitHeaderValues.Contains(_fakeResponseConfiguration.Header.value))
        {
            return true;
        }

        return false;
    }
}
