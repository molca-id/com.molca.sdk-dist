# Molca SDK (`com.molca.sdk`) — distribution

> ### ⚠ Deprecated as of 0.2.6 — this is the final release
>
> `com.molca.sdk` has been dissolved into `com.molca.core` as of **Core 2.0.0**. Its uGUI layer
> (`ColorIDButton`, `ColorSchemeDropdown`, `LanguageDropdown`, and related widgets) now ships in Core
> itself; its app-scaffolding code (`AuthUI`, `ProfileUI`, `PreloadCheck`, `GameManager`, media handlers,
> and the `MolcaSDK`/`MolcaSDK.Editor` assemblies, renamed to `Molca.App`/`Molca.App.Editor`) moved into
> Core's **Starter Project Content** sample. There is no replacement add-on and no successor package —
> **do not add this dependency to a new project.**
>
> **If your project is on Core 1.x and depends on this package:** upgrade to Core 2.0.0 following
> [Upgrading Molca Core 1.x to 2.0](https://github.com/molca-id/com.molca.core-dist/blob/main/Documentation~/reference/UPGRADING_TO_2_0.md),
> import Core's **Starter Project Content** sample for the scaffolding you still need, remove this
> package from `Packages/manifest.json`, and update source references per the generated API
> replacement table in that guide (`MolcaSDK` → `Molca.App`).
>
> This repository is not archived — existing tags keep resolving for projects still pinned to them —
> but no new versions will be published here. 0.2.6 changes nothing but this notice.

Shared SDK layer on top of Molca Core: common app scaffolding (auth, media, modals, home, preload, UI building blocks) that the VR/DT SDK forks and projects extend.

Read-only mirror of the embedded `com.molca.sdk` UPM package, published from the private
framework dev repo. Do not edit here — changes are made upstream and re-published.

**Superseded — see the deprecation notice above.**

## Install

Add to the consumer project's `Packages/manifest.json` (Core resolves from its own dist repo;
Unity forbids Git-URL package dependencies, so both lines are listed):

```json
"com.molca.core": "https://github.com/molca-id/com.molca.core-dist.git#1.9.7",
"com.molca.sdk":  "https://github.com/molca-id/com.molca.sdk-dist.git#<version>"
```

See the tagged releases for available versions. `PUBLISH_MANIFEST.txt` lists exactly what shipped.
