using Molca;
using Molca.ColorID;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

namespace MolcaSDK.UI
{
    /// <summary>
    /// Dropdown UI component for switching between color schemes (e.g., Light/Dark mode).
    /// </summary>
    [RequireComponent(typeof(TMP_Dropdown))]
    public class ColorSchemeDropdown : MonoBehaviour
    {
        [Tooltip("Optional icons for each scheme. Array index should match scheme index in ColorSchemeManager.")]
        [SerializeField] private Sprite[] schemeIcons;

        private TMP_Dropdown dropdown;
        private IColorSchemeService _schemes;

        private async void Start()
        {
            await RuntimeManager.WaitForInitialization();

            dropdown = GetComponent<TMP_Dropdown>();
            _schemes = RuntimeManager.GetService<IColorSchemeService>();
            if (_schemes == null)
            {
                Debug.LogWarning("ColorSchemeDropdown: IColorSchemeService not available.");
                return;
            }

            if (_schemes.SchemeCount == 0)
            {
                Debug.LogWarning("ColorSchemeDropdown: No color schemes available in ColorSchemeManager.");
                return;
            }

            PopulateDropdown();

            dropdown.onValueChanged.AddListener(OnSchemeChanged);

            // Subscribe to external scheme changes to keep dropdown in sync
            _schemes.SchemeChanged += OnExternalSchemeChanged;
        }

        private void OnDestroy()
        {
            if (_schemes != null)
                _schemes.SchemeChanged -= OnExternalSchemeChanged;
        }

        private void PopulateDropdown()
        {
            var schemeNames = _schemes.SchemeNames;
            var options = new List<TMP_Dropdown.OptionData>();

            for (int i = 0; i < schemeNames.Length; i++)
            {
                Sprite icon = (schemeIcons != null && i < schemeIcons.Length) ? schemeIcons[i] : null;
                options.Add(new TMP_Dropdown.OptionData(schemeNames[i], icon, Color.white));
            }

            dropdown.options = options;
            dropdown.value = _schemes.ActiveSchemeIndex;
            dropdown.RefreshShownValue();
        }

        private void OnSchemeChanged(int index)
        {
            _schemes.SetScheme(index);
        }

        /// <summary>
        /// Called when the scheme is changed externally (e.g., via code or another UI).
        /// Keeps the dropdown in sync.
        /// </summary>
        private void OnExternalSchemeChanged(ColorModule newScheme)
        {
            if (dropdown != null && dropdown.value != _schemes.ActiveSchemeIndex)
            {
                dropdown.SetValueWithoutNotify(_schemes.ActiveSchemeIndex);
                dropdown.RefreshShownValue();
            }
        }
    }
}
