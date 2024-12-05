# Fake Response
![Fake Response](assets/fakeresponse.png "Fake Response")

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/damonjames/fake-response/ci.yml)
![GitHub Tag Version](https://img.shields.io/github/v/tag/DamonJames/fake-response)
![GitHub Tag Version](https://img.shields.io/nuget/dt/FakeResponse)

A simple HTTP client builder extension to intercept and return fake responses based on request headers

## Contents

1. [About](#about)
2. [Quickstart](#quickstart)
3. [Instructions](#instructions)

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
1. Download the latest version of `FakeResponse` via NuGet
2. At the very start of where you are registering your services (in either `Startup.cs` or `Program.cs`) file, add the following settings:

```
...
FakeResponseOptions.Global.IsProductionEnvironment = false;
...
```

3. In your middleware where you are building a new HTTP client calling out to an external API, add the following:

```
builder.Services.AddHttpClient(...)
  .AddFakeResponseHandler(config => config
    .ForHeader("MyHeaderName", "MyHeaderValue")
    .ReturnStatus(HttpStatusCode.OK));
```

4. Run your app, ensure your endpoints are hooked up to call the client in question.
5. Make requests with your fake header and observe your fake response 🎉

## Instructions
Find and install the latest version of `FakeResponse` via NuGet to your solution.

### Setting up your environment
Ensure you are injecting the `IHttpContextAccessor` during service registration. This allows for the fake response handlers to read the request headers.
```
builder.Services.AddHttpContextAccessor();
```

Ensure you have a method of detecting whether or not your service is running in a production environment.

The following property will need to be set based on this within the service registration process (`Startup.cs`/`Program.cs`):

```
FakeResponseOptions.Global.IsProductionEnvironment = true/false;
```

This is to ensure that the fake response handlers are not registered as part of any http clients if the service is running in production.

By default, this value is set to `true` to ensure that if this setting is missed, there is no chance of accidently deploying the fake response handlers in a production setting.

This setting must be declared **before** the initialisation of any http clients that are configured to use fake response handlers.

### Configuring your fake response handlers
The fake response handler configuration extends on the `IHttpClientBuilder` interface which is used to set up http clients during service registration.

Assuming you are using the built-in http client registration to inject your http clients, you would simply need to call the `AddFakeResponseHandler` extension method to begin configuration:

```
builder.Services.AddHttpClient(...).AddFakeResponseHandler(...);
```

The `AddFakeResponseHandler` extension method allows you to configure the specifications for returning a fake response in a fluent manner.

There are currently four configuration points you can tune to your needs:

#### 1. Header name & value
This is required to declare the custom header name and the value it should be for which the handler will detect and determine whether to return the fake response from.

##### Example:
```
.ForHeader("MyHeaderName", "MyHeaderValue")
```

#### 2. Request path
This is an optional value you can provide which will allow for returning a fake response from a specific path of the request you are intercepting. If this is not provided, the fake response will be returned from all paths (if the configured header and value is passed through).

##### Example:
```
.ForPath("/path/for/fake/response")
```

#### 3. Query parameters
This is an optional configuration you can provide to return fake responses based on requests made with query parameters. If this is not provided, the fake response will be returned for all query parameters (if the configured header and value is passed through). You are able to add as many as needed by chaining the method calls to add query parameters.

There are two ways you can provide query parameters; either by simply providing the query parameter key and value, or by providing a key and a function which passes through the query parameter value where you are able to run custom matching logic if your http request passes through dynamic query parameters which can differ with every request.

##### Example:
```
.ForQueryParameter("utm_source", "mobile")
.ForQueryParameter("userFilter", (value) => value.StartsWith("email eq"))
```

#### 4. Status code
This is an optional value you can provide to declare the status code to return in your fake response scenario. If this value is not provided, it is configured to return `HttpStatusCode.OK` by default.

##### Example:
```
.ReturnStatus(HttpStatusCode.OK)
```

#### 5. String content
This is an optional value you can provide to declare the string content to return in your fake response scenario.

##### Example:
```
.ReturnContent("myStringContent")
```

A full example of the above configuration goes as follows:
```
builder.Services.AddHttpClient(...)
  .AddFakeResponseHandler(config => config
    .ForHeader("MyHeaderName", "MyHeaderValue")
    .ForPath("/path/for/fake/response")
    .ForQueryParameter("utm_source", "mobile")
    .ForQueryParameter("userFilter", (value) => value.StartsWith("email eq"))
    .ReturnStatus(HttpStatusCode.OK)
    .ReturnContent("myStringContent"));
```

### Advance usage
The fake response handler is built to handle multiple scenarios. The value of the custom header should represent the scenario(s) you are returning the fake response for. With this, you are able to declare multiple scenarios under one custom header.

To do this, you simply need to comma-separate your header values
```
"MyHeaderName": "MyHeaderValue1,MyHeaderValue2"
```
And configure your handlers as so:
```
.AddFakeResponseHandler(config => config
  .ForHeader("MyHeaderName", "MyHeaderValue1")
  ...);
...
.AddFakeResponseHandler(config => config
  .ForHeader("MyHeaderName", "MyHeaderValue2")
  ...);
```
And both scenarios will be picked up by the handler.

You may also want to setup multiple fake responses under one http client, which is also possible. You are able to set up multiple handlers quite easily by chaining the add fake response handler method calls like so
```
builder.Services.AddHttpClient(...)
  .AddFakeResponseHandler(...)
  .AddFakeResponseHandler(...)
...
```
With the above, you can configure multiple scenarios for one http client however you wish.