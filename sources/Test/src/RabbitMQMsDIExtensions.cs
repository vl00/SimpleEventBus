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
            EventReceivedFunc configEventReceived = null, 
            Func<IServiceProvider, IEventSerializer<byte[]>> configSerializer = null,
            Func<ConnectionFactory> configConnectionFactory = null)
        {
            builder.Config(services =>
            {   
                if (configEventReceived != null) services.TryAddSingleton(configEventReceived);
                if (configSerializer != null) services.TryAddSingleton(_ => configSerializer(_));
                if (configConnectionFactory != null) services.TryAddSingleton(_ => configConnectionFactory());

                services.TryAddSingleton<IEventNameTypeResolver, DefaultEventNameTypeResolver>();
                services.TryAddSingleton<IEventBus, RabbitMQEventBus>();
                services.TryAddSingleton<IRabbitMQConnection, DefaultRabbitMQConnection>();
            });
            return builder;
        }
    }
}