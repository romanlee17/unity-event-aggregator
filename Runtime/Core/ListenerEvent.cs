namespace romanlee17.EventAggregatorRuntime {

    internal readonly struct ListenerEvent {

        public readonly string eventName;
        public readonly string listenerName;
        public readonly int eventDepth;

        public ListenerEvent(string eventName, string listenerName, int eventDepth) {
            this.eventName = eventName;
            this.listenerName = listenerName;
            this.eventDepth = eventDepth;
        }

    }

}