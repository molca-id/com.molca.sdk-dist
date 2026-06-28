using Molca;
using Molca.ColorID;
using UnityEngine;
using UnityEngine.UI;

namespace MolcaSDK.UI
{
    /// <summary>
    /// Simple toggle button for switching between color schemes.
    /// Ideal for Light/Dark mode switching with just 2 schemes.
    /// </summary>
    public class ColorSchemeToggle : MonoBehaviour
    {
        [Header("Optional Visual Feedback")]
        [Tooltip("Optional image to show the current scheme icon.")]
        [SerializeField] private Image schemeIcon;
        
        [Tooltip("Icons for each scheme. Array index should match scheme index in ColorSchemeManager.")]
        [SerializeField] private Sprite[] schemeIcons;

        [Header("Button")]
        [Tooltip("The button to use for toggling. If not set, will try to get from this GameObject.")]
        [SerializeField] private Button toggleButton;

        private IColorSchemeService _schemes;

        private async void Start()
        {
            await RuntimeManager.WaitForInitialization();

            _schemes = RuntimeManager.GetService<IColorSchemeService>();
            if (_schemes == null)
            {
                Debug.LogWarning("ColorSchemeToggle: IColorSchemeService not available.");
                return;
            }

            if (toggleButton == null)
                toggleButton = GetComponent<Button>();

            if (toggleButton != null)
                toggleButton.onClick.AddListener(OnToggleClicked);

            // Subscribe to scheme changes to update visuals
            _schemes.SchemeChanged += OnSchemeChanged;

            // Set initial visual state
            UpdateVisuals();
        }

        private void OnDestroy()
        {
            if (_schemes != null)
                _schemes.SchemeChanged -= OnSchemeChanged;
        }

        private void OnToggleClicked()
        {
            _schemes?.ToggleScheme();
        }

        private void OnSchemeChanged(ColorModule newScheme)
        {
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (schemeIcon != null && schemeIcons != null)
            {
                if (_schemes == null) return;
                int index = _schemes.ActiveSchemeIndex;
                if (index >= 0 && index < schemeIcons.Length && schemeIcons[index] != null)
                {
                    schemeIcon.sprite = schemeIcons[index];
                }
            }
        }
    }
}
