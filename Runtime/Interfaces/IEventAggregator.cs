namespace romanlee17.EventAggregatorRuntime {

    /// <summary>Defines a mechanism for components to communicate with each other through loosely coupled events.
    /// Allows for publishing events to subscribers, and for subscribing or unsubscribing to events.</summary>
    public interface IEventAggregator {

        /// <summary>Publishes an event to all subscribers of the event type.</summary>
        /// <param name="publisher">Publisher of the event.</param>
        /// <typeparam name="TEvent">Type of event to publish.</typeparam>
        /// <param name="eventData">Event instance to publish.</param>
        public void Publish<TEvent>(object publisher, TEvent eventData);

        /// <summary>Subscribes a subscriber to a specific type of event.</summary>
        /// <typeparam name="TEvent">The type of event to subscribe to.</typeparam>
        /// <param name="eventListener">The subscriber to register.</param>
        public void Subscribe<TEvent>(IEventListener<TEvent> eventListener);

        /// <summary>Unsubscribes a subscriber from a specific type of event.</summary>
        /// <typeparam name="TEvent">The type of event to unsubscribe from.</typeparam>
        /// <param name="eventListener">The subscriber to unregister.</param>
        public void Unsubscribe<TEvent>(IEventListener<TEvent> eventListener);

        /// <summary>Clear all events in container and dipose it.</summary>
        public void Dispose();

    }

}