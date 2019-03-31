using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleEventBus.RabbitMQ
{
    public class RabbitMQEventBus : IEventBus, IPublisher
    {
        const string BROKER_NAME = "simple_event_bus";

        private readonly IRabbitMQConnection _persistentConnection;
        private readonly ILogger _logger;
        private readonly EventReceivedFunc _eventReceiver;
        private readonly SubscribeEventFunc _subscribeEventFunc;
        private readonly IEventHandlerProvider _eventHandlerProvider;
        private readonly IEventSerializer<byte[]> _eventSerializer;
        private readonly IEventNameTypeResolver _eventNameTypeResolver;
        private readonly RabbitMQSubscriptions _subscriptions;

        private IModel _consumerChannel;
        private string _queueName; //= "";

        public ISubscriptions Subscriptions => _subscriptions;

        public RabbitMQEventBus(ILoggerFactory loggerFactory, 
            IRabbitMQConnection persistentConnection, 
            RabbitMQSubscriptions subscriptions, 
            EventReceivedFunc eventReceiver,
            SubscribeEventFunc subscribeEventFunc,
            IEventHandlerProvider eventHandlerProvider,
            IEventSerializer<byte[]> eventSerializer, 
            IEventNameTypeResolver eventNameTypeResolver)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _logger = loggerFactory.CreateLogger<RabbitMQEventBus>();

            _subscriptions = subscriptions;
            _eventReceiver = eventReceiver;
            _subscribeEventFunc = subscribeEventFunc;
            _eventHandlerProvider = eventHandlerProvider;
            _eventSerializer = eventSerializer;
            _eventNameTypeResolver = eventNameTypeResolver;
        }

        public async Task Start()
        {
            _queueName = $"simple_event_bus_rq_" + Guid.NewGuid().ToString("n");

            _subscriptions.Start(BROKER_NAME, _queueName);

            await Task.WhenAll(_subscribeEventFunc.Invoke().Select(_ => _subscriptions.Subscribe(_)));

            await Task.Factory.StartNew(() =>
            {
                _consumerChannel = CreateConsumerChannel();
            });
        }

        public async Task Stop()
        {
            await Subscriptions.Clear();

            await Task.Factory.StartNew(() =>
            {
                var channel = _consumerChannel;
                _consumerChannel = null;
                if (channel != null)
                {
                    channel.ModelShutdown -= channel_ModelShutdown;
                    channel.CallbackException -= channel_CallbackException;

                    try_del_queue(channel);

                    channel.Dispose();
                }
            });
        }
        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            var channel = _persistentConnection.CreateChannel();

            try_create_exchange(channel);
            try_create_queue(channel);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                await OnEventReceived(ea.RoutingKey, ea.Body);
            };

            channel.BasicConsume(_queueName, true, consumer);

            channel.ModelShutdown += channel_ModelShutdown;
            channel.CallbackException += channel_CallbackException;

            return channel;
        }

        void channel_ModelShutdown(object _, ShutdownEventArgs e)
        {
            _logger.LogWarning("Consumer channel is on shutdown. Trying to re-create...");
            _consumerChannel.Dispose();
            _consumerChannel = CreateConsumerChannel();
        }

        void channel_CallbackException(object _, CallbackExceptionEventArgs e)
        {
            _logger.LogWarning("Consumer channel throw exception. Trying to re-create...");
            _consumerChannel.Dispose();
            _consumerChannel = CreateConsumerChannel();
        }

        void try_create_exchange(IModel channel)
        {
            channel.ExchangeDeclare(
                exchange: BROKER_NAME,
                type: "direct",
                durable: true,
                autoDelete: false,
                arguments: null
            );
        }

        void try_create_queue(IModel channel)
        {
            channel.QueueDeclare(
                queue: _queueName,
                durable: false,
                exclusive: true,
                autoDelete: false,
                arguments: null
            );
        }

        void try_del_queue(IModel channel)
        {
            channel.QueueDeleteNoWait(_queueName, false, false);
        }

        public IPublisher Publisher => this;

        public Task Publish<T>(T eventEntity) where T : IEvent
        {
            return Task.Factory.StartNew(() =>
            {
                if (!_persistentConnection.IsConnected)
                    _persistentConnection.TryConnect();

                using (var channel = _persistentConnection.CreateChannel())
                {
                    try_create_exchange(channel);

                    var eventName = _eventNameTypeResolver.GetEventName(eventEntity?.GetType());
                    var body = _eventSerializer.Serialize(eventEntity);

                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2; // persistent

                    Exception ex = null;
                    for (int i = 0, c = 5; i < c; i++)
                    {
                        try
                        {
                            channel.BasicPublish(BROKER_NAME, eventName, properties, body);
                            break;
                        }
                        catch (Exception ex0)
                        {
                            if (i + 1 == c) ex = ex0;
                            else Task.Delay(500).Wait();
                        }
                    }
                    if (ex != null)
                    {
                        _logger.LogError(ex.ToString());
                        throw ex;
                    }
                }
            });
        }

        Task OnEventReceived(string eventName, byte[] @event)
        {
            if (Subscriptions.IsEmpty) return Task.CompletedTask;

            var eventType = _eventNameTypeResolver.GetEventType(eventName);
            var eo = _eventSerializer.Deserialize(eventType, @event);

            if (!Subscriptions.HasSubscription(eventType)) return Task.CompletedTask;

            var handlers = _eventHandlerProvider.GetHandlers(eventType);
            if (handlers == null) return Task.CompletedTask;

            return _eventReceiver.Invoke(eventType, eo, handlers);
        }
    }
}