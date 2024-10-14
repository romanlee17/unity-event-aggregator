namespace romanlee17.EventAggregatorRuntime {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class EventAggregator : IEventAggregator, IDisposable {

        public static IEventAggregator Create(string name = default) {
            // Avoid empty names.
            if (string.IsNullOrEmpty(name) == true) {
                name = $"EventAggregator [{Containers.Count}]";
            }
            // Avoid duplicate names.
            else if (Containers.Any(x => x.Key.Name == name)) {
                int duplicateCount = Containers.Count(x => x.Key.Name.Contains($"{name} ("));
                name = $"{name} ({duplicateCount + 1})";
            }
            // Create new event aggregator.
            return new EventAggregator(name);
        }

        // Event aggregator debugger events.
        internal static event Action OnConstructorEvent;
        internal event Action<EventData> OnPublishEvent;
        internal event Action<EventData> OnListenerEvent;

        // Event aggregator debugger properties.
        internal string Name { get; }
        internal static Dictionary<EventAggregator, EventCollection> Containers { get; } = new();

        // Prevent external instantiation.
        private EventAggregator() { 
            
        }

        private EventAggregator(string name) {
            Name = name;
            lockObject = new();
            eventListenerCollection = new();
            Containers.Add(this, new());
            OnConstructorEvent?.Invoke();
        }

        // Event aggregator runtime fields.
        private readonly object lockObject;
        private readonly Dictionary<Type, EventListeners> eventListenerCollection;
        private readonly HashSet<object> eventStack = new();
        private int eventDepth = 0;

        private void RemoveMissingListeners() {
            foreach (EventListeners eventListeners in eventListenerCollection.Values) {
                eventListeners.RemoveAll(eventListener => eventListener == null);
            }
        }

        #region IEventAggregator

        // TODO: implement delegate to mock and make minimal build version of this method.
        public void Publish<TEvent>(object publisher, TEvent eventData) {
            lock (lockObject) {
                Type eventType = typeof(TEvent);
                RemoveMissingListeners();

                // Event aggregator debugger event.
                EventData publisherEventData = new(
                    eventType: EventType.Publisher,
                    eventSource: publisher.GetType().Name,
                    eventName: eventType.Name,
                    eventDepth: eventDepth
                );
                Containers[this].Add(publisherEventData);
                OnPublishEvent?.Invoke(publisherEventData);

                // Increment depth because of publish event.
                eventDepth++;

                // Publish event for all registered listeners.
                if (eventListenerCollection.ContainsKey(eventType)) {
                    EventListeners eventListeners = eventListenerCollection[eventType];

                    try {
                        // Add event in stack to detect recursive event publishing.
                        if (eventStack.Contains(eventData)) {
                            throw new InvalidOperationException("Recursive event publishing detected.");
                        }
                        eventStack.Add(eventData);

                        // Execute all registered event listeners.
                        foreach (object eventListener in eventListeners) {
                            // Event aggregator debugger event.
                            EventData listenerEventData = new(
                                eventType: EventType.Listener,
                                eventSource: eventListener.GetType().Name,
                                eventName: eventType.Name,
                                eventDepth: eventDepth
                            );
                            Containers[this].Add(listenerEventData);
                            OnListenerEvent?.Invoke(listenerEventData);

                            // Increment depth because of listener event.
                            eventDepth++;

                            // Execute event listener.
                            try {
                                ((IEventListener<TEvent>)eventListener).OnEvent(eventData);
                            }
                            catch (Exception exception) {
                                Debug.LogError(exception);
                            }
                            finally {
                                // Decrement depth because of listener event.
                                eventDepth--;
                            }
                        }
                    }
                    catch (Exception exception) {
                        Debug.LogError(exception);
                    }
                    finally {
                        // Remove event from stack.
                        eventStack.Remove(eventData);
                    }
                }

                // Decrement depth because of publish event.
                eventDepth--;
            }
        }

        public void Subscribe<TEvent>(IEventListener<TEvent> eventListener) {
            lock (lockObject) {
                RemoveMissingListeners();
                // Add event listener to collection.
                Type eventType = typeof(TEvent);
                if (eventListenerCollection.ContainsKey(eventType) == false) {
                    eventListenerCollection[eventType] = new();
                }
                eventListenerCollection[eventType].Add(eventListener);
            }
        }

        public void Unsubscribe<TEvent>(IEventListener<TEvent> eventListener) {
            lock (lockObject) {
                RemoveMissingListeners();
                // Remove event listener from collection.
                Type eventType = typeof(TEvent);
                if (eventListenerCollection.ContainsKey(eventType)) {
                    eventListenerCollection[eventType].Remove(eventListener);
                }
            }
        }

        #endregion

        #region IDisposable

        ~EventAggregator() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing == true) {
                // Release managed resources.
                OnPublishEvent = null;
                OnListenerEvent = null;
                eventListenerCollection.Clear();
                Containers[this].Clear();
                Containers.Remove(this);
            }
            // Release unmanaged resources.
        }

        #endregion

    }

}