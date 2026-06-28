using Molca;
using Molca.Settings;
using UnityEngine;
using System.Linq;

namespace MolcaSDK.UI
{
    public class UIScaleOption : MonoBehaviour
    {
        [System.Serializable]
        public struct ScaleOption
        {
            public float scale;
            public ButtonState buttonState;
        }

        [SerializeField] private ScaleOption[] scaleOptions;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private async void Start()
        {
            await RuntimeManager.WaitForInitialization();
            await Awaitable.WaitForSecondsAsync(0.1f);

            var scaleModule = GlobalSettings.GetModule<CanvasScaleModule>();
            foreach (var option in scaleOptions)
            {
                if(scaleModule.UIScaleNormalized == option.scale)
                {
                    option.buttonState.SetState(true);
                }
                option.buttonState.onClicked += OnScaleOptionClicked;
            }
        }

        private void OnScaleOptionClicked(ButtonState buttonState)
        {
            var scaleOption = scaleOptions.FirstOrDefault(option => option.buttonState == buttonState);
            if (scaleOption.buttonState == null) return;

            var scaleModule = GlobalSettings.GetModule<CanvasScaleModule>();
            scaleModule.UIScaleNormalized = scaleOption.scale;
        }
    }
}