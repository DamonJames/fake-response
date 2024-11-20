using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FakeResponse.Tests;

public sealed class AddFakeResponseHandlerShould
{
    private readonly IHttpClientBuilder _httpClientBuilder;

    public AddFakeResponseHandlerShould()
    {
        _httpClientBuilder = Substitute.For<IHttpClientBuilder>();
        _httpClientBuilder.Name.Returns("TestClient");
    }

    [Fact]
    public void AddFakeResponseHandlerToServiceCollectionAndHttpMessageBuilder()
    {
        var serviceCollection = new ServiceCollection();
        _httpClientBuilder.Services.Returns(serviceCollection);
        FakeResponseOptions.Global.IsProductionEnvironment = false;

        _httpClientBuilder.AddFakeResponseHandler(opt => opt.ForHeaderValue("Test"));

        var serviceDescriptor = serviceCollection
            .SingleOrDefault(sd => sd.ServiceType == typeof(FakeResponseHandler) && sd.Lifetime == ServiceLifetime.Transient);
        serviceDescriptor.Should().NotBeNull();

        var httpClientFactoryOptionsDescriptor = serviceCollection
            .SingleOrDefault(sd => sd.ServiceType == typeof(IConfigureOptions<HttpClientFactoryOptions>));
        httpClientFactoryOptionsDescriptor.Should().NotBeNull();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>();
        var options = optionsMonitor.Get("TestClient");

        options.Should().NotBeNull();
        options.HttpMessageHandlerBuilderActions.Should().HaveCount(1);
    }

    [Fact]
    public void NotAddFakeResponseHandlerToServiceCollection()
    {
        var serviceCollection = new ServiceCollection();
        _httpClientBuilder.Services.Returns(serviceCollection);
        FakeResponseOptions.Global.IsProductionEnvironment = true;

        _httpClientBuilder.AddFakeResponseHandler(opt => opt.ForHeaderValue("Test"));

        var serviceDescriptor = serviceCollection
            .SingleOrDefault(sd => sd.ServiceType == typeof(FakeResponseHandler) && sd.Lifetime == ServiceLifetime.Transient);
        serviceDescriptor.Should().BeNull();

        var httpClientFactoryOptionsDescriptor = serviceCollection
            .SingleOrDefault(sd => sd.ServiceType == typeof(IConfigureOptions<HttpClientFactoryOptions>));
        httpClientFactoryOptionsDescriptor.Should().BeNull();
    }

    [Fact]
    public void AddFakeResponseHandlerWithContentToServiceCollectionAndHttpMessageBuilder()
    {
        var serviceCollection = new ServiceCollection();
        _httpClientBuilder.Services.Returns(serviceCollection);
        FakeResponseOptions.Global.IsProductionEnvironment = false;

        _httpClientBuilder.AddFakeResponseHandler<object>(opt => opt.ReturnContent(new { }));

        var serviceDescriptor = serviceCollection
            .SingleOrDefault(sd => sd.ServiceType == typeof(FakeResponseHandler<object>) && sd.Lifetime == ServiceLifetime.Transient);
        serviceDescriptor.Should().NotBeNull();

        var httpClientFactoryOptionsDescriptor = serviceCollection
            .SingleOrDefault(sd => sd.ServiceType == typeof(IConfigureOptions<HttpClientFactoryOptions>));
        httpClientFactoryOptionsDescriptor.Should().NotBeNull();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>();
        var options = optionsMonitor.Get("TestClient");

        options.Should().NotBeNull();
        options.HttpMessageHandlerBuilderActions.Should().HaveCount(1);
    }

    [Fact]
    public void NotAddFakeResponseHandlerWithContentToServiceCollection()
    {
        var serviceCollection = new ServiceCollection();
        _httpClientBuilder.Services.Returns(serviceCollection);
        FakeResponseOptions.Global.IsProductionEnvironment = true;

        _httpClientBuilder.AddFakeResponseHandler<object>(opt => opt.ReturnContent(new { }));

        var serviceDescriptor = serviceCollection
            .SingleOrDefault(sd => sd.ServiceType == typeof(FakeResponseHandler<object>) && sd.Lifetime == ServiceLifetime.Transient);
        serviceDescriptor.Should().BeNull();

        var httpClientFactoryOptionsDescriptor = serviceCollection
            .SingleOrDefault(sd => sd.ServiceType == typeof(IConfigureOptions<HttpClientFactoryOptions>));
        httpClientFactoryOptionsDescriptor.Should().BeNull();
    }
}
