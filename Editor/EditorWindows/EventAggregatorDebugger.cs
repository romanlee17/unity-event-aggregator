namespace romanlee17.EventAggregatorEditor {
    using romanlee17.EventAggregatorRuntime;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    internal class EventAggregatorDebugger : EditorWindow {

        private const string WINDOW_MENU_ITEM = "romanlee17/Event Aggregator Debugger";
        private const string WINDOW_TITLE_TEXT = "Event Aggregator Debugger";

        private const string CONTENT_CONTAINER_ELEMENT = "content-container";
        private const string CLEAR_BUTTON_ELEMENT = "clear-button";

        private const string ODD_CONTAINER_CLASS = "container-odd";
        private const string EVEN_CONTAINER_CLASS = "container-even";

        [SerializeField] private Texture2D windowDarkIcon;
        [SerializeField] private Texture2D windowLightIcon;

        [SerializeField] private VisualTreeAsset eventAggregatorUXML;
        [SerializeField] private VisualTreeAsset publisherUXML;
        [SerializeField] private VisualTreeAsset listenerUXML;

        [MenuItem(WINDOW_MENU_ITEM)]
        public static void Open() {
            EventAggregatorDebugger window = CreateWindow<EventAggregatorDebugger>();
            window.titleContent.text = WINDOW_TITLE_TEXT;
            if (EditorGUIUtility.isProSkin == true) {
                // Dark mode is active so use the light icon.
                window.titleContent.image = window.windowLightIcon;
            }
            else {
                // Light mode is active so use the dark icon.
                window.titleContent.image = window.windowDarkIcon;
            }
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private string WindowId {
            get => $"{GetType().Name}.{GetInstanceID()}";
        }

        private string DropdownChoice {
            get => EditorPrefs.GetString($"{WindowId}.{DropdownChoice}", string.Empty);
            set => EditorPrefs.SetString($"{WindowId}.{DropdownChoice}", value);
        }

        private VisualElement contentContainer;
        private DropdownField eventAggregatorDropdown;
        private readonly List<string> dropdownChoices = new();

        private void OnEnable() {
            VisualElement eventAggregatorElement = eventAggregatorUXML.CloneTree();
            eventAggregatorElement.style.flexGrow = 1;
            contentContainer = eventAggregatorElement.Q<VisualElement>(CONTENT_CONTAINER_ELEMENT);
            eventAggregatorDropdown = eventAggregatorElement.Q<DropdownField>();
            rootVisualElement.Add(eventAggregatorElement);
            // Listen to all event aggregator class instances events.
            EventAggregator.OnPublishEvent += OnPublishEvent;
            EventAggregator.OnListenerEvent += OnListenerEvent;
        }

        private void OnDisable() {
            // Stop listening to all event aggregator class instances events.
            EventAggregator.OnPublishEvent -= OnPublishEvent;
            EventAggregator.OnListenerEvent -= OnListenerEvent;
        }

        private void CreateGUI() {


            /*VisualElement clearButton = eventAggregatorTree.Q<VisualElement>(CLEAR_BUTTON_ELEMENT);
            clearButton.RegisterCallback<ClickEvent>(callback => {
                // TODO: implement this.
            });*/
        }

        private bool isOddContainer = true;
        private VisualElement eventContainer;

        private void UpdateEventContainer(int eventDepth) {
            if (eventContainer == null) {
                eventContainer = new();
                eventContainer.AddToClassList(ODD_CONTAINER_CLASS);
                contentContainer.Add(eventContainer);
                isOddContainer = false;
                return;
            }
            if (eventDepth == 0) {
                eventContainer = new();
                eventContainer.AddToClassList(isOddContainer ? ODD_CONTAINER_CLASS : EVEN_CONTAINER_CLASS);
                contentContainer.Add(eventContainer);
                isOddContainer = !isOddContainer;
            }
        }

        private void OnPublishEvent(EventData listenerEventData) {
            UpdateEventContainer(listenerEventData.EventDepth);
            eventContainer.Add(new PublisherElement(publisherUXML) {
                PublisherName = listenerEventData.EventSource,
                EventName = listenerEventData.EventName,
                EventDepth = listenerEventData.EventDepth
            });
        }

        private void OnListenerEvent(EventData publisherEventData) {
            UpdateEventContainer(publisherEventData.EventDepth);
            eventContainer.Add(new ListenerElement(listenerUXML) {
                EventName = publisherEventData.EventName,
                ListenerName = publisherEventData.EventSource,
                EventDepth = publisherEventData.EventDepth
            });
        }

        /*private void RefreshDropdownChoices() {
            // Clear the dropdown choices.
            dropdownChoices.Clear();
            // Add all console container names to the dropdown choices.
            foreach (string name in EventAggregator.Instances.Keys.Select(x => x.name)) {
                dropdownChoices.Add(name);
            }
        }*/

    }

}