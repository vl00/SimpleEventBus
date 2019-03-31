using System;
using System.Threading.Tasks;

namespace SimpleEventBus.InMemory
{
    public class InMemorySubscriptions : ISubscriptions
    {
        public bool IsEmpty => false;

        public bool HasSubscription(Type type) => true;

        public Task Clear() => Task.CompletedTask;

        public Task Subscribe<T>() where T : IEvent
        {
            return Subscribe(typeof(T));
        }

        public Task Subscribe(Type type)
        {
            return Task.CompletedTask;
        }

        public Task Unsubscribe<T>() where T : IEvent
        {
            return Unsubscribe(typeof(T));
        }

        public Task Unsubscribe(Type type)
        {
            return Task.CompletedTask;
        }
    }
}