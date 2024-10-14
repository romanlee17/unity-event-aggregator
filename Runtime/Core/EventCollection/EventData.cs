namespace romanlee17.EventAggregatorRuntime {

    internal readonly struct EventData {

        private const int DEFAULT_DEPTH = 0;

        public EventData(EventType eventType, string eventSource, string eventName) {
            EventType = eventType;
            EventSource = eventSource;
            EventName = eventName;
            EventDepth = DEFAULT_DEPTH;
        }

        public EventData(EventType eventType, string eventSource, string eventName, int eventDepth) {
            EventType = eventType;
            EventSource = eventSource;
            EventName = eventName;
            EventDepth = eventDepth;
        }

        public EventType EventType { get; }
        public string EventSource { get; }
        public string EventName { get; }
        public int EventDepth { get; }

    }

}