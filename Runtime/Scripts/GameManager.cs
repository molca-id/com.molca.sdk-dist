using UnityEngine;
using Molca;
using System;
using Molca.Networking.Http;
using Molca.Modals;
using Molca.Localization;

namespace MolcaSDK
{
    public class GameManager : RuntimeSubsystem
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                    Debug.LogError("GameManager is not initialized!");
                return _instance;
            }
        }

        [Header("Connection Settings")]
        [SerializeField] private bool enableConnectionFailConfirmation;
        [SerializeField] private ModalConfirmationHelper connectionFailConfirmation;

        private bool _isInitialized;

        public override void Initialize(Action<IRuntimeSubsystem> finishCallback)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("GameManager is already initialized!");
                finishCallback?.Invoke(this);
                return;
            }

            _instance = this;
            _isInitialized = true;

            SetupEventListeners();

            finishCallback?.Invoke(this);
        }

        private IHttpClient _http;

        // async void shim: subscription must wait for HttpClient's service to exist
        // (the legacy static event tolerated any init order; the instance event
        // needs the live IHttpClient). Body is fully try/caught per async contract.
        private async void SetupEventListeners()
        {
            try
            {
                await RuntimeManager.WaitForInitialization();
                _http = RuntimeManager.GetService<IHttpClient>();
                if (_http != null)
                    _http.ConnectionError += OnConnectionError;
            }
            catch (Exception e)
            {
                Debug.LogError($"GameManager: failed to subscribe to HTTP connection errors: {e.Message}");
            }
        }

        private async void OnConnectionError(string error)
        {
            if (!enableConnectionFailConfirmation || string.IsNullOrEmpty(error))
                return;

            try
            {
                await Awaitable.WaitForSecondsAsync(.2f);
                if (connectionFailConfirmation != null)
                {
                    connectionFailConfirmation.confirmationData.message.SetTextForLanguage(LocalizationManager.DefaultLanguageCode, error);
                    connectionFailConfirmation.Create();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error showing connection failure modal: {e.Message}");
            }
        }

        private void OnDestroy()
        {
            if (_http != null)
                _http.ConnectionError -= OnConnectionError;
            
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}