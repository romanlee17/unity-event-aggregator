namespace romanlee17.EventAggregatorRuntime {

    /// <summary>Defines a contract for subscribers that handle specific types of events.</summary>
    /// <typeparam name="TEvent">The type of event to be handled by the subscriber.</typeparam>
    public interface IEventListener<TEvent> {

        /// <summary>Handles an event of the subscribed type.</summary>
        /// <param name="eventData">The event instance to handle.</param>
        public void OnEvent(TEvent eventData);

    }

}