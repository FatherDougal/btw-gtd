﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Btw.Portable;
using Gtd.CoreDomain;

namespace Gtd.Client
{
    public sealed class AppEventStore : IEventStore
    {
        readonly MessageStore _store;
        IHandle<Message> _handler;

        public AppEventStore(MessageStore store)
        {
            _store = store;
        }

        public void SetDispatcher(IHandle<Message> dispatcher)
        {
            _handler = dispatcher;
        }

        public void AppendEventsToStream(string name, long streamVersion, ICollection<Event> events)
        {
            if (events.Count == 0) return;
            // functional events don't have an identity

            try
            {
                _store.AppendToStore(name, streamVersion, events.Cast<object>().ToArray());
            }
            catch (AppendOnlyStoreConcurrencyException e)
            {
                // load server events
                var server = LoadEventStream(name);
                // throw a real problem
                throw OptimisticConcurrencyException.Create(server.StreamVersion, e.ExpectedStreamVersion, name, server.Events);
            }
            // sync handling. Normally we would push this to async
            foreach (var @event in events)
            {
                _handler.Handle(@event);
            }
        }
        public EventStream LoadEventStream(string name)
        {
            var list = new List<Event>();
            var streamVersion = 0L;
            foreach (var record in _store.EnumerateMessages(name, 0, int.MaxValue))
            {
                list.AddRange(record.Items.Cast<Event>());
                streamVersion = record.StreamVersion;
            }
            return new EventStream(streamVersion, new ReadOnlyCollection<Event>(list));
        }
    }
}