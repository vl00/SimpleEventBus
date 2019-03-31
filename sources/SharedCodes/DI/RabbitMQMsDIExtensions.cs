using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMQ.Client;
using SimpleEventBus.InMemory;
using System;

namespace SimpleEventBus.RabbitMQ
{
    static class RabbitMQMsDIExtensions
    {
        public static SimpleEventBusMsDIBuilder AddRabbitMQ(this SimpleEventBusMsDIBuilder builder,  
            Func<ConnectionFactory> configConnectionFactory = null)
        {
            if (configConnectionFactory != null) builder.Services.TryAddSingleton(_ => configConnectionFactory());

            builder.Services.TryAddSingleton<RabbitMQEventBus>();
            builder.Services.TryAddSingleton<IRabbitMQConnection, DefaultRabbitMQConnection>();
            builder.Services.TryAddSingleton<RabbitMQSubscriptions>();

            return builder;
        }
    }
}