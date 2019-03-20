using System.Threading.Tasks;

namespace SimpleEventBus
{
    public interface IEventHandler<TEvent> : IEventHandler
        where TEvent : IEvent
    {
        Task Handle(TEvent @event);
    }

    public interface IEventHandler 
    { 
        //Task Handle(object @event);
    }
}