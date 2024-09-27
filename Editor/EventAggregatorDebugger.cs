namespace romanlee17.EventAggregatorEditor {
    using romanlee17.EventAggregatorRuntime;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    internal class EventAggregatorDebugger : EditorWindow {

        [SerializeField] private VisualTreeAsset eventAggregatorUXML;
        [SerializeField] private VisualTreeAsset publishEventUXML;
        [SerializeField] private VisualTreeAsset listenerEventUXML;

        [MenuItem("romanlee17/Event Aggregator Debugger")]
        public static void OpenWindow() {
            // Get existing open window or if none, make a new one.
            EventAggregatorDebugger window = GetWindow<EventAggregatorDebugger>();
            window.titleContent = new GUIContent("Event Aggregator Debugger");
            window.minSize = new Vector2(600, 400);
        }

        private void OnEnable() {
            // Listen to all event aggregator class instances events.
            EventAggregator.OnPublishEvent += OnPublishEvent;
            EventAggregator.OnListenerEvent += OnListenerEvent;
            EventAggregator.OnBranchCloseEvent += OnBranchCloseEvent;
        }

        private void OnDisable() {
            // Stop listening to all event aggregator class instances events.
            EventAggregator.OnPublishEvent -= OnPublishEvent;
            EventAggregator.OnListenerEvent -= OnListenerEvent;
            EventAggregator.OnBranchCloseEvent -= OnBranchCloseEvent;
        }

        private void OnPublishEvent(PublishEvent publishEvent) {
            if(createContainer == true) {
                NextEventsContainer();
            }
            VisualElement publishEventRoot = publishEventUXML.Instantiate();
            publishEventRoot.Q<Label>("publisher-name").text = publishEvent.publisherName;
            publishEventRoot.Q<Label>("event-name").text = publishEvent.eventName;
            VisualElement borderLeft = publishEventRoot.Q("border-left-icon");
            if (publishEvent.eventDepth > 0) {
                // Duplicate the border left icon for each depth.
                for (int i = 1; i < publishEvent.eventDepth; i++) {
                    VisualElement borderLeftDepth = new();
                    borderLeftDepth.AddToClassList("border-left-icon");
                    publishEventRoot.Q("publish-event").Insert(0, borderLeftDepth);
                }
            }
            else {
                borderLeft.style.display = DisplayStyle.None;
            }
            EventsContainer.Add(publishEventRoot);
        }

        private void OnListenerEvent(ListenerEvent listenerEvent) {
            VisualElement listenerEventRoot = listenerEventUXML.Instantiate();
            listenerEventRoot.Q<Label>("event-name").text = listenerEvent.eventName;
            listenerEventRoot.Q<Label>("listener-name").text = listenerEvent.listenerName;
            VisualElement borderLeft = listenerEventRoot.Q("border-left-icon");
            if (listenerEvent.eventDepth > 0) {
                // Duplicate the border left icon for each depth.
                for (int i = 1; i < listenerEvent.eventDepth; i++) {
                    VisualElement borderLeftDepth = new();
                    borderLeftDepth.AddToClassList("border-left-icon");
                    listenerEventRoot.Q("publish-event").Insert(0, borderLeftDepth);
                }
            }
            else {
                borderLeft.style.display = DisplayStyle.None;
            }
            EventsContainer.Add(listenerEventRoot);
        }

        private void OnBranchCloseEvent() {
            // Close current branch container, and create
            // a new one with different background color.
            createContainer = true;
        }

        private VisualElement contentRoot = null;
        private VisualElement eventsContainer = null;
        private bool isOddContainer = false;
        private bool createContainer = false;

        private VisualElement EventsContainer {
            get {
                InitializeWindow();
                return eventsContainer;
            }
        }

        private VisualElement ContentRoot {
            get {
                InitializeWindow();
                return contentRoot;
            }
        }

        private void InitializeWindow() {
            if (contentRoot == null) {
                VisualElement windowRoot = eventAggregatorUXML.Instantiate();
                windowRoot.style.flexGrow = 1;
                contentRoot = windowRoot.Q<VisualElement>("content-root");
                // Clear button.
                VisualElement clearButton = windowRoot.Q<VisualElement>("clear-button");
                clearButton.RegisterCallback<ClickEvent>(clickEvent => {
                    contentRoot.Clear();
                    eventsContainer = null;
                    createContainer = true;
                });
                // Add window root to root visual element.
                rootVisualElement.Add(windowRoot);
            }
            if (eventsContainer == null) {
                // Create first events container.
                NextEventsContainer();
            }
        }

        private void NextEventsContainer() {
            if (isOddContainer == false) {
                // Odd by default.
                isOddContainer = true;
                eventsContainer = new();
                eventsContainer.AddToClassList("events-container-odd");
                ContentRoot.Add(eventsContainer);
            }
            else {
                // Create even container.
                isOddContainer = false;
                eventsContainer = new();
                eventsContainer.AddToClassList("events-container-even");
                ContentRoot.Add(eventsContainer);
            }
            createContainer = false;
        }

        public void CreateGUI() {
            InitializeWindow();
        }

    }

}