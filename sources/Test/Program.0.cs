using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleEventBus;
using SimpleEventBus.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

class XEvent : IEvent
{
    public int I { get; set; }
}

class SEvent : IEvent
{
    public Guid GID { get; set; }
    public DateTime Time { get; set; }
}

class H0 : IEventHandler { }

class H1 : IEventHandler<SEvent>
{
    Task IEventHandler<SEvent>.Handle(SEvent @event)
    {
        Console.WriteLine($"@@event#{@event.GetType().Name}: {this.GetType().Name}");
        return Task.CompletedTask;
    }
}

class H2 : H1, IEventHandler<XEvent>, IEventHandler<SEvent>
{
    Task IEventHandler<XEvent>.Handle(XEvent @event)
    {
        Console.WriteLine($"@@event#{@event.GetType().Name}: {this.GetType().Name}");
        return Task.CompletedTask;
    }

    public Task Handle(SEvent @event)
    {
        Console.WriteLine($"@@event#{@event.GetType().Name}: {this.GetType().Name}");
        return Task.CompletedTask;
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        await Task.CompletedTask;
        var c = new ServiceCollection();

        c.AddSimpleEventBus()
            .AddInMemory(smr => 
            {
                smr.Subscribe<XEvent>();
                smr.Subscribe<SEvent>();
                smr.Subscribe<SEvent>();
            });

        //c.AddScoped<H0>().AddScoped<H1>().AddScoped<H2>();
        c.AddScoped(typeof(IEventHandler<SEvent>), typeof(H1));
        c.AddScoped(typeof(IEventHandler<SEvent>), typeof(H2));
        c.AddScoped(typeof(IEventHandler<XEvent>), typeof(H2));

        var sp = c.BuildServiceProvider() as IServiceProvider;
        var bus = sp.GetService<IEventBus>();

        using (var ctx = sp.CreateScope())
        {
            sp = ctx.ServiceProvider;

            Assert.Same(sp.GetService<IPublisher>(), bus.GetPublisher(sp));

            await bus.GetPublisher(sp).Publish(new XEvent { I = 1000 });
            await sp.GetService<IPublisher>().Publish(new SEvent { GID = Guid.NewGuid(), Time = DateTime.Now });

        }
    }
}