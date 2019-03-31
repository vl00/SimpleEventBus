using System;
using System.Collections.Generic;

class ExchangeOption
{
    public string Name { get; set; }
    public string Type { get; set; }
    public bool Durable { get; set; }
    public bool AutoDelete { get; set; }
    public IDictionary<string, object> Arguments { get; set; }
}

class QueueOption
{
    public string Name { get; set; }
    public bool Durable { get; set; }
    public bool Exclusive { get; set; }
    public bool AutoDelete { get; set; }
    public IDictionary<string, object> Arguments { get; set; }
}

class DelayMQueueOption
{
    public ExchangeOption TtlDlxExchange { get; set; }
    public QueueOption TtlDlxQueue { get; set; }
    public ExchangeOption FallbackExchange { get; set; }
    public QueueOption FallbackQueue { get; set; }
}