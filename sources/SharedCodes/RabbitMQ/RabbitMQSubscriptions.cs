using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventBus.RabbitMQ
{
    public class RabbitMQSubscriptions : ISubscriptions
    {
        readonly ConcurrentDictionary<Type, object> _handlers = new ConcurrentDictionary<Type, object>();
        readonly IRabbitMQConnection _persistentConnection;
        readonly IEventNameTypeResolver _eventNameTypeResolver;

        string BROKER_NAME;
        string _queueName;

        public RabbitMQSubscriptions(IRabbitMQConnection persistentConnection, IEventNameTypeResolver eventNameTypeResolver)
        {
            _persistentConnection = persistentConnection;
            _eventNameTypeResolver = eventNameTypeResolver;
        }

        internal void Start(string BROKER_NAME, string _queueName)
        {
            this.BROKER_NAME = BROKER_NAME;
            this._queueName = _queueName;
        }

        public bool IsEmpty => _handlers.IsEmpty;

        public bool HasSubscription(Type type) => _handlers.ContainsKey(type);

        public Task Clear()
        {
            var cns = _handlers.ToArray();
            _handlers.Clear();

            return Task.Factory.StartNew(() =>
            {
                if (!_persistentConnection.IsConnected)
                    _persistentConnection.TryConnect();

                foreach (var d in cns)
                {
                    using (var channel = _persistentConnection.CreateChannel())
                    {
                        channel.QueueUnbind(
                            queue: _queueName,
                            exchange: BROKER_NAME,
                            routingKey: _eventNameTypeResolver.GetEventName(d.Key)
                        );
                    }
                }
            });
        }

        public Task Subscribe<T>() where T : IEvent
        {
            return Subscribe(typeof(T));
        }

        public Task Subscribe(Type type)
        {
            if (!typeof(IEvent).IsAssignableFrom(type)) return Task.CompletedTask;

            if (!_handlers.TryAdd(type, null)) return Task.CompletedTask;

            return Task.Factory.StartNew(() =>
            {
                if (!_persistentConnection.IsConnected)
                    _persistentConnection.TryConnect();

                using (var channel = _persistentConnection.CreateChannel())
                {
                    channel.QueueBind(
                        queue: _queueName,
                        exchange: BROKER_NAME,
                        routingKey: _eventNameTypeResolver.GetEventName(type),
                        arguments: null
                    );
                }
            });
        }

        public Task Unsubscribe<T>() where T : IEvent
        {
            return Unsubscribe(typeof(T));
        }

        public Task Unsubscribe(Type type)
        {
            if (typeof(IEvent).IsAssignableFrom(type) && _handlers.TryRemove(type, out _))
            {
                return Task.Factory.StartNew(() =>
                {
                    if (!_persistentConnection.IsConnected)
                        _persistentConnection.TryConnect();

                    using (var channel = _persistentConnection.CreateChannel())
                    {
                        channel.QueueUnbind(
                            queue: _queueName,
                            exchange: BROKER_NAME,
                            routingKey: _eventNameTypeResolver.GetEventName(type)
                        );
                    }
                });
            }

            return Task.CompletedTask;
        }
    }
}