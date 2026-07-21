---
title: Quick Setup
category: Getting Started
order: 20
---

# Quick Setup

The SDK's Quick Setup installer seeds the **held-back starter settings** — the bootstrap configuration
the SDK deliberately does not ship as live assets — into your project's consumer space. It exists
because `com.molca.sdk` is a read-only package: config that a project is meant to edit cannot live
inside the package, so it is copied out on demand.

## Running it

From the menu:

- **Molca → SDK → Quick Setup → Install Starter Settings** — copies the starter settings, **keeping**
  any files you already have (idempotent; safe to run repeatedly).
- **Molca → SDK → Quick Setup → Repair (Overwrite) Starter Settings** — re-copies, **overwriting**
  existing files. Use this to reset to the shipped defaults.

Both land the settings under `Assets/_MolcaSDK/Settings/`. The same steps are offered by the
[Onboarding Wizard](ONBOARDING.md); the installer is `QuickSetupInstaller` (namespace
`MolcaSDK.Editor.Setup`), invoked there through reflection so Core never hard-depends on the SDK.

## What it installs

The starter settings are the SDK's bootstrap scaffolding — the shared `GlobalSettings` module list,
input actions, and lighting configuration a fresh SDK app expects. They are seeded into project space
so you can edit them without touching the package; the package itself stays immutable.

After running it, open **Project Settings → Molca Settings** (or the [Hub](HUB.md)) to review the
seeded configuration, and see [Settings](SETTINGS.md) for how the modules work.

## See also

- [SDK Overview](SDK_OVERVIEW.md)
- [Getting Started](GETTING_STARTED.md)
- [App Flow](SDK_APP_FLOW.md)
- [Onboarding Wizard](ONBOARDING.md)
