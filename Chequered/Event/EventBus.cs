using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Chequered.Event
{
    /// <summary>
    ///   Decouples senders from listeners. This implementation is thread-safe.
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        private readonly ThreadLocal<bool> _areEventsBeingProcessed = new ThreadLocal<bool>(() => false);
        private readonly EventProcessingMode _eventProcessingMode;

        private readonly ConcurrentDictionary<Type, object> _handleCache = new ConcurrentDictionary<Type, object>();

        /// <summary>
        ///   A queue of type Tuple&lt;PendingEvent, Action&lt;PendingEvent&gt;&gt;.
        /// </summary>
        private readonly ThreadLocal<Queue<PendingEvent>> _pendingEventHandlers =
            new ThreadLocal<Queue<PendingEvent>>(() => new Queue<PendingEvent>());

        private readonly Dictionary<Type, Dictionary<uint, object>> _subscriptions =
            new Dictionary<Type, Dictionary<uint, object>>();

        private uint _nextId;

        /// <summary>
        ///   Creates a new EventBus.
        /// </summary>
        /// <param name="eventProcessingMode">
        ///   How this EventBus will handle recursive event invocations. See
        ///   <see cref="EventProcessingMode" />.
        /// </param>
        public EventBus(EventProcessingMode eventProcessingMode = EventProcessingMode.BreadthFirst)
        {
            _eventProcessingMode = eventProcessingMode;
        }

        /// <inheritdoc />
        public IEventHandle<TEvent> GetHandle<TEvent>()
        {
            if (!_handleCache.ContainsKey(typeof(TEvent)))
            {
                _handleCache[typeof(TEvent)] = new EventHandle<TEvent>(this);
            }
            return (IEventHandle<TEvent>) _handleCache[typeof(TEvent)];
        }

        private static void SendDepthFirst(object @event, Action<Exception> onException, List<object> handlers)
        {
            foreach (var handler in handlers)
            {
                ((dynamic) handler).Invoke((dynamic) @event);
            }
        }

        private void Send<TEvent>(TEvent ev)
        {
            Send(ev, exception => { Environment.FailFast("exception occured in event handler", exception); });
        }

        private void Send<TEvent>(TEvent @event, Action<Exception> onException)
        {
            List<object> handlers;
            lock (this)
            {
                handlers = _subscriptions.ContainsKey(typeof(TEvent))
                    ? _subscriptions[typeof(TEvent)].Values.ToList()
                    : new List<object>();
            }
            switch (_eventProcessingMode)
            {
                case EventProcessingMode.BreadthFirst:
                    SendBreadthFirst(@event, onException, handlers);
                    break;
                case EventProcessingMode.DepthFirst:
                    SendDepthFirst(@event, onException, handlers);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private void SendBreadthFirst<TEvent>(TEvent @event, Action<Exception> onException, List<object> handlers)
        {
            foreach (var handler in handlers)
            {
                _pendingEventHandlers.Value.Enqueue(
                    new PendingEvent(@event, handler, onException));
            }
            if (_areEventsBeingProcessed.Value)
            {
                //we only want the bottommost Send() on the callstack to process events
                return;
            }
            _areEventsBeingProcessed.Value = true;
            while (_pendingEventHandlers.Value.Any())
            {
                var pendingEvent = _pendingEventHandlers.Value.Dequeue();
                try
                {
                    ((dynamic) pendingEvent.EventHandler).Invoke((dynamic) pendingEvent.Event);
                }
                catch (Exception ex)
                {
                    pendingEvent.OnException(ex);
                }
            }
        }

        private IDisposable Subscribe<TEvent>(Action<TEvent> callback)
        {
            lock (this)
            {
                if (!_subscriptions.ContainsKey(typeof(TEvent)))
                {
                    _subscriptions[typeof(TEvent)] = new Dictionary<uint, object>();
                }
                var registrationId = _nextId;
                _subscriptions[typeof(TEvent)].Add(_nextId, callback);
                _nextId++;

                return new EventSubscription<TEvent>(registrationId, this);
            }
        }

        private IDisposable SubscribeOnce<TEvent>(Action<TEvent> callback)
        {
            IDisposable r = null;
            r = Subscribe<TEvent>(
                ev =>
                {
                    //We are intentionally modifying this closure to gain access to the return value of Subscribe.
                    // ReSharper disable once AccessToModifiedClosure
                    // ReSharper disable once PossibleNullReferenceException
                    r.Dispose();
                    callback(ev);
                });
            return r;
        }

        private void Unsubscribe<TEvent>(uint registrationId)
        {
            lock (this)
            {
                _subscriptions[typeof(TEvent)].Remove(registrationId);
                if (!_subscriptions[typeof(TEvent)].Any())
                {
                    _subscriptions.Remove(typeof(TEvent));
                }
            }
        }

        private class EventSubscription<TEvent> : IDisposable
        {
            private readonly EventBus _parent;
            private readonly uint _registrationId;
            private bool _isDisposed;

            public EventSubscription(uint registrationId, EventBus parent)
            {
                _registrationId = registrationId;
                _parent = parent;
                _isDisposed = false;
            }

            public void Dispose()
            {
                lock (this)
                {
                    if (_isDisposed)
                    {
                        return;
                    }
                    _isDisposed = true;
                }
                _parent.Unsubscribe<TEvent>(_registrationId);
            }
        }

        private class EventHandle<TEvent> : IEventHandle<TEvent>
        {
            private readonly EventBus _eventBus;

            public EventHandle(EventBus eventBus)
            {
                _eventBus = eventBus;
            }

            public void Send(TEvent ev)
            {
                _eventBus.Send(ev);
            }

            public void Send(TEvent ev, Action<Exception> onException)
            {
                _eventBus.Send(ev, onException);
            }

            public IDisposable Subscribe(Action<TEvent> callback)
            {
                return _eventBus.Subscribe(callback);
            }

            public IDisposable SubscribeOnce(Action<TEvent> callback)
            {
                return _eventBus.SubscribeOnce(callback);
            }
        }

        private class PendingEvent
        {
            public PendingEvent(object @event, object eventHandler, Action<Exception> onException)
            {
                Event = @event;
                EventHandler = eventHandler;
                OnException = onException;
            }

            public object Event { get; }
            public object EventHandler { get; }

            public Action<Exception> OnException { get; }
        }
    }
}