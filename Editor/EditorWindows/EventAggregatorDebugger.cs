namespace romanlee17.EventAggregatorEditor {
    using romanlee17.EventAggregatorRuntime;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    using static UnityEngine.UI.InputField;
    using EventType = EventAggregatorRuntime.EventType;

    internal class EventAggregatorDebugger : EditorWindow {

        private const string WINDOW_MENU_ITEM = "romanlee17/Event Aggregator Debugger";
        private const string WINDOW_TITLE_TEXT = "Event Aggregator Debugger";

        private const string CONTENT_CONTAINER_ELEMENT = "content-container";
        private const string CLEAR_BUTTON_ELEMENT = "clear-button";

        private const string ODD_CONTAINER_CLASS = "container-odd";
        private const string EVEN_CONTAINER_CLASS = "container-even";

        private const bool DEFAULT_ODD_COUNTER = true;

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
            get => EditorPrefs.GetString($"{WindowId}.{nameof(DropdownChoice)}", string.Empty);
            set => EditorPrefs.SetString($"{WindowId}.{nameof(DropdownChoice)}", value);
        }

        private VisualElement contentContainer;
        private DropdownField dropdownElement;
        private EventAggregator eventAggregator;

        private void OnEnable() {
            // Window content element.
            VisualElement windowContentElement = eventAggregatorUXML.CloneTree();
            windowContentElement.style.flexGrow = 1;

            // Content container element.
            contentContainer = windowContentElement.Q<VisualElement>(CONTENT_CONTAINER_ELEMENT);

            // Dropdown element.
            dropdownElement = windowContentElement.Q<DropdownField>();
            dropdownElement.value = DropdownChoice;
            dropdownElement.RegisterValueChangedCallback(OnDropdownChange);

            // Clear button element.
            VisualElement clearButton = windowContentElement.Q<VisualElement>(CLEAR_BUTTON_ELEMENT);
            clearButton.RegisterCallback<ClickEvent>(OnClearButtonClick);

            rootVisualElement.Add(windowContentElement);

            // Listen to event aggregator constructor events.
            EventAggregator.OnConstructorEvent += OnConstructorEvent;
        }

        private void OnDisable() {
            // Stop listening to event aggregator instance.
            if (eventAggregator != null) {
                eventAggregator.OnPublishEvent -= OnPublishEvent;
                eventAggregator.OnListenerEvent -= OnListenerEvent;
            }
            // Stop listening to event aggregator constructor events.
            EventAggregator.OnConstructorEvent -= OnConstructorEvent;
        }

        private bool oddCounter = DEFAULT_ODD_COUNTER;
        private VisualElement eventContainer;

        private void OnDropdownChange(ChangeEvent<string> changeEvent) {
            DropdownChoice = changeEvent.newValue;
            RestoreContainer(changeEvent.newValue);
        }

        private void OnClearButtonClick(ClickEvent clickEvent) {
            string dropdownChoice = DropdownChoice;
            foreach (EventAggregator eventAggregator in EventAggregator.Containers.Keys) {
                if (eventAggregator.Name != dropdownChoice) continue;
                // Clear event aggregator event collection.
                EventAggregator.Containers[eventAggregator].Clear();
                // Clear content container and reset odd element checker.
                contentContainer.Clear();
                oddCounter = DEFAULT_ODD_COUNTER;
            }
        }

        private void UpdateEventContainer(int eventDepth) {
            if (eventContainer == null) {
                eventContainer = new();
                eventContainer.AddToClassList(ODD_CONTAINER_CLASS);
                contentContainer.Add(eventContainer);
                oddCounter = false;
                return;
            }
            if (eventDepth == 0) {
                eventContainer = new();
                eventContainer.AddToClassList(oddCounter ? ODD_CONTAINER_CLASS : EVEN_CONTAINER_CLASS);
                contentContainer.Add(eventContainer);
                oddCounter = !oddCounter;
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

        private void OnConstructorEvent() {
            dropdownElement.choices = EventAggregator.Containers.Keys.Select(x => x.Name).ToList();
            // Try to restore previous dropdown choice if instance doesn't exist.
            if (eventAggregator == null) {
                // Try to select last console container chosen.
                RestoreContainer(DropdownChoice);
            }
        }

        private void RestoreContainer(string eventAggregatorName) {
            contentContainer.Clear();
            oddCounter = DEFAULT_ODD_COUNTER;
            // Stop listening to previous event aggregator instance.
            if (eventAggregator != null) {
                eventAggregator.OnPublishEvent -= OnPublishEvent;
                eventAggregator.OnListenerEvent -= OnListenerEvent;
            }
            // Update event aggregator instance.
            foreach (EventAggregator eventAggregator in EventAggregator.Containers.Keys) {
                if (eventAggregator.Name != eventAggregatorName) continue;
                this.eventAggregator = eventAggregator;
                eventAggregator.OnPublishEvent += OnPublishEvent;
                eventAggregator.OnListenerEvent += OnListenerEvent;
            }
            // Recreate event container based on new event aggregator instance.
            foreach (EventData eventData in EventAggregator.Containers[this.eventAggregator]) {
                UpdateEventContainer(eventData.EventDepth);
                if (eventData.EventType == EventType.Publisher) {
                    eventContainer.Add(new PublisherElement(publisherUXML) {
                        PublisherName = eventData.EventSource,
                        EventName = eventData.EventName,
                        EventDepth = eventData.EventDepth
                    });
                }
                else {
                    eventContainer.Add(new ListenerElement(listenerUXML) {
                        EventName = eventData.EventName,
                        ListenerName = eventData.EventSource,
                        EventDepth = eventData.EventDepth
                    });
                }
            }
        }

    }

}