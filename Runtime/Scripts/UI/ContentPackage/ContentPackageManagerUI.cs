using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MolcaSDK.UI;
using Molca;
using Molca.ContentPackage;
using Molca.ContentPackage.Core;
using Molca.ContentPackage.Services;
using Molca.ContentPackage.Utilities;
using Molca.Settings;

namespace MolcaSDK.UI.ContentPackage
{
    /// <summary>
    /// Runtime UI for browsing, installing, and uninstalling content packages.
    /// Attach to a Canvas root. Wire all serialized references in the Inspector.
    /// The <see cref="ColorIDButtonGroup"/> on <see cref="_listContainer"/> must be
    /// configured as radio (single selection, <c>allowSwitchOff = false</c>).
    /// </summary>
    public class ContentPackageManagerUI : MonoBehaviour
    {
        // ── Header ───────────────────────────────────────────────────────────

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private ColorIDButton   _refreshButton;
        [SerializeField] private TextMeshProUGUI _refreshButtonLabel;
        [SerializeField] private TextMeshProUGUI _statusLabel;

        // ── Package list (left panel) ────────────────────────────────────────

        [Header("Package List")]
        [SerializeField] private Transform           _listContainer;
        [SerializeField] private ColorIDButtonGroup  _listButtonGroup;
        [SerializeField] private PackageListItemUI   _listItemPrefab;
        [SerializeField] private TextMeshProUGUI     _emptyLabel;

        // ── Detail panel (right panel) ────────────────────────────────────────

        [Header("Detail Panel")]
        [SerializeField] private GameObject      _detailPanel;
        [SerializeField] private TextMeshProUGUI _detailName;
        [SerializeField] private TextMeshProUGUI _detailId;
        [SerializeField] private TextMeshProUGUI _detailVersion;
        [SerializeField] private TextMeshProUGUI _detailDescription;
        [SerializeField] private TextMeshProUGUI _detailSize;
        [SerializeField] private TextMeshProUGUI _detailDownloadSize;
        [SerializeField] private TextMeshProUGUI _detailTags;
        [SerializeField] private TextMeshProUGUI _detailChangelog;
        [SerializeField] private GameObject      _detailChangelogRow;
        [SerializeField] private TextMeshProUGUI _detailStatus;
        [SerializeField] private TextMeshProUGUI _detailDependencies;
        [SerializeField] private TextMeshProUGUI _detailUsedBy;
        [SerializeField] private TextMeshProUGUI _detailErrorMessage;
        [SerializeField] private GameObject      _errorRow;

        // ── Progress ──────────────────────────────────────────────────────────

        [Header("Progress")]
        [SerializeField] private GameObject      _progressRow;
        [SerializeField] private Slider          _progressSlider;
        [SerializeField] private TextMeshProUGUI _progressLabel;

        // ── Action buttons ────────────────────────────────────────────────────

        [Header("Action Buttons")]
        [SerializeField] private ColorIDButton   _installButton;
        [SerializeField] private TextMeshProUGUI _installButtonLabel;
        [SerializeField] private ColorIDButton   _uninstallButton;
        [SerializeField] private ColorIDButton   _updateAllButton;
        [SerializeField] private TextMeshProUGUI _updateAllButtonLabel;
        [SerializeField] private ColorIDButton   _cancelButton;
        [SerializeField] private ColorIDButton   _freeUpSpaceButton;
        [SerializeField] private TextMeshProUGUI _freeUpSpaceButtonLabel;

        // ── Footer ────────────────────────────────────────────────────────────

        [Header("Footer")]
        [SerializeField] private TextMeshProUGUI _footerInstalledSizeLabel;
        [SerializeField] private TextMeshProUGUI _footerInstalledCountLabel;

        // ── Status colors ─────────────────────────────────────────────────────

        [Header("Status Colors")]
        [SerializeField] private Color _colorAvailable   = new Color(0.55f, 0.55f, 0.55f);
        [SerializeField] private Color _colorDownloading = new Color(1f,    0.75f, 0f);
        [SerializeField] private Color _colorInstalled   = new Color(0.1f,  0.8f,  0.1f);
        [SerializeField] private Color _colorFailed      = new Color(1f,    0.25f, 0.25f);
        [SerializeField] private Color _colorUpdate      = new Color(0.3f,  0.7f,  1f);

        // ── Internal state ────────────────────────────────────────────────────

        [Inject] private PackageSubsystem _packageSubsystem;

        private PackageService         _service;
        private ContentPackageSettings _settings;

        private readonly List<PackageListItemUI> _items = new List<PackageListItemUI>();
        private string _selectedPackageId;

        // Per-operation tokens — each flow owns its own so they don't stomp each other.
        private CancellationTokenSource _installCts;
        private CancellationTokenSource _refreshCts;
        private CancellationTokenSource _downloadSizeCts;
        private CancellationTokenSource _freeUpSpaceCts;

        private bool _refreshing;
        private bool _updatingAll;
        private bool _freeingSpace;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private async void Start()
        {
            try
            {
                await RuntimeManager.WaitForInitialization();

                _service  = _packageSubsystem?.PackageService;
                _settings = GlobalSettings.GetModule<ContentPackageSettings>();

                if (_service == null)
                {
                    SetStatus("PackageService unavailable.", _colorFailed);
                    return;
                }

                SubscribeEvents();

                if (_refreshButton   != null) _refreshButton.onClick.AddListener(OnRefreshClicked);
                if (_installButton   != null) _installButton.onClick.AddListener(OnInstallClicked);
                if (_uninstallButton != null) _uninstallButton.onClick.AddListener(OnUninstallClicked);
                if (_updateAllButton != null) _updateAllButton.onClick.AddListener(OnUpdateAllClicked);
                if (_cancelButton    != null) _cancelButton.onClick.AddListener(OnCancelClicked);
                if (_freeUpSpaceButton != null) _freeUpSpaceButton.onClick.AddListener(OnFreeUpSpaceClicked);

                RebuildList();
                RefreshFooter();
                RefreshFreeUpSpaceButton();
                SetStatus("Ready", _colorAvailable);
            }
            catch (System.OperationCanceledException)
            {
                // cancellation is not an error — exit quietly
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnDestroy()
        {
            CancelAndDispose(ref _installCts);
            CancelAndDispose(ref _freeUpSpaceCts);
            CancelAndDispose(ref _refreshCts);
            CancelAndDispose(ref _downloadSizeCts);
            UnsubscribeEvents();
        }

        // ── Event wiring ──────────────────────────────────────────────────────

        private void SubscribeEvents()
        {
            if (_service == null) return;
            _service.OnPackageStateChanged += OnPackageStateChanged;
            _service.OnDownloadProgress    += OnDownloadProgress;
            _service.OnPackageError        += OnPackageError;
            _service.OnCatalogRefreshed    += OnCatalogRefreshed;
        }

        private void UnsubscribeEvents()
        {
            if (_service == null) return;
            _service.OnPackageStateChanged -= OnPackageStateChanged;
            _service.OnDownloadProgress    -= OnDownloadProgress;
            _service.OnPackageError        -= OnPackageError;
            _service.OnCatalogRefreshed    -= OnCatalogRefreshed;
        }

        // ── Service callbacks ─────────────────────────────────────────────────

        private void OnPackageStateChanged(string packageId, PackageStatus status)
        {
            RefreshItemCard(packageId);
            if (packageId == _selectedPackageId)
                RefreshDetailPanel();
            RefreshFooter();
            RefreshUpdateAllButton();
            RefreshFreeUpSpaceButton();
        }

        private void OnDownloadProgress(string packageId, float progress)
        {
            if (packageId != _selectedPackageId) return;
            SetProgress(progress);
        }

        private void OnPackageError(string packageId, string error)
        {
            if (packageId == _selectedPackageId)
                RefreshDetailPanel();
            SetStatus($"Error: {error}", _colorFailed);
        }

        private void OnCatalogRefreshed()
        {
            RebuildList();
            RefreshFooter();
            RefreshUpdateAllButton();
            RefreshFreeUpSpaceButton();
            _refreshing = false;
            if (_refreshButton      != null) _refreshButton.interactable      = true;
            if (_refreshButtonLabel != null) _refreshButtonLabel.text         = "Refresh Catalog";
        }

        // ── List ──────────────────────────────────────────────────────────────

        private void RebuildList()
        {
            foreach (var item in _items)
                if (item != null) Destroy(item.gameObject);
            _items.Clear();

            if (_settings == null || _settings.packageConfigs == null || _settings.packageConfigs.Count == 0)
            {
                if (_emptyLabel != null) _emptyLabel.gameObject.SetActive(true);
                return;
            }

            if (_emptyLabel != null) _emptyLabel.gameObject.SetActive(false);

            foreach (var cfg in _settings.packageConfigs)
            {
                if (!cfg.isVisible) continue;

                var item   = Instantiate(_listItemPrefab, _listContainer);
                var state  = _service.GetPackageState(cfg.packageId);
                var remote = _service.GetRemoteMetadata(cfg.packageId);
                item.Initialize(cfg, state, StatusColor(state?.status ?? PackageStatus.Available), OnItemSelected);
                item.RefreshSize(remote?.bundleSizeBytes ?? 0);
                _items.Add(item);
            }

            if (_listButtonGroup != null)
                _listButtonGroup.RefreshButtonGroup();

            // Auto-select: keep previous selection if still present, else pick first item.
            if (_items.Count > 0)
            {
                bool previousStillExists = !string.IsNullOrEmpty(_selectedPackageId)
                    && _items.Exists(i => i.PackageId == _selectedPackageId);

                string idToSelect = previousStillExists ? _selectedPackageId : _items[0].PackageId;
                OnItemSelected(idToSelect);
            }
        }

        private void RefreshItemCard(string packageId)
        {
            var item = _items.Find(i => i.PackageId == packageId);
            if (item == null) return;

            var state  = _service.GetPackageState(packageId);
            var remote = _service.GetRemoteMetadata(packageId);
            item.Refresh(state, StatusColor(state?.status ?? PackageStatus.Available));
            item.RefreshSize(remote?.bundleSizeBytes ?? 0);
        }

        // ── Selection ─────────────────────────────────────────────────────────

        private void OnItemSelected(string packageId)
        {
            _selectedPackageId = packageId;

            foreach (var item in _items)
                item.SetSelected(item.PackageId == packageId);

            ShowDetail(packageId);
        }

        // ── Detail panel ──────────────────────────────────────────────────────

        private void ShowDetail(string packageId)
        {
            if (string.IsNullOrEmpty(packageId) || _settings == null)
            {
                if (_detailPanel != null) _detailPanel.SetActive(false);
                return;
            }

            var cfg = _settings.GetPackageConfig(packageId);
            if (cfg == null) { if (_detailPanel != null) _detailPanel.SetActive(false); return; }

            if (_detailPanel != null) _detailPanel.SetActive(true);
            RefreshDetailPanel();
            FetchDownloadSizeAsync(packageId);
        }

        private void RefreshDetailPanel()
        {
            if (string.IsNullOrEmpty(_selectedPackageId) || _settings == null) return;

            var cfg    = _settings.GetPackageConfig(_selectedPackageId);
            var state  = _service?.GetPackageState(_selectedPackageId);
            var remote = _service?.GetRemoteMetadata(_selectedPackageId);
            if (cfg == null) return;

            var status = state?.status ?? PackageStatus.Available;

            SetText(_detailName,    string.IsNullOrEmpty(cfg.displayName) ? cfg.packageId : cfg.displayName);
            SetText(_detailId,      cfg.packageId);
            SetText(_detailVersion, remote?.version ?? cfg.metadata?.version ?? "—");

            var desc = remote?.description ?? cfg.metadata?.description;
            SetText(_detailDescription, string.IsNullOrEmpty(desc) ? "No description." : desc);

            SetText(_detailSize, remote != null ? SizeFormatter.Format(remote.bundleSizeBytes) : "");

            // Tags — comma-separated; hidden when empty.
            var tags = remote?.tags;
            SetText(_detailTags, tags != null && tags.Length > 0 ? string.Join(", ", tags) : "");

            // Changelog — hide row entirely when absent to save space.
            bool hasChangelog = !string.IsNullOrEmpty(remote?.changelog);
            if (_detailChangelogRow != null) _detailChangelogRow.SetActive(hasChangelog);
            if (hasChangelog) SetText(_detailChangelog, remote.changelog);

            if (_detailStatus != null)
            {
                _detailStatus.text  = StatusLabel(status);
                _detailStatus.color = StatusColor(status);
            }

            if (_detailDependencies != null)
            {
                var deps = new System.Text.StringBuilder();
                if (cfg.dependencies != null)
                    foreach (var d in cfg.dependencies)
                    {
                        if (string.IsNullOrEmpty(d.packageId)) continue;
                        deps.AppendLine($"• {d.packageId}");
                    }
                _detailDependencies.text = deps.Length > 0 ? deps.ToString().TrimEnd() : "None";
            }

            if (_detailUsedBy != null)
            {
                var usedBy = new System.Text.StringBuilder();
                foreach (var other in _settings.packageConfigs)
                {
                    if (other.dependencies == null) continue;
                    foreach (var dep in other.dependencies)
                    {
                        if (dep.packageId != cfg.packageId) continue;
                        string label = string.IsNullOrEmpty(other.displayName) ? other.packageId : other.displayName;
                        usedBy.AppendLine($"• {label}");
                        break;
                    }
                }
                _detailUsedBy.text = usedBy.Length > 0 ? usedBy.ToString().TrimEnd() : "None";
            }

            bool hasError = status == PackageStatus.Failed && !string.IsNullOrEmpty(state?.errorMessage);
            if (_errorRow != null) _errorRow.SetActive(hasError);
            SetText(_detailErrorMessage, hasError ? state.errorMessage : "");

            bool downloading = status == PackageStatus.Downloading;
            if (_progressRow != null) _progressRow.SetActive(downloading);
            if (downloading && state != null) SetProgress(state.downloadProgress);

            bool busy = downloading || _updatingAll || _freeingSpace;
            if (_cancelButton    != null) _cancelButton.gameObject.SetActive(busy);
            if (_installButton   != null) _installButton.gameObject.SetActive(!busy);
            if (_uninstallButton != null) _uninstallButton.gameObject.SetActive(!busy);
            if (_freeUpSpaceButton != null) _freeUpSpaceButton.gameObject.SetActive(!busy);

            if (_installButton != null)
            {
                bool canInstall = status == PackageStatus.Available
                               || status == PackageStatus.Failed
                               || status == PackageStatus.UpdateAvailable;
                _installButton.interactable = canInstall && !busy;
                if (_installButtonLabel != null)
                    _installButtonLabel.text = status == PackageStatus.UpdateAvailable ? "Update" : "Install";
            }

            if (_uninstallButton != null)
                _uninstallButton.interactable = !cfg.isRequired
                    && !IsUsedByInstalledPackage(cfg.packageId)
                    && (status == PackageStatus.Installed || status == PackageStatus.UpdateAvailable)
                    && !busy;

            RefreshFreeUpSpaceButton();
        }

        // ── Pre-install download size ─────────────────────────────────────────

        /// <summary>
        /// Asynchronously fetches the pending download size for <paramref name="packageId"/>
        /// and updates <see cref="_detailDownloadSize"/>. Cancels any in-flight size fetch
        /// for the previously selected package before starting.
        /// </summary>
        private async void FetchDownloadSizeAsync(string packageId)
        {
            try
            {
                CancelAndDispose(ref _downloadSizeCts);

                // Clear previous value immediately so stale data is never shown.
                SetText(_detailDownloadSize, "");

                var state  = _service?.GetPackageState(packageId);
                var status = state?.status ?? PackageStatus.Available;

                // Only relevant before content is on-device.
                if (status == PackageStatus.Installed || status == PackageStatus.Downloading) return;

                _downloadSizeCts = new CancellationTokenSource();
                var ct = _downloadSizeCts.Token;

                long bytes = await _service.GetDownloadSizeAsync(packageId);

                // Guard: user may have navigated away or CTS was disposed by OnDestroy.
                if (ct.IsCancellationRequested || packageId != _selectedPackageId) return;

                SetText(_detailDownloadSize, bytes > 0 ? $"↓ {SizeFormatter.Format(bytes)}" : "");
            }
            catch (System.OperationCanceledException)
            {
                // cancellation is not an error — exit quietly
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        // ── Footer ────────────────────────────────────────────────────────────

        private void RefreshFooter()
        {
            if (_settings == null || _settings.packageConfigs == null) return;

            int  count      = 0;
            long totalBytes = 0;

            foreach (var cfg in _settings.packageConfigs)
            {
                if (!cfg.isVisible) continue;
                var state = _service?.GetPackageState(cfg.packageId);
                if (state == null) continue;
                if (state.status != PackageStatus.Installed && state.status != PackageStatus.UpdateAvailable) continue;

                count++;
                var remote = _service?.GetRemoteMetadata(cfg.packageId);
                if (remote != null) totalBytes += remote.bundleSizeBytes;
            }

            if (_footerInstalledCountLabel != null)
                _footerInstalledCountLabel.text = $"{count} package{(count == 1 ? "" : "s")} installed";

            if (_footerInstalledSizeLabel != null)
                _footerInstalledSizeLabel.text = totalBytes > 0 ? SizeFormatter.Format(totalBytes) : "";
        }

        private void RefreshUpdateAllButton()
        {
            if (_updateAllButton == null) return;

            int updateCount = 0;
            if (_settings?.packageConfigs != null)
                foreach (var cfg in _settings.packageConfigs)
                {
                    var state = _service?.GetPackageState(cfg.packageId);
                    if (state?.status == PackageStatus.UpdateAvailable) updateCount++;
                }

            _updateAllButton.gameObject.SetActive(updateCount > 0);
            _updateAllButton.interactable = updateCount > 0 && !_updatingAll;
            if (_updateAllButtonLabel != null)
                _updateAllButtonLabel.text = updateCount > 1
                    ? $"Update All ({updateCount})"
                    : "Update";
        }

        private void RefreshFreeUpSpaceButton()
        {
            if (_freeUpSpaceButton == null) return;

            long reclaimableBytes = GetReclaimableBytes();
            bool hasReclaimable = reclaimableBytes > 0;

            _freeUpSpaceButton.interactable = hasReclaimable && !_freeingSpace && !_updatingAll;
            if (_freeUpSpaceButtonLabel != null)
            {
                _freeUpSpaceButtonLabel.text = hasReclaimable
                    ? $"Free Up Space ({SizeFormatter.Format(reclaimableBytes)})"
                    : "Free Up Space";
            }
        }

        private long GetReclaimableBytes()
        {
            if (_service == null) return 0;

            long total = 0;
            foreach (var packageId in _service.GetEvictionCandidates(0))
            {
                var state = _service.GetPackageState(packageId);
                if (state != null)
                    total += System.Math.Max(0, state.installedSizeBytes);
            }
            return total;
        }

        private void SetProgress(float value)
        {
            if (_progressSlider != null) _progressSlider.value = value;
            if (_progressLabel  != null) _progressLabel.text   = $"{value:P0}";
        }

        // ── Button handlers ───────────────────────────────────────────────────

        private async void OnInstallClicked()
        {
            try
            {
                if (string.IsNullOrEmpty(_selectedPackageId) || _service == null) return;

                CancelAndDispose(ref _installCts);
                _installCts = new CancellationTokenSource();

                SetStatus($"Installing {_selectedPackageId}…", _colorDownloading);
                SetButtonsInteractable(false);

                var progress = new System.Progress<float>(SetProgress);
                var result   = await _service.InstallPackageAsync(_selectedPackageId, progress, _installCts.Token);

                SetButtonsInteractable(true);

                if (result.Success)
                    SetStatus($"Installed: {_selectedPackageId}", _colorInstalled);
                else if (result.WasCancelled)
                    SetStatus("Cancelled.", _colorAvailable);
                else
                    SetStatus($"Failed: {result.ErrorMessage}", _colorFailed);
            }
            catch (System.OperationCanceledException)
            {
                // cancellation is not an error — exit quietly
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async void OnUninstallClicked()
        {
            try
            {
                if (string.IsNullOrEmpty(_selectedPackageId) || _service == null) return;

                CancelAndDispose(ref _installCts);
                _installCts = new CancellationTokenSource();

                SetStatus($"Uninstalling {_selectedPackageId}…", _colorDownloading);
                SetButtonsInteractable(false);

                var result = await _service.UninstallPackageAsync(_selectedPackageId, _installCts.Token);

                SetButtonsInteractable(true);

                if (result.Success)
                    SetStatus($"Uninstalled: {_selectedPackageId}", _colorAvailable);
                else
                    SetStatus($"Failed: {result.ErrorMessage}", _colorFailed);
            }
            catch (System.OperationCanceledException)
            {
                // cancellation is not an error — exit quietly
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async void OnUpdateAllClicked()
        {
            try
            {
                if (_service == null || _updatingAll) return;

                _updatingAll = true;
                RefreshUpdateAllButton();
                SetButtonsInteractable(false);

                CancelAndDispose(ref _installCts);
                _installCts = new CancellationTokenSource();
                var ct = _installCts.Token;

                var toUpdate = new List<string>();
                if (_settings?.packageConfigs != null)
                    foreach (var cfg in _settings.packageConfigs)
                    {
                        var state = _service.GetPackageState(cfg.packageId);
                        if (state?.status == PackageStatus.UpdateAvailable)
                            toUpdate.Add(cfg.packageId);
                    }

                int succeeded = 0;
                int failed    = 0;

                foreach (var packageId in toUpdate)
                {
                    if (ct.IsCancellationRequested) break;

                    SetStatus($"Updating {packageId}… ({succeeded + failed + 1}/{toUpdate.Count})", _colorDownloading);

                    var progress = new System.Progress<float>(v =>
                    {
                        if (packageId == _selectedPackageId) SetProgress(v);
                    });

                    var result = await _service.InstallPackageAsync(packageId, progress, ct);

                    if (result.Success)        succeeded++;
                    else if (!result.WasCancelled) failed++;
                }

                _updatingAll = false;
                SetButtonsInteractable(true);
                RefreshUpdateAllButton();
                RefreshFreeUpSpaceButton();
                RefreshDetailPanel();

                if (ct.IsCancellationRequested)
                    SetStatus("Update cancelled.", _colorAvailable);
                else if (failed > 0)
                    SetStatus($"Updated {succeeded}/{toUpdate.Count}. {failed} failed.", _colorFailed);
                else
                    SetStatus($"All {succeeded} package{(succeeded == 1 ? "" : "s")} updated.", _colorInstalled);
            }
            catch (System.OperationCanceledException)
            {
                // cancellation is not an error — exit quietly
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async void OnRefreshClicked()
        {
            try
            {
                if (_refreshing || _service == null) return;
                _refreshing = true;
                if (_refreshButton      != null) _refreshButton.interactable      = false;
                if (_refreshButtonLabel != null) _refreshButtonLabel.text         = "Refreshing…";
                SetStatus("Refreshing catalog…", _colorDownloading);

                CancelAndDispose(ref _refreshCts);
                _refreshCts = new CancellationTokenSource();

                var result = await _service.RefreshCatalogAsync(_refreshCts.Token);

                // OnCatalogRefreshed will reset _refreshing and button state on success.
                // Handle the failure / cancel paths here.
                if (!result.Success)
                {
                    _refreshing = false;
                    if (_refreshButton      != null) _refreshButton.interactable      = true;
                    if (_refreshButtonLabel != null) _refreshButtonLabel.text         = "Refresh Catalog";

                    if (!result.WasCancelled)
                        SetStatus($"Refresh failed: {result.ErrorMessage}", _colorFailed);
                    else
                        SetStatus("Refresh cancelled.", _colorAvailable);
                }
            }
            catch (System.OperationCanceledException)
            {
                // cancellation is not an error — exit quietly
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnCancelClicked()
        {
            CancelAndDispose(ref _freeUpSpaceCts);
            CancelAndDispose(ref _installCts);
            SetStatus("Cancelling…", _colorAvailable);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private async void OnFreeUpSpaceClicked()
        {
            if (_service == null || _freeingSpace) return;

            long reclaimableBytes = GetReclaimableBytes();
            if (reclaimableBytes <= 0)
            {
                SetStatus("No optional cached packages to remove.", _colorAvailable);
                RefreshFreeUpSpaceButton();
                return;
            }

            _freeingSpace = true;
            CancelAndDispose(ref _freeUpSpaceCts);
            _freeUpSpaceCts = new CancellationTokenSource();

            SetButtonsInteractable(false);
            RefreshDetailPanel();
            SetStatus($"Freeing {SizeFormatter.Format(reclaimableBytes)}...", _colorDownloading);

            OperationResult result;
            try
            {
                result = await _service.FreeUpSpaceAsync(0, _freeUpSpaceCts.Token);
            }
            catch (System.OperationCanceledException)
            {
                result = OperationResult.CreateCancelled();
            }
            catch (System.Exception ex)
            {
                result = OperationResult.CreateFailure(ex.Message);
            }

            _freeingSpace = false;
            SetButtonsInteractable(true);
            RebuildList();
            RefreshFooter();
            RefreshUpdateAllButton();
            RefreshFreeUpSpaceButton();
            RefreshDetailPanel();

            if (result.Success)
                SetStatus($"Freed up to {SizeFormatter.Format(reclaimableBytes)}.", _colorInstalled);
            else if (result.WasCancelled)
                SetStatus("Free up space cancelled.", _colorAvailable);
            else
                SetStatus($"Free up space failed: {result.ErrorMessage}", _colorFailed);
        }

        private void SetStatus(string message, Color color)
        {
            if (_statusLabel == null) return;
            _statusLabel.text  = message;
            _statusLabel.color = color;
        }

        private static void SetText(TextMeshProUGUI label, string value)
        {
            if (label != null) label.text = value;
        }

        private void SetButtonsInteractable(bool value)
        {
            if (_installButton   != null) _installButton.interactable   = value;
            if (_uninstallButton != null) _uninstallButton.interactable = value;
            if (_refreshButton   != null) _refreshButton.interactable   = value;
            if (_freeUpSpaceButton != null) _freeUpSpaceButton.interactable = value;
        }

        private static void CancelAndDispose(ref CancellationTokenSource cts)
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }

        private Color StatusColor(PackageStatus status) => status switch
        {
            PackageStatus.Available       => _colorAvailable,
            PackageStatus.Downloading     => _colorDownloading,
            PackageStatus.Installed       => _colorInstalled,
            PackageStatus.Failed          => _colorFailed,
            PackageStatus.UpdateAvailable => _colorUpdate,
            _                             => _colorAvailable
        };

        private bool IsUsedByInstalledPackage(string packageId)
        {
            if (_settings?.packageConfigs == null) return false;
            foreach (var other in _settings.packageConfigs)
            {
                if (other.dependencies == null) continue;
                foreach (var dep in other.dependencies)
                {
                    if (dep.packageId != packageId) continue;
                    var otherState = _service != null ? _service.GetPackageState(other.packageId) : null;
                    if (otherState is { status: PackageStatus.Installed or PackageStatus.UpdateAvailable })
                        return true;
                }
            }
            return false;
        }

        private static string StatusLabel(PackageStatus status) => status switch
        {
            PackageStatus.Available       => "Available",
            PackageStatus.Downloading     => "Downloading…",
            PackageStatus.Installed       => "Installed",
            PackageStatus.Failed          => "Failed",
            PackageStatus.UpdateAvailable => "Update Available",
            _                             => "Unknown"
        };
    }
}
