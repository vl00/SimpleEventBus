using RabbitMQ.Client;
using System;

namespace SimpleEventBus.RabbitMQ
{
    public interface IRabbitMQConnection : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateChannel();
    }
}