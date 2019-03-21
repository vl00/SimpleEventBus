## SimpleEventBus

简单的事件总线实现。目前支持 单一进程内 和 rabbitmq 。

主要接口：
```csharp
namespace SimpleEventBus
{
    //事件实体需要实现此接口
    public interface IEvent { }

    //事件处理handler需要实现此接口
    public interface IEventHandler<TEvent> : IEventHandler
        where TEvent : IEvent
    {
        Task Handle(TEvent @event);
    }
    
    //事件发布者
    public interface IPublisher
    {
        Task Publish<T>(T eventEntity) where T : IEvent;
    } 
    
    //订阅管理，eventhandler都由IoC/DI提供，这里负责订阅事件
    public interface ISubscriptionsManager
    {   
        //event handlers are registerd by IoC/DI
        void Subscribe<T>() where T : IEvent; 
        void Subscribe(Type type);
        void Unsubscribe<T>() where T : IEvent;
        void Unsubscribe(Type type);
        
        //remote时使用
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
```
