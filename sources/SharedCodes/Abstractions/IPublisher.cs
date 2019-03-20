using System;
using System.Threading.Tasks;

namespace SimpleEventBus
{
    public interface IPublisher
    {
        Task Publish<T>(T eventEntity) where T : IEvent;
    }
}