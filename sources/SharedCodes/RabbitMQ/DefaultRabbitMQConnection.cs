using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SimpleEventBus.RabbitMQ
{
    public class DefaultRabbitMQConnection : IRabbitMQConnection
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly ILogger _logger;

        IConnection _connection;
        bool _disposed;

        readonly object sync_root = new object();

        public DefaultRabbitMQConnection(ConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = loggerFactory.CreateLogger<DefaultRabbitMQConnection>();

            _connectionFactory.AutomaticRecoveryEnabled = false;
            _connectionFactory.UseBackgroundThreadsForIO = true;
        }

        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

        public IModel CreateChannel()
        {
            if (!IsConnected)
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");

            return _connection.CreateModel();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try
            {
				if (_connection != null)
				{
					_connection.ConnectionShutdown -= OnConnectionShutdown;
                    _connection.CallbackException -= OnCallbackException;
                    _connection.ConnectionBlocked -= OnConnectionBlocked;
					_connection.Dispose();
				}
            }
            catch (IOException ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        public bool TryConnect()
        {
            _logger.LogInformation("RabbitMQ Client is trying to connect");
			
			if (_disposed) return false;
            if (IsConnected) return true;

            lock (sync_root)
            {
                if (IsConnected) return true;

                Exception ex = null;
                for (int i = 0, c = 5; i < c; i++)
                {
                    try
                    {
                        _connection = _connectionFactory.CreateConnection();
                        _logger.LogInformation("created conn");
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
                    _logger.LogWarning(ex.ToString());
                    _logger.LogError("FATAL ERROR: RabbitMQ connections could not be created and opened");
                    return false;
                }
                else
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;

                    _logger.LogInformation($"RabbitMQ persistent connection acquired a connection {_connection.Endpoint.HostName} and is subscribed to events");
                    return true;
                }
            }
        }

        void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;
            _logger.LogWarning("A RabbitMQ connection is blocked. Trying to re-connect...");

            TryConnect();
        }

        void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;
            _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");

            TryConnect();
        }

        void OnConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            if (_disposed) return;
            _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");

            TryConnect();
        }
    }
}
