using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Molca.Modals;
using MolcaSDK.UI;
using MolcaSDK.Media;

namespace MolcaSDK
{
    public class MediaConfirmationModal : BaseModal
    {
        public TextMeshProUGUI titleText;
        public MediaPreviewUI mediaPreview;
        public Button confirmButton;
        public Button cancelButton;

        private Action _onConfirm;
        private Action _onCancel;

        public void Setup(MediaInfo media, string title, Action onConfirm, Action onCancel)
        {
            if (titleText != null) titleText.text = title;
            if (mediaPreview != null && media != null) mediaPreview.LoadPreview(media);
            _onConfirm = onConfirm;
            _onCancel = onCancel;
        }

        private void Awake()
        {
            if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirmClicked);
            if (cancelButton != null) cancelButton.onClick.AddListener(OnCancelClicked);
        }

        private void OnDestroy()
        {
            if (confirmButton != null) confirmButton.onClick.RemoveListener(OnConfirmClicked);
            if (cancelButton != null) cancelButton.onClick.RemoveListener(OnCancelClicked);
        }

        private void OnConfirmClicked()
        {
            _onConfirm?.Invoke();
            Close();
        }

        private void OnCancelClicked()
        {
            _onCancel?.Invoke();
            Close();
        }

        public override void SetNoButtonVisible(bool visible)
        {
            if (cancelButton != null)
                cancelButton.gameObject.SetActive(visible);
        }
    }
} 