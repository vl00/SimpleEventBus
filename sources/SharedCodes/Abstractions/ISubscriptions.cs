using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleEventBus
{
    public delegate IEnumerable<Type> SubscribeEventFunc();

    public interface ISubscriptions
    {    
        Task Subscribe<T>() where T : IEvent;
        Task Subscribe(Type type);
        Task Unsubscribe<T>() where T : IEvent;
        Task Unsubscribe(Type type);

        bool HasSubscription(Type type);

        Task Clear();

        bool IsEmpty { get; }
    }
}