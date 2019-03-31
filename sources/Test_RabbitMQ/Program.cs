using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class DelayMQueue
{
    readonly ConnectionFactory _connectionFactory;
    readonly DelayMQueueOption _option;
    readonly ILogger _logger;
    readonly object sync_root = new object();
    IConnection _connection;
    IModel _channel;
    Action<byte[]> _consumer;

    public DelayMQueue(ConnectionFactory connectionFactory, ILoggerFactory loggerFactory, DelayMQueueOption option)
    {
        _connectionFactory = connectionFactory;
        _option = option;
        _logger = loggerFactory.CreateLogger("dlx ttl");

        _connectionFactory.AutomaticRecoveryEnabled = false;
    }

    bool isConnected => _connection != null && _connection.IsOpen;
    string routeKey => _option.TtlDlxQueue.Arguments["x-dead-letter-routing-key"].ToString();

    bool TryConnect(bool c = false)
    {
        if (isConnected) return true;

        lock (sync_root)
        {
            if (isConnected) return true;

            try
            {
                _connection = _connectionFactory.CreateConnection();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.ToString());
                _logger.LogError("FATAL ERROR: RabbitMQ connections could not be created and opened");
                throw ex;
            }

            _connection.ConnectionShutdown += OnConnectionShutdown;
            _connection.CallbackException += OnCallbackException;
            _connection.ConnectionBlocked += OnConnectionBlocked;

            _logger.LogInformation("created conn");

            if (c || _channel != null)
            {
                _channel?.Dispose();
                _channel = create_channel(true);
            }

            return true;
        }
    }

    IModel create_channel(bool c = false)
    {
        var channel = _connection.CreateModel();

        if (_option.TtlDlxExchange.Type != null)
            channel.ExchangeDeclare(_option.TtlDlxExchange.Name, _option.TtlDlxExchange.Type, _option.TtlDlxExchange.Durable, _option.TtlDlxExchange.AutoDelete, _option.TtlDlxExchange.Arguments);
        if (_option.TtlDlxQueue != null)
        {
			_option.TtlDlxQueue.Arguments["x-message-ttl"] = Convert.ToInt32(_option.TtlDlxQueue.Arguments["x-message-ttl"]);
            channel.QueueDeclare(_option.TtlDlxQueue.Name, _option.TtlDlxQueue.Durable, _option.TtlDlxQueue.Exclusive, _option.TtlDlxQueue.AutoDelete, _option.TtlDlxQueue.Arguments);
            channel.QueueBind(_option.TtlDlxQueue.Name, _option.TtlDlxExchange.Name, routeKey, null);
        }
        if (_option.FallbackExchange.Type != null)
            channel.ExchangeDeclare(_option.FallbackExchange.Name, _option.FallbackExchange.Type, _option.FallbackExchange.Durable, _option.FallbackExchange.AutoDelete, _option.FallbackExchange.Arguments);
        if (_option.FallbackQueue != null)
        {
            channel.QueueDeclare(_option.FallbackQueue.Name, _option.FallbackQueue.Durable, _option.FallbackQueue.Exclusive, _option.FallbackQueue.AutoDelete, _option.FallbackQueue.Arguments);
            channel.QueueBind(_option.FallbackQueue.Name, _option.FallbackExchange.Name, routeKey, null);
        }

        if (c)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += consumer_Received;
            channel.BasicConsume(_option.FallbackQueue.Name, true, consumer);
        }

        return channel;
    }

    public void Product(byte[] message)
    {
        TryConnect();
        using (var channel = create_channel())
            channel.BasicPublish(_option.TtlDlxExchange.Name, routeKey, null, message);
    }

    public void Consume(Action<byte[]> consumer)
    {
        TryConnect(true);
        _consumer += consumer;
    }

    void consumer_Received(object _, BasicDeliverEventArgs e)
    {
        _consumer?.Invoke(e.Body);
    }

    void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
    {
        _logger.LogWarning("A RabbitMQ connection is blocked. Trying to re-connect...");
        TryConnect();
    }

    void OnCallbackException(object sender, CallbackExceptionEventArgs e)
    {
        _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");
        TryConnect();
    }

    void OnConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");
        TryConnect();
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("config.json")
            .Build();

        var option = config.GetSection("DelayQueues:test_delay").Get<DelayMQueueOption>();
        var logf = new LoggerFactory().AddConsole();
        var log = logf.CreateLogger("main");

        var dq = new DelayMQueue(new ConnectionFactory
        {
            UserName = "root",
            Password = "root",
        }, logf, option);

        _ = Task.Factory.StartNew(() => 
        {
            dq.Consume(bys => 
            {
                log.LogInformation($"recv {DateTime.Now.ToString("HH:mm:ss.fff")} {Encoding.UTF8.GetString(bys)}");
            });
        }, TaskCreationOptions.LongRunning);
        //pause();

        log.LogInformation($"send 123 at {DateTime.Now.ToString("HH:mm:ss.fff")}");
        dq.Product(Encoding.UTF8.GetBytes("123"));
        await Task.Delay(1000 * 5);
        log.LogInformation($"send 456 at {DateTime.Now.ToString("HH:mm:ss.fff")}");
        dq.Product(Encoding.UTF8.GetBytes("456"));

        pause();
    }
    
    static void pause()
    {
		Console.WriteLine("{{pause...}}");
        Console.ReadLine();
    }
}
