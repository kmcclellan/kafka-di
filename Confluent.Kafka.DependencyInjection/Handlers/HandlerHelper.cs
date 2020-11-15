using System;
using System.Collections.Generic;
using System.Linq;

namespace Confluent.Kafka.DependencyInjection.Handlers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated by container")]
    class HandlerHelper<T>
    {
        readonly IEnumerable<T> handlers;

        public HandlerHelper(IEnumerable<T> handlers)
        {
            this.handlers = handlers;
        }

        public Action<IClient, TEvent>? Resolve<TClient, TEvent>(
            Func<T, Action<IClient, TEvent>> selector,
            Action<TClient, TEvent>? _) =>
                Resolve(selector, (x, y) => x + y);

        public Func<IClient, TEvent, TResult>? Resolve<TClient, TEvent, TResult>(
            Func<T, Func<IClient, TEvent, TResult>> selector,
            Func<TClient, TEvent, TResult>? _) =>
                Resolve(selector, (x, y) => x + y);

        TReturn? Resolve<TReturn>(
            Func<T, TReturn> selector,
            Func<TReturn?, TReturn, TReturn> combine)
            where TReturn : Delegate =>
                handlers.Select(selector).Aggregate(default, combine);
    }
}
