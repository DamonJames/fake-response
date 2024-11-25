# Fake Response
![Fake Response](assets/fakeresponse.png "Fake Response")

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/damonjames/fake-response/ci.yml)
![GitHub Tag Version](https://img.shields.io/github/v/tag/DamonJames/fake-response)

A simple HTTP client builder extension to intercept and return fake responses based on request headers

## Quickstart
1. Download the latest version of the package via NuGet
2. In your `Startup.cs` file, add the following settings:
```
...
FakeResponseOptions.Global.IsProductionEnvironment = false;
FakeResponseOptions.Global.HeaderName = "MyHeaderName";
...
```
*Note*: the above example demonstrates simply hardcoding the production environment property to `false`, ensure in a real scenario there is suitable logic to detect whether the application is running in your production environment.

3. In your middleware where you are building a new HTTP client, add the following:
```
builder.Services.AddHttpClient(...)
  .AddFakeResponseHander(config => config
    .ForHeaderValue("MyHeaderValue")
    .ReturnStatus(HttpStatusCode.OK));
```
4. Run your app, make requests with your fake headers and observe your fake response!