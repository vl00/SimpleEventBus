using System;
using System.Threading.Tasks;

namespace SimpleEventBus
{
    //need config??

    public interface IEventBus : IDisposable
    {
        Task Start();

        Task Stop();

        ISubscriptionsManager SubscriptionsManager { get; }

        IPublisher GetPublisher(IServiceProvider serviceProvider = null);
    }
}