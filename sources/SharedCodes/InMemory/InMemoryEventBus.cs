using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleEventBus.InMemory
{
    public class InMemoryEventBus : IEventBus
    {
        public InMemoryEventBus(InMemoryPublisher publisher, InMemorySubscriptions subscriptions)
        {
            Subscriptions = subscriptions;
            Publisher = publisher ?? throw new ArgumentNullException(nameof(Publisher));
        }

        public ISubscriptions Subscriptions { get; }

        public IPublisher Publisher { get; }

        public Task Start()
        {
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            return Task.CompletedTask;
        }
    }
}