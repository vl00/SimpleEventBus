using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SimpleEventBus;
using SimpleEventBus.InMemory;
using SimpleEventBus.RabbitMQ;
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

class H2 : IEventHandler<XEvent>, IEventHandler<SEvent>
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
    static ILogger log;

    static void Pause()
    {
        Console.WriteLine("{{pause...}}");
        Console.ReadLine();
    }

    static async Task Main(string[] args)
    {
        var cmd = args.Length > 0 ? args[0] : null;

        await Task.CompletedTask;
        var c = new ServiceCollection();

        c.AddLogging(_ => _.AddConsole());

        c.AddSimpleEventBus()
            .AddEventNameTypeResolver()
            .AddEventReceivedFunc(OnHandleReceiverd)
            .AddEventSerializer<byte[]>(_ => new JsonEventSerializer())
            .AddSubscribeEventFunc(() => new[]
            {
                typeof(XEvent),
                typeof(SEvent), typeof(SEvent),
            })
            .AddRabbitMQ(() =>
            {
                var cff = new RabbitMQ.Client.ConnectionFactory();
                //cff.AutomaticRecoveryEnabled = false; 
                //cff.TopologyRecoveryEnabled = false;
                cff.UserName = "root";
                cff.Password = "root";
                return cff;
            });

        //c.AddScoped<H0>().AddScoped<H1>().AddScoped<H2>();
        c.AddScoped(typeof(IEventHandler<SEvent>), typeof(H1));
        c.AddScoped(typeof(IEventHandler<SEvent>), typeof(H2));
        c.AddScoped(typeof(IEventHandler<XEvent>), typeof(H2));

        var sp = c.BuildServiceProvider() as IServiceProvider;
        log = sp.GetService<ILoggerFactory>().CreateLogger("123");
        var bus = sp.GetService<RabbitMQEventBus>();
        await bus.Start();

        switch (cmd)
        {
            case "c":
                { 
                    log.LogInformation("run c");  
                    Pause();
                }
                break;
            case "p":
                { 
                    log.LogInformation("run p");  
                    Pause();
                    await bus.Publisher.Publish(new SEvent { GID = Guid.NewGuid(), Time = DateTime.Now });
                    Pause();
                    await bus.Publisher.Publish(new XEvent { I = 1000 });
                    Pause();
                }
                break;
        }

        (sp as IDisposable)?.Dispose(); //??can't dispose??

        Console.WriteLine("{{should end...}}");
    }

    static Task OnHandleReceiverd(Type type, object entity, IEnumerable<IEventHandler> handlers)
    {
        return Task.WhenAll(_());

        IEnumerable<Task> _()
        {
            foreach (var handler in handlers)
            {
                if (type == typeof(XEvent))
                {
                    yield return ((IEventHandler<XEvent>)handler).Handle((XEvent)entity);
                }
                else if (type == typeof(SEvent))
                {
                    yield return ((IEventHandler<SEvent>)handler).Handle((SEvent)entity);
                }
            }
        }
    }
}