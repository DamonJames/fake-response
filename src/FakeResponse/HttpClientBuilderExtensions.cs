using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace FakeResponse;

public static partial class HttpClientBuilderExtensions
{
    public static IHttpClientBuilder AddFakeResponseHandler<TFakeContent>(
        this IHttpClientBuilder builder,
        Action<FakeResponseConfigurationBuilder<TFakeContent>> setupAction)
    {
        if (FakeResponseOptions.Global.IsProductionEnvironment)
        {
            return builder;
        }

        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(setupAction);

        var fakeResponseConfigurationBuilder = new FakeResponseConfigurationBuilder<TFakeContent>();

        setupAction.Invoke(fakeResponseConfigurationBuilder);

        var serviceKey = Guid.NewGuid();

        builder.Services.AddKeyedTransient(serviceKey, (sp, _) =>
            new FakeResponseHandler<TFakeContent>(sp.GetRequiredService<IHttpContextAccessor>(), fakeResponseConfigurationBuilder.Build()));

        builder.Services.Configure<HttpClientFactoryOptions>(builder.Name, options =>
        {
            options.HttpMessageHandlerBuilderActions.Add(b => b.AdditionalHandlers.Add(b.Services.GetRequiredKeyedService<FakeResponseHandler<TFakeContent>>(serviceKey)));
        });

        return builder;
    }

    public static IHttpClientBuilder AddFakeResponseHandler(
        this IHttpClientBuilder builder,
        Action<FakeResponseConfigurationBuilder> setupAction)
    {
        if (FakeResponseOptions.Global.IsProductionEnvironment)
        {
            return builder;
        }

        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(setupAction);

        var fakeResponseConfigurationBuilder = new FakeResponseConfigurationBuilder();

        setupAction.Invoke(fakeResponseConfigurationBuilder);

        var serviceKey = Guid.NewGuid();

        builder.Services.AddKeyedTransient(serviceKey, (sp, _) =>
            new FakeResponseHandler(sp.GetRequiredService<IHttpContextAccessor>(), fakeResponseConfigurationBuilder.Build()));

        builder.Services.Configure<HttpClientFactoryOptions>(builder.Name, options =>
        {
            options.HttpMessageHandlerBuilderActions.Add(b => b.AdditionalHandlers.Add(b.Services.GetRequiredKeyedService<FakeResponseHandler>(serviceKey)));
        });

        return builder;
    }
}