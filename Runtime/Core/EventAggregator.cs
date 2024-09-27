namespace romanlee17.EventAggregatorRuntime {
    using System;
    using System.Collections.Generic;

    public class EventAggregator : IEventAggregator {

        public static IEventAggregator Create() {
            return new EventAggregator();
        }

        internal static event Action<PublishEvent> OnPublishEvent;
        internal static event Action<ListenerEvent> OnListenerEvent;
        internal static event Action OnBranchCloseEvent;

        // Prevent external instantiation.
        private EventAggregator() {
            lockObject = new();
            eventListenersCollection = new();
        }

        private readonly object lockObject;
        private readonly Dictionary<Type, EventListeners> eventListenersCollection;
        private readonly HashSet<object> eventStack = new();
        private int eventDepth = 0;

        public void Publish<TEvent>(object publisher, TEvent eventData) {
            lock (lockObject) {
                RemoveMissingListeners();
                Type eventType = typeof(TEvent);
                // Debugger publish event.
                PublishEvent publishEvent = new(
                    publisherName: publisher.GetType().Name,
                    eventName: eventType.Name,
                    eventDepth: eventDepth
                );
                OnPublishEvent?.Invoke(publishEvent);
                // Increment depth because of publish event.
                eventDepth++;
                // Publish event for all registered listeners.
                if (eventListenersCollection.ContainsKey(eventType)) {
                    EventListeners eventListeners = eventListenersCollection[eventType];
                    try {
                        // Add event in stack to detect recursive event publishing.
                        if (eventStack.Contains(eventData)) {
                            throw new InvalidOperationException("Recursive event publishing detected.");
                        }
                        eventStack.Add(eventData);
                        // Execute all registered event listeners.
                        foreach (object eventListener in eventListeners) {
                            // Debugger listener event.
                            ListenerEvent listenerEvent = new(
                                eventName: eventType.Name,
                                listenerName: eventListener.GetType().Name,
                                eventDepth: eventDepth
                            );
                            OnListenerEvent?.Invoke(listenerEvent);
                            // Increment depth because of listener event.
                            eventDepth++;
                            // Execute event listener.
                            try {
                                ((IEventListener<TEvent>)eventListener).OnEvent(eventData);
                            }
                            finally {
                                // Decrement depth because of listener event.
                                eventDepth--;
                            }
                        }
                    }
                    finally {
                        // Remove event from stack.
                        eventStack.Remove(eventData);
                    }
                }
                // Decrement depth because of publish event.
                eventDepth--;
                // Debugger branch close event.
                if (eventDepth == 0) {
                    OnBranchCloseEvent?.Invoke();
                }
            }
        }

        public void Subscribe<TEvent>(IEventListener<TEvent> eventListener) {
            lock (lockObject) {
                RemoveMissingListeners();
                Type eventType = typeof(TEvent);
                if (eventListenersCollection.ContainsKey(eventType) == false) {
                    eventListenersCollection[eventType] = new();
                }
                eventListenersCollection[eventType].Add(eventListener);
            }
        }

        public void Unsubscribe<TEvent>(IEventListener<TEvent> eventListener) {
            lock (lockObject) {
                RemoveMissingListeners();
                Type eventType = typeof(TEvent);
                if (eventListenersCollection.ContainsKey(eventType)) {
                    eventListenersCollection[eventType].Remove(eventListener);
                }
            }
        }

        private void RemoveMissingListeners() {
            foreach (EventListeners eventListeners in eventListenersCollection.Values) {
                eventListeners.RemoveAll(eventListener => eventListener == null);
            }
        }

    }

}