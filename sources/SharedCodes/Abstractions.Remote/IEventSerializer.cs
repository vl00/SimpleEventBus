using System;

namespace SimpleEventBus
{
    public interface IEventSerializer<TRaw>
    {
        TRaw Serialize(object eventEntity);

        object Deserialize(Type eventType, TRaw rawEvent);
    }
}