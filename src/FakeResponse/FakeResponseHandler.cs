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
        var headers = _httpContextAccessor.HttpContext?.Request?.Headers;

        if (FakeHeaderMatches(headers) &&
            PathMatches(request) &&
            QueryParamsMatch(request) &&
            DynamicQueryParamsMatch(request))
        {
            var response = new HttpResponseMessage(_fakeResponseConfiguration.StatusCode)
            {
                Content = new StringContent(_fakeResponseConfiguration.Content)
            };

            return await Task.FromResult(response);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private bool PathMatches(HttpRequestMessage request)
    {
        if (string.IsNullOrWhiteSpace(_fakeResponseConfiguration?.Path))
        {
            return true;
        }

        if (request.RequestUri == null)
        {
            return false;
        }

        if (request.RequestUri.IsAbsoluteUri)
        {
            return request.RequestUri?.AbsolutePath == _fakeResponseConfiguration.Path;
        }

        return request.RequestUri.ToString() == _fakeResponseConfiguration.Path;
    }

    private bool QueryParamsMatch(HttpRequestMessage request)
    {
        if (_fakeResponseConfiguration.QueryParameters.Count == 0)
        {
            return true;
        }

        if (request.RequestUri == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.RequestUri.Query))
        {
            return false;
        }

        var queryParams = HttpUtility.ParseQueryString(request.RequestUri.Query);

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

    private bool DynamicQueryParamsMatch(HttpRequestMessage request)
    {
        if (_fakeResponseConfiguration.DynamicQueryParameters.Count == 0)
        {
            return true;
        }

        if (request.RequestUri == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.RequestUri.Query))
        {
            return false;
        }

        var queryParams = HttpUtility.ParseQueryString(request.RequestUri.Query);

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
