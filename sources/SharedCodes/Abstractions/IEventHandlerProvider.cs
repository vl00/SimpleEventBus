using System;
using System.Collections.Generic;

namespace SimpleEventBus
{
    public interface IEventHandlerProvider
    {
        IEnumerable<IEventHandler> GetHandlers(Type type); 

        IEnumerable<IEventHandler<T>> GetHandlers<T>() where T : IEvent;
    }
}