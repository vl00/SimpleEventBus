using Newtonsoft.Json;
using System;
using System.Text;

namespace SimpleEventBus
{
    public class JsonEventSerializer : IEventSerializer<byte[]>
    {
        public byte[] Serialize(object eventEntry)
        {
            var es = JsonConvert.SerializeObject(eventEntry);
            return Encoding.UTF8.GetBytes(es);
        }

        public object Deserialize(Type eventType, byte[] @event)
        {
            var es = Encoding.UTF8.GetString(@event);
            return JsonConvert.DeserializeObject(es, eventType);
        }
    }
}