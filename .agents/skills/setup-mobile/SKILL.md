---
name: setup-mobile
description: Build and run the SSW.Rewards .NET MAUI mobile app on an Android emulator (or device). REQUIRES a reachable backend first — run the API/AdminUI via the setup-admin-api skill, or point at staging. Use when someone wants to build/run the mobile app, point it at a backend, or troubleshoot the MAUI build/deploy.
---

# Build & run the mobile app (.NET MAUI)

Aspire does **not** run the mobile app — it runs on an emulator/device the usual way. But it needs a
backend to talk to. Full guide: `_docs/Aspire-Local-Dev.md`.

## Step 0 — have a backend (prerequisite)

The app needs an API + identity. Either:
- run the local stack (**setup-admin-api** skill / `aspire run` from the repo root), **or**
- just point at **staging** (works with no local backend — simplest for "does it run").

> The emulator can't reach the host's `localhost`, so for the mobile app use **staging** or a
> **tailscale** URL — not `local` — unless you've set up Tailscale to the local API.

## Step 1 — MAUI workloads (one-time)

```bash
dotnet workload install maui        # or maui-android for Android only
```
On a system-wide .NET install this needs `sudo`.

## Step 2 — point the app at a backend

```bash
./rewards-dev api staging      # API → staging (leaves your local identity/WebAPI untouched)
# or: ./rewards-dev api tailscale   (stable phone URL + auto `tailscale serve`; see _docs/Aspire-Local-Dev.md)
# `./rewards-dev help` is fully self-teaching; it wraps `dotnet run --project tools/RewardsDev`.
```

## Step 3 — Firebase config (git-ignored)

`google-services.json` (Android) and `GoogleService-Info.plist` (iOS) are git-ignored; only
`*.template` placeholders are committed. The Android build **fails** if `google-services.json` is the
placeholder. Get the real files from Keeper, or use the **Sync mobile secrets (isolated)** command on
the dashboard `mobile-app` resource.

## Step 4 — build & deploy to a running emulator

```bash
# list emulators / boot one
~/Library/Android/sdk/emulator/emulator -list-avds
~/Library/Android/sdk/emulator/emulator -avd <avd-name> &

# build + deploy + launch — Android-only, no iOS workload needed
./rewards-dev mobile android
# (= dotnet build src/MobileUI/MobileUI.csproj -t:Run -f net10.0-android \
#      -p:MobileTargetFrameworks=net10.0-android)
```

`./rewards-dev mobile android` (and the dashboard **Build & Run (Android)** command) is the supported
Android-only path. To target a specific emulator with the raw dotnet form, add
`-p:AdbTarget="-s <emulator-id>"`.

## Troubleshooting

- **App installs but crashes instantly: `No assemblies found … Fast Deployment`** — you did a raw
  `adb install` of the Debug APK. Use `dotnet build -t:Run` (or `rewards-dev mobile android`) instead.
- **Build error in `ProcessGoogleServicesJson` / XML parse** — `google-services.json` is the
  `// Copy from Keeper` placeholder. Materialize the real one (Step 3).
- **`NETSDK1147: workload "ios" must be installed` / restore demands the iOS workload** — the project
  multi-targets iOS + Android. Use `rewards-dev mobile android`, which passes
  `-p:MobileTargetFrameworks=net10.0-android` so restore is Android-only. `maui-android` is enough.
- **`NETSDK1005: … doesn't have a target for 'net10.0'` in referenced libs** — don't override the
  well-known `-p:TargetFrameworks=…`; it leaks into referenced projects. Use the custom
  `-p:MobileTargetFrameworks=net10.0-android` (what `rewards-dev mobile android` does).
- **API calls fail from the emulator** — you're pointed at `local` (`localhost`), which the emulator
  can't reach. Switch with `./rewards-dev api staging` (or tailscale).
