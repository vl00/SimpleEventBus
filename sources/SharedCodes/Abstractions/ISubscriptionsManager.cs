using System;
using System.Collections.Generic;

namespace SimpleEventBus
{
    public interface ISubscriptionsManager
    {
        //no-need:ISubscription Subscribe<T>(Func<IServiceProvider, T> handler) where T : IEventHandler;     
        void Subscribe<T>() where T : IEvent; //event handlers are registerd by IoC/DI
        void Subscribe(Type type);
        void Unsubscribe<T>() where T : IEvent;
        void Unsubscribe(Type type);

        event EventHandler<ISubscription> OnSubscriptionAdded;
        event EventHandler<ISubscription> OnSubscriptionRemoved;

        //enable changed for remote
        IEnumerable<IEventHandler> GetHandlers(Type type, IServiceProvider serviceProvider); 
        IEnumerable<IEventHandler<T>> GetHandlers<T>(IServiceProvider serviceProvider) where T : IEvent;

        bool HasSubscriptions(Type type);

        void Clear();

        bool IsEmpty { get; }
    }
}