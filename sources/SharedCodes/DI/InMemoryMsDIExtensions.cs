using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace SimpleEventBus.InMemory
{
    static class InMemoryMsDIExtensions
    {
        public static SimpleEventBusMsDIBuilder AddInMemory(this SimpleEventBusMsDIBuilder builder, Action<IServiceCollection> config)
        {
            config(builder.Services);
            return builder;
        }

        public static SimpleEventBusMsDIBuilder AddInMemory(this SimpleEventBusMsDIBuilder builder)
        {
            builder.Services.TryAddScoped<InMemorySubscriptions>();
            builder.Services.TryAddScoped<InMemoryPublisher>();
            builder.Services.TryAddScoped<InMemoryEventBus>();
            return builder;
        }
    }
}