# Fake Response
![Fake Response](assets/fakeresponse.png "Fake Response")

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/damonjames/fake-response/ci.yml)
![GitHub Tag Version](https://img.shields.io/github/v/tag/DamonJames/fake-response)
![GitHub Tag Version](https://img.shields.io/nuget/dt/FakeResponse)

A simple HTTP client builder extension to intercept and return fake responses based on request headers

## About

This package provides a customisable fake HTTP response handler for simulating third-party API responses based on request headers. Designed for end-to-end testing of front-end applications, back-end services, and databases, it allows developers to control and mock external service responses without relying on real third-party systems, test data, or their availability.

By configuring specific request headers, the package intercepts outgoing HTTP requests and returns predefined or dynamic fake responses. This enables seamless integration testing across the entire stack, ensuring consistent, reliable tests regardless of external dependencies.

Key Features:

- Header-Based Response Simulation: Customize fake responses based on request headers to mimic different scenarios, status codes, and payloads.
- End-to-End Testing: Test web applications, microservices, and databases without relying on third-party services or external data.
- Improved Reliability: Eliminate issues related to third-party service outages or network delays during testing.
- Flexible Configuration: Easily define and manage mock responses through configuration files or code-based setup.

This package is ideal for teams looking to enhance testing efficiency, reduce test flakiness, and gain confidence in their application's behavior in various external integration scenarios.

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

3. In your middleware where you are building a new HTTP client calling out to an external API, add the following:
```
builder.Services.AddHttpClient(...)
  .AddFakeResponseHandler(config => config
    .ForHeaderValue("MyHeaderValue")
    .ReturnStatus(HttpStatusCode.OK));
```
4. Run your app, ensure your endpoints are hooked up to call the client in question.
5. Make requests with your fake headers and observe your fake response!