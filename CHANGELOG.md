# Changelog — com.molca.sdk

All notable changes to the Molca SDK package.

## [0.2.4] — 2026-07-04

### Fixed
- **Modals now untrack from `ModalManager` when destroyed externally.** Six modals
  (`DatePickerModal`, `MediaConfirmationModal`, `NumberInputKeyboard`, `ProgressModal`, `SelectionModal`,
  `TextInputModal`) declared their own `private void OnDestroy()`, shadowing the base cleanup so a modal
  destroyed outside the normal close path stayed registered. They now `override` the base and call
  `base.OnDestroy()` (part of the Sprint 79 silent-failure scrub).

### Changed
- Bump the `com.molca.core` dependency pin to `1.12.2` (required: the modal fix relies on Core 1.12.2's new
  `protected virtual BaseModal.OnDestroy()`).

## [0.2.3] — 2026-07-04

### Added
- **Default UI Token Catalog (Sprint 57.5).** Authored the SDK's `MolcaUiTokenCatalog` asset
  (`Runtime/Settings/UI Tokens/UI Token Catalog.asset`) — the concrete token values Core's abstract
  registry expects (Core ships the engine but no values). Includes the `control/button-icon` token.

## [0.2.2] — 2026-07-01

### Fixed
- **Confirmation modals moved into the SDK layer, where their dependency actually lives.**
  `Confirmation Short.prefab` and `Confirmation Detailed.prefab` were Prefab Variants of
  `com.molca.sdk`'s `Runtime/Prefabs/UI/Button.prefab`, but the prefabs themselves shipped in
  `com.molca.core` — a reverse layering dependency (Core → SDK) that architecture.md forbids. On a
  fresh install this raced with asset import order and surfaced as a "missing Prefab" console error
  for both variants. They're now under `Runtime/Prefabs/Modals/` in this package, guids unchanged
  (git-mv only, so all existing references resolve automatically); `com.molca.core`'s `Modals` folder
  keeps only the assets it doesn't depend on the SDK for (`Modal Loading`, `Modal Message`).

### Changed
- Bump the `com.molca.core` dependency pin to `1.11.3` (Onboarding Wizard + the Confirmation-prefab fix).

## [0.2.1] — 2026-07-01

### Changed
- Bump the `com.molca.core` dependency pin to `1.11.1` (tested against current Core).

### Fixed
- **`MediaLoader` HTTP-asset anti-pattern.** The media load path now goes through the framework's
  HTTP-request mechanism instead of the previous non-conforming call, per Core networking conventions;
  `VideoHandler` adjusted to match.

## [0.2.0] — Unreleased

### Changed
- **SDK layer hardening pass (Sprint 66).** First quality pass since the Sprint-60 extraction, with no
  fork-facing API changes:
  - Removed the unused `GameManager.Instance` static singleton (no callers); kept the `RuntimeSubsystem`
    and its connection-error modal. A reflection test now fails the build if a public static self-typed
    singleton is reintroduced.
  - Async contract: every previously-unguarded `async void` Unity entry point (~17 runtime files) is now a
    thin shim wrapped in `try/catch` — `OperationCanceledException` handled quietly, real failures logged
    via `Debug.LogException`.
  - `MediaLoader` load failures now log the full exception (stack) instead of `e.Message` only.
  - Audit recorded internally (`Documentation~/internal/SDK_HARDENING_AUDIT.md`, not shipped): `.Result`
    usages are `AsyncOperationHandle` (not blocking `Task`); `FindObjectOfType` is absent. Blanket XML-doc
    coverage continues as an ongoing fill-in.

## [0.1.3] — Unreleased

### Changed
- Bump the `com.molca.core` dependency pin to `1.9.8` (Core now declares the toggleable built-in modules
  it uses directly — ugui/audio/unitywebrequest/uielements — per Sprint 63).

## [0.1.2] — Unreleased

### Fixed
- **Standalone closure (built-in modules):** declare `com.unity.modules.video` (used by `MediaInfo`'s
  `UnityEngine.Video.VideoPlayer`) and `com.unity.ugui` (used directly throughout via `UnityEngine.UI` /
  `EventSystems`). A consumer without the Video module enabled previously failed to compile with CS1069.
  Built-in modules are `com.unity.modules.*` UPM packages, so declaring them lets UPM ensure they're
  present on install.
- `MolcaSDK.Package.Tests` now also guards built-in-module closure (Runtime use of a toggleable
  `UnityEngine` namespace must be backed by a declared dependency).

## [0.1.1] — Unreleased

### Fixed
- **Standalone closure:** ship `ZXing.Net` (`Runtime/Plugins/zxing.unity.dll`, v0.16.11.0, Apache-2.0)
  inside the package. `QRScanner` referenced `ZXing` via a dev-repo-only `Assets/Plugins/zxing.dll`, so a
  clean consumer would fail to compile `MolcaSDK`. The DLL now ships with the package (Auto Reference on,
  matching prior behavior) and the Apache license is included. Consumers that already imported `zxing.dll`
  directly must remove their copy to avoid an ambiguous-`ZXing` conflict.
- `MolcaSDK.Package.Tests` now also guards against shipped source using a third-party namespace whose
  assembly isn't bundled (catches the ZXing class of leak that a path-only scan missed).

## [0.1.0] — Unreleased

### Added
- Initial extraction of the shared SDK layer from `Assets/_MolcaSDK/` into a UPM package
  (Sprint 60). Assembly names (`MolcaSDK`, `MolcaSDK.Editor`) and all asset GUIDs preserved.
- `com.molca.core` declared as a package dependency (standalone closure: Core resolves through UPM).
- Quick setup starter settings under `Samples~/QuickSetup/Settings/` with re-generated, disjoint GUIDs,
  plus `QuickSetupInstaller` (menu **Molca ▸ SDK ▸ Quick Setup**) that copies them idempotently into
  `Assets/_MolcaSDK/Settings/` (Sprint 60.9).
- `MolcaSDK.Package.Tests` edit-mode boundary guard: SDK depends on Core, shipped surface references no
  dev-repo-only paths, quick-setup templates carry disjoint GUIDs (Sprint 60.8).

### Fixed
- Removed a hardcoded test-scene `onClick` (`Assets/_MolcaSDK/Tests/.../Test_Addressables.unity`) from the
  shared `PackageContent Button` prefab — a reusable building block must not auto-load a demo scene.
- Updated stale `Assets/_MolcaSDK/Code/...` source paths in `MODAL_TYPES_SUMMARY.md` to the package path.
