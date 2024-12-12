using Microsoft.AspNetCore.Http;
using System.Web;

namespace FakeResponse;

internal sealed class FakeResponseHandler(
    IHttpContextAccessor httpContextAccessor,
    FakeResponseConfiguration fakeResponseConfiguration) : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly FakeResponseConfiguration _fakeResponseConfiguration = fakeResponseConfiguration;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext?.Request?.Headers == null ||
            request.RequestUri == null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var headers = _httpContextAccessor.HttpContext?.Request?.Headers;

        var requestUri = request.RequestUri.IsAbsoluteUri ? request.RequestUri : new Uri($"https://example.com{request.RequestUri}");

        if (FakeHeaderMatches(headers) &&
            PathMatches(requestUri) &&
            QueryParamsMatch(requestUri) &&
            DynamicQueryParamsMatch(requestUri))
        {
            var response = new HttpResponseMessage(_fakeResponseConfiguration.StatusCode)
            {
                Content = new StringContent(_fakeResponseConfiguration.Content)
            };

            return await Task.FromResult(response);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private bool PathMatches(Uri requestUri)
    {
        if (string.IsNullOrWhiteSpace(_fakeResponseConfiguration?.Path))
        {
            return true;
        }

        return requestUri.AbsolutePath == _fakeResponseConfiguration.Path;
    }

    private bool QueryParamsMatch(Uri requestUri)
    {
        if (_fakeResponseConfiguration.QueryParameters.Count == 0)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(requestUri.Query))
        {
            return false;
        }

        var queryParams = HttpUtility.ParseQueryString(requestUri.Query);

        if (queryParams.Count == 0)
        {
            return false;
        }

        return _fakeResponseConfiguration.QueryParameters.All(qp =>
        {
            var requestQpValue = queryParams[qp.Key];

            if (requestQpValue == null)
            {
                return false;
            }

            if (requestQpValue == "*" || requestQpValue == qp.Value)
            {
                return true;
            }

            return false;
        });
    }

    private bool DynamicQueryParamsMatch(Uri requestUri)
    {
        if (_fakeResponseConfiguration.DynamicQueryParameters.Count == 0)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(requestUri.Query))
        {
            return false;
        }

        var queryParams = HttpUtility.ParseQueryString(requestUri.Query);

        if (queryParams.Count == 0)
        {
            return false;
        }

        return _fakeResponseConfiguration.DynamicQueryParameters.All(qp =>
        {
            var requestQpValue = queryParams[qp.Key];

            if (requestQpValue == null)
            {
                return false;
            }

            return qp.Value(requestQpValue);
        });
    }

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
