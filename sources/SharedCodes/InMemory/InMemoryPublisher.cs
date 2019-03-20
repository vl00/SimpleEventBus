using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleEventBus.InMemory
{
    public class InMemoryPublisher : IPublisher
    {
        readonly IServiceProvider _sp;
        readonly ISubscriptionsManager _subscriptionsManager;

        public InMemoryPublisher(IServiceProvider sp, ISubscriptionsManager subscriptionsManager)
        {
            _sp = sp;
            _subscriptionsManager = subscriptionsManager ?? ((ISubscriptionsManager)sp.GetService(typeof(ISubscriptionsManager)));
        }

        public Task Publish<T>(T eventEntity) where T : IEvent
        {
            var handlers = _subscriptionsManager.GetHandlers<T>(_sp);
            if (handlers == null) return Task.CompletedTask;
            return Task.WhenAll(get_tasks(eventEntity, handlers));
        }

        static IEnumerable<Task> get_tasks<T>(T eventEntity, IEnumerable<IEventHandler<T>> handlers) where T : IEvent
        {
            foreach (var handler in handlers)
            {
                yield return handler.Handle(eventEntity);
            }
        }
    }
}