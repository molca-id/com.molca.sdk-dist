# Molca SDK (`com.molca.sdk`) — distribution

Shared SDK layer on top of Molca Core: common app scaffolding (auth, media, modals, home, preload, UI building blocks) that the VR/DT SDK forks and projects extend.

Read-only mirror of the embedded `com.molca.sdk` UPM package, published from the private
framework dev repo. Do not edit here — changes are made upstream and re-published.

## Install

Add to the consumer project's `Packages/manifest.json` (Core resolves from its own dist repo;
Unity forbids Git-URL package dependencies, so both lines are listed):

```json
"com.molca.core": "https://github.com/molca-id/com.molca.core-dist.git#1.9.7",
"com.molca.sdk":  "https://github.com/molca-id/com.molca.sdk-dist.git#<version>"
```

See the tagged releases for available versions. `PUBLISH_MANIFEST.txt` lists exactly what shipped.
