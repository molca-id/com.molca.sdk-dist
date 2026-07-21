---
title: Overview
category: Getting Started
order: 10
---

# Overview

`com.molca.sdk` is the **shared SDK layer** — common application scaffolding that sits on top of
[Molca Core](OVERVIEW.md) and below the domain-specific forks and your project. It depends on
`com.molca.core` (plus uGUI and the Unity video module) and adds the pieces almost every Molca app
needs: an app-level `GameManager`, auth UI, media loading, a modal library, uGUI building blocks, a
preload phase, and general utilities. All SDK types live in the `MolcaSDK` namespace.

## Layer position

```
Project content        (your scenes, screens, scenario assets)
   ↓ subclass only
com.molca.sdk          (this layer — auth, media, modals, home, preload, UI widgets)
   ↓ subclass only
com.molca.core         (RuntimeManager, DI, Events, Sequences, Networking, Modals, Settings…)
```

Like Core, the SDK is a **read-only package**: extend it from project space by subclassing, never by
editing it. Held-back bootstrap configuration (GlobalSettings, input actions, lighting) is seeded into
your project by [Quick Setup](SDK_QUICK_SETUP.md) rather than shipped as live assets in the package.

## Feature areas

| Area | Guide | What it adds |
|---|---|---|
| App flow | [App Flow](SDK_APP_FLOW.md) | `GameManager`, the `Preload` phase, and `Home` screens. |
| Auth | [Auth](SDK_AUTH.md) | Login/guest UI on top of Core's `AuthManager`. |
| Media | [Media](SDK_MEDIA.md) | Cached async image/video/document loading. |
| Modals | [SDK Modals](SDK_MODALS.md) | A library of concrete modals over Core's `BaseModal`. |
| UI | [SDK UI](SDK_UI.md) | uGUI widgets bound to ColorID, Localization, and UI tokens. |
| Utilities | [SDK Utilities](SDK_UTILITIES.md) | Helpers including a ZXing-based QR scanner. |

## Forks build on this

Fork-specific documentation — VR interaction, digital-twin sync, and other domain layers — does **not**
live here. Each fork (`molca-sdk-vr`, `molca-sdk-dt`, …) is its own `com.molca.*` package and drops its
`Documentation~/reference/*.md` guides into itself; Core's docs provider scans every installed
`com.molca.*` package, so a fork's guides appear in the Hub docs browser automatically alongside these.

## See also

- [Molca Core Overview](OVERVIEW.md)
- [Quick Setup](SDK_QUICK_SETUP.md)
- [App Flow](SDK_APP_FLOW.md)
- [Getting Started](GETTING_STARTED.md)
