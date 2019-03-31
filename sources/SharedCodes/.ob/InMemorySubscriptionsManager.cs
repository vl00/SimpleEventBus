using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SimpleEventBus.InMemory
{
    public class InMemorySubscriptionsManager : ISubscriptionsManager
    {
        private readonly ConcurrentDictionary<Type, Subscription> _handlers = new ConcurrentDictionary<Type, Subscription>();

        public event EventHandler<ISubscription> OnSubscriptionAdded;
        public event EventHandler<ISubscription> OnSubscriptionRemoved;

        private void RaiseOnSubAdded(ISubscription subscription) => OnSubscriptionAdded?.Invoke(this, subscription);
        private void RaiseOnSubRemoved(ISubscription subscription) => OnSubscriptionRemoved?.Invoke(this, subscription);

        public bool IsEmpty => _handlers.IsEmpty;

        public bool HasSubscriptions(Type type) => _handlers.ContainsKey(type);

        public void Clear()
        {
            var cns = _handlers.ToArray();
            _handlers.Clear();
            foreach (var d in cns)
                RaiseOnSubRemoved(d.Value);
        }

        public void Subscribe<T>() where T : IEvent
        {
            Subscribe(typeof(T));
        }

        public void Subscribe(Type type)
        {
            if (!typeof(IEvent).IsAssignableFrom(type)) return;

            if (_handlers.TryGetValue(type, out _)) return;

            var d0 = new Subscription(this, type);
            if (_handlers.TryAdd(type, d0))
                RaiseOnSubAdded(d0);
        }

        public void Unsubscribe<T>() where T : IEvent
        {
            Unsubscribe(typeof(T));
        }

        public void Unsubscribe(Type type)
        {
            if (typeof(IEvent).IsAssignableFrom(type) && _handlers.TryGetValue(type, out var sub))
                sub.Unsubscribe();
        }

        public virtual IEnumerable<IEventHandler> GetHandlers(Type type, IServiceProvider serviceProvider)
        {
            if (!HasSubscriptions(type)) return null;
            var handleType = typeof(IEnumerable<>).MakeGenericType(typeof(IEventHandler<>).MakeGenericType(type));
            return (IEnumerable<IEventHandler>)serviceProvider?.GetService(handleType);
        }

        public virtual IEnumerable<IEventHandler<T>> GetHandlers<T>(IServiceProvider serviceProvider) where T : IEvent
        {
            if (!HasSubscriptions(typeof(T))) return null;
            return (IEnumerable<IEventHandler<T>>)serviceProvider?.GetService(typeof(IEnumerable<IEventHandler<T>>));
        }

        class Subscription : ISubscription
        {
            InMemorySubscriptionsManager _this;

            public Type Type { get; }

            public Subscription(InMemorySubscriptionsManager _this, Type type)
            {
                this._this = _this;
                Type = type;
            }

            public void Unsubscribe()
            {
                if (_this == null) return;
                var @this = Interlocked.Exchange(ref _this, null);
                if (@this == null) return;

                if (@this._handlers.TryRemove(Type, out _))
                    @this.RaiseOnSubRemoved(this);
            }
        }
    }
}