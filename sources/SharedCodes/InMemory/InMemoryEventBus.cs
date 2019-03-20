using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleEventBus.InMemory
{
    public class InMemoryEventBus : IEventBus
    {
        IServiceProvider serviceProvider;

        public InMemoryEventBus(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            SubscriptionsManager = ((ISubscriptionsManager)serviceProvider.GetService(typeof(ISubscriptionsManager))) ?? throw new ArgumentNullException(nameof(SubscriptionsManager));
        }

        public ISubscriptionsManager SubscriptionsManager { get; }

        public IPublisher GetPublisher(IServiceProvider serviceProvider = null)
        {
            return (IPublisher)(serviceProvider ?? this.serviceProvider)?.GetService(typeof(IPublisher));
        }

        public Task Start()
        {
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            serviceProvider = null;
            SubscriptionsManager.Clear();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _ = Stop();
        }
    }
}