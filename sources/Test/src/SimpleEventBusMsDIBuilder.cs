using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace SimpleEventBus
{
    class SimpleEventBusMsDIBuilder
    {
        readonly IServiceCollection services;

        public SimpleEventBusMsDIBuilder(IServiceCollection services)
        {
            this.services = services;
        }

        public void Config(Action<IServiceCollection> config) => config(services);
    }

    static class MsDIExtensions
    {
        public static SimpleEventBusMsDIBuilder AddSimpleEventBus(this IServiceCollection services) => new SimpleEventBusMsDIBuilder(services);
    }
}