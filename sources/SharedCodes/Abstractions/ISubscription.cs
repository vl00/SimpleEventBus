using System;

namespace SimpleEventBus
{
    public interface ISubscription
    {
        Type Type { get; }
        void Unsubscribe();
    }
}