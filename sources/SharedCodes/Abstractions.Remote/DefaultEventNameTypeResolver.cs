using System;

namespace SimpleEventBus
{
    public class DefaultEventNameTypeResolver : IEventNameTypeResolver
    {
        public string GetEventName(Type eventType) => eventType?.FullName;

        public Type GetEventType(string eventName) => Type.GetType(eventName);
    }
}