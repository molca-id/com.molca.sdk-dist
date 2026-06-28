# Changelog — com.molca.sdk

All notable changes to the Molca SDK package.

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
