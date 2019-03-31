using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace SimpleEventBus
{
    class SimpleEventBusMsDIBuilder
    {
        public IServiceCollection Services { get; }

        public SimpleEventBusMsDIBuilder(IServiceCollection services)
        {
            this.Services = services;
        }
    }

    static class MsDIExtensions
    {
        public static SimpleEventBusMsDIBuilder AddSimpleEventBus(this IServiceCollection services)
        {
            services.TryAddScoped<IEventHandlerProvider, DefaultEventHandlerProvider>();
            return new SimpleEventBusMsDIBuilder(services);
        }
    }

    static class MsDIExtensions_Remote
    {
        public static SimpleEventBusMsDIBuilder AddEventReceivedFunc(this SimpleEventBusMsDIBuilder builder, EventReceivedFunc func)
        {
            builder.Services.TryAddSingleton(func);
            return builder;
        }

        public static SimpleEventBusMsDIBuilder AddSubscribeEventFunc(this SimpleEventBusMsDIBuilder builder, SubscribeEventFunc func)
        {
            builder.Services.TryAddSingleton(func);
            return builder;
        }

        public static SimpleEventBusMsDIBuilder AddEventNameTypeResolver(this SimpleEventBusMsDIBuilder builder, Func<IServiceProvider, IEventNameTypeResolver> func = null)
        {
            if (func != null) builder.Services.TryAddSingleton(func);
            else builder.Services.TryAddSingleton<IEventNameTypeResolver, DefaultEventNameTypeResolver>();
            return builder;
        }

        public static SimpleEventBusMsDIBuilder AddEventSerializer<TRaw>(this SimpleEventBusMsDIBuilder builder, Func<IServiceProvider, IEventSerializer<TRaw>> func = null)
        {
            builder.Services.TryAddSingleton(func);
            return builder;
        }
    }
}