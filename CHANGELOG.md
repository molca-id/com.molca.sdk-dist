# Changelog — com.molca.sdk

All notable changes to the Molca SDK package.

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
