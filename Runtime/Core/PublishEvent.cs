namespace romanlee17.EventAggregatorRuntime {

    internal readonly struct PublishEvent {

        public readonly string publisherName;
        public readonly string eventName;
        public readonly int eventDepth;

        public PublishEvent(string publisherName, string eventName, int eventDepth) {
            this.publisherName = publisherName;
            this.eventName = eventName;
            this.eventDepth = eventDepth;
        }

    }

}