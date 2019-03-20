using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleEventBus
{
    public delegate Task EventReceivedFunc(Type type, object entity, IEnumerable<IEventHandler> handlers);
}