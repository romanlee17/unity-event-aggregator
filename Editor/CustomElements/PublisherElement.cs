namespace romanlee17.EventAggregatorEditor {
    using UnityEngine.UIElements;

    internal class PublisherElement : VisualElement {

        public PublisherElement(VisualTreeAsset visualTreeAsset) {
            visualTreeAsset.CloneTree(this);
            publisherNameLabel = this.Q<Label>("publisher-name");
            eventNameLabel = this.Q<Label>("event-name");
            eventContentElement = this.Q<VisualElement>("event-content");
        }

        private readonly VisualElement eventContentElement;
        private readonly Label publisherNameLabel;
        private readonly Label eventNameLabel;

        public string PublisherName {
            get => publisherNameLabel.text;
            set => publisherNameLabel.text = value;
        }

        public string EventName {
            get => eventNameLabel.text;
            set => eventNameLabel.text = value;
        }

        public int EventDepth {
            get => borderLeftCount;
            set => CreateBorderElements(value);
        }

        private int borderLeftCount = 0;

        private void CreateBorderElements(int count) {
            // Remove all elements with border left class.
            eventContentElement.Query<VisualElement>("border-left").ForEach(x => x.RemoveFromHierarchy());
            // Add border left elements 'count' number of times.
            for (int x = 0; x < count; x++) {
                VisualElement borderLeftElement = new();
                borderLeftElement.AddToClassList("border-left");
                eventContentElement.Insert(0, borderLeftElement);
            }
            borderLeftCount = count;
        }

    }

}