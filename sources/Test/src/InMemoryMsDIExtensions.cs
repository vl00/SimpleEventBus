using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace SimpleEventBus.InMemory
{
    static class InMemoryMsDIExtensions
    {
        public static SimpleEventBusMsDIBuilder AddInMemorySubscriptionsManager(this SimpleEventBusMsDIBuilder builder, Action<ISubscriptionsManager> config = null)
        {
            builder.Config(services =>
            {
                services.TryAddSingleton<ISubscriptionsManager>(_ =>
                {
                    var smr = new InMemorySubscriptionsManager();
                    config?.Invoke(smr);
                    return smr;
                });
            });
            return builder;
        }

        public static SimpleEventBusMsDIBuilder AddInMemory(this SimpleEventBusMsDIBuilder builder, Action<ISubscriptionsManager> config = null)
        {
            builder.AddInMemorySubscriptionsManager(config);
            builder.Config(services =>
            {
                services.TryAddScoped<IPublisher, InMemoryPublisher>();
                services.TryAddScoped<IEventBus, InMemoryEventBus>();
            });
            return builder;
        }
    }
}