using System;
using System.Threading.Tasks;

namespace SimpleEventBus
{
    //need config??

    public interface IEventBus
    {
        Task Start();

        Task Stop();

        ISubscriptions Subscriptions { get; }

        IPublisher Publisher { get; }
    }
}