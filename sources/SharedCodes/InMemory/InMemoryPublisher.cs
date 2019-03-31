using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleEventBus.InMemory
{
    public class InMemoryPublisher : IPublisher
    {
        readonly IEventHandlerProvider _eventHandlerProvider;

        public InMemoryPublisher(IEventHandlerProvider eventHandlerProvider)
        {
            _eventHandlerProvider = eventHandlerProvider;
        }

        public Task Publish<T>(T eventEntity) where T : IEvent
        {
            var handlers = _eventHandlerProvider.GetHandlers<T>();
            return handlers == null ? Task.CompletedTask : Task.WhenAll(get_tasks(eventEntity, handlers));
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