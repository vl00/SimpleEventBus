using System;

namespace SimpleEventBus
{
    public interface IEventNameTypeResolver
    {
        string GetEventName(Type eventType);
        
        Type GetEventType(string eventName);
    }
}