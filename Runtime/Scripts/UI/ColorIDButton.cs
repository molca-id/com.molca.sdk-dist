using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Molca.ColorID;
using Molca;

namespace MolcaSDK.UI
{
    [RequireComponent(typeof(ColorID))]
    [AddComponentMenu("MolcaSDK/UI/Color ID Button")]
    public class ColorIDButton : Button
    {
        [Header("Color ID Configuration")]
        [SerializeField] private ColorIDReference normalColor = new ColorIDReference("Primary");
        [SerializeField] private ColorIDReference highlightedColor = new ColorIDReference("Secondary");
        [SerializeField] private ColorIDReference pressedColor = new ColorIDReference("Accent");
        [SerializeField] private ColorIDReference selectedColor = new ColorIDReference("Primary");
        [SerializeField] private ColorIDReference disabledColor = new ColorIDReference("Disabled");

        [Header("Toggle Configuration")]
        [SerializeField] private bool isToggleButton = false;
        [SerializeField] private bool isOn = false;
        [SerializeField] private bool excludeFromGroup = false;

        [Header("Events")]
        
        [Header("Toggle Events")]
        public UnityEvent<bool> onToggleChanged;
        public UnityEvent onToggleOn;
        public UnityEvent onToggleOff;

        [Header("Hover Events")]
        public UnityEvent onPointerEnter;
        public UnityEvent onPointerExit;

        private ColorID colorIDComponent;
        private ColorIDButtonGroup buttonGroup;

        public bool IsToggleButton => isToggleButton;
        public bool IsOn 
        { 
            get => isOn; 
            set => SetToggleState(value, true);
        }

        protected override void Awake()
        {
            base.Awake();
            colorIDComponent = GetComponent<ColorID>();
        }

        protected override void Start()
        {
            base.Start();
            transition = Transition.None;
            UpdateColors();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            
            if (isToggleButton && interactable)
            {
                Toggle();
            }
        }

        public void Toggle()
        {
            if (!isToggleButton) return;
            SetToggleState(!isOn, true);
        }

        public void SetToggleState(bool state, bool notifyGroup = true)
        {
            if (isOn == state) return;

            isOn = state;
            
            // Update visual state
            if (isToggleButton)
            {
                SetColor(isOn ? selectedColor : normalColor);
            }
            
            // Notify button group if needed
            if (notifyGroup && buttonGroup != null)
            {
                buttonGroup.OnButtonToggled(this);
            }
            
            // Invoke events
            onToggleChanged?.Invoke(isOn);
            if (isOn)
                onToggleOn?.Invoke();
            else
                onToggleOff?.Invoke();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (interactable)
            {
                SetColor(highlightedColor);
            }
            onPointerEnter?.Invoke();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            if (interactable)
            {
                // Return to appropriate color based on toggle state
                if (isToggleButton && isOn)
                    SetColor(selectedColor);
                else
                    SetColor(normalColor);
            }
            onPointerExit?.Invoke();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (interactable)
            {
                SetColor(pressedColor);
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (interactable)
            {
                SetColor(highlightedColor);
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            if (interactable)
            {
                // For toggle buttons, maintain current state color
                if (!isToggleButton)
                    SetColor(selectedColor);
            }
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            if (interactable)
            {
                // Return to appropriate color based on toggle state
                if (isToggleButton && isOn)
                    SetColor(selectedColor);
                else if (!isToggleButton)
                    SetColor(normalColor);
            }
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            
            if (!interactable)
            {
                SetColor(disabledColor);
            }
            else
            {
                // Restore appropriate color when interactable is re-enabled
                UpdateColors();
            }
        }

        // Override the interactable property to handle color updates
        public new bool interactable
        {
            get => base.interactable;
            set
            {
                if (base.interactable != value)
                {
                    base.interactable = value;
                    
                    // Update colors when interactable state changes
                    if (value)
                    {
                        // Re-enabled - restore appropriate color
                        UpdateColors();
                    }
                    else
                    {
                        // Disabled - set disabled color
                        SetColor(disabledColor);
                    }
                }
            }
        }

        private void SetColor(ColorIDReference colorRef)
        {
            if (colorIDComponent != null)
            {
                colorIDComponent.SetColor(colorRef.SwatchName, colorRef.ColorId);
            }
        }

        private void UpdateColors()
        {
            if (!IsInteractable())
            {
                SetColor(disabledColor);
                return;
            }
            
            if (isToggleButton)
            {
                SetColor(isOn ? selectedColor : normalColor);
            }
            else
            {
                SetColor(normalColor);
            }
        }

        public void SetNormalColor(string colorId)
        {
            normalColor.ColorId = colorId;
            if (currentSelectionState == SelectionState.Normal && !isToggleButton)
            {
                UpdateColors();
            }
        }

        public void SetHighlightedColor(string colorId)
        {
            highlightedColor.ColorId = colorId;
        }

        public void SetPressedColor(string colorId)
        {
            pressedColor.ColorId = colorId;
        }

        public void SetSelectedColor(string colorId)
        {
            selectedColor.ColorId = colorId;
            if (isToggleButton && isOn)
            {
                UpdateColors();
            }
        }

        public void SetDisabledColor(string colorId)
        {
            disabledColor.ColorId = colorId;
        }

        // Button group management
        internal void RegisterWithGroup(ColorIDButtonGroup group)
        {
            buttonGroup = group;
        }

        internal void UnregisterFromGroup()
        {
            buttonGroup = null;
        }

        public bool ExcludeFromGroup => excludeFromGroup;
    }
} 