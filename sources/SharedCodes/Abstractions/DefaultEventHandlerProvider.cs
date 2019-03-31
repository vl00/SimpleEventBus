using System;
using System.Collections.Generic;

namespace SimpleEventBus
{
    public class DefaultEventHandlerProvider : IEventHandlerProvider
    {
        readonly IServiceProvider services;

        public DefaultEventHandlerProvider(IServiceProvider services) => this.services = services;

        public IEnumerable<IEventHandler> GetHandlers(Type type)
        {
            var handleType = typeof(IEnumerable<>).MakeGenericType(typeof(IEventHandler<>).MakeGenericType(type));
            return (IEnumerable<IEventHandler>)services.GetService(handleType);
        }

        public IEnumerable<IEventHandler<T>> GetHandlers<T>() where T : IEvent
        {
            return (IEnumerable<IEventHandler<T>>)services.GetService(typeof(IEnumerable<IEventHandler<T>>));
        }
    }
}