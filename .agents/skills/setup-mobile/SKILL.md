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

The app targets **both iOS and Android**. Install both for full coverage:

```bash
dotnet workload install maui            # iOS + Android
```

Or just one platform if that's all you build (e.g. you test on an Android device, no iPhone):

```bash
dotnet workload install maui-android    # Android only  → `./rewards-dev mobile android`
dotnet workload install maui-ios        # iOS only      → `./rewards-dev mobile ios` (Mac + Xcode)
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

> **Fresh git worktree?** These files are materialized *into the working tree*, so each new worktree
> needs them even if another worktree on the machine already has them. Run
> `rewards-dev secrets sync-mobile` once in the new worktree (idempotent). See the **New worktree
> bootstrap** note in [Aspire-Local-Dev.md](../../../_docs/Aspire-Local-Dev.md) so you don't chase a
> build that fails only on Firebase.

## Step 4 — build & deploy (Android emulator or iOS simulator)

The app builds for both platforms. Build one platform at a time — only that platform's workload is
needed. (Tip: testing on a physical Android device is the common path here.)

```bash
# --- Android ---
~/Library/Android/sdk/emulator/emulator -list-avds          # list AVDs
~/Library/Android/sdk/emulator/emulator -avd <avd-name> &   # boot one
./rewards-dev mobile android
# (= dotnet build src/MobileUI/MobileUI.csproj -t:Run -f net10.0-android \
#      -p:MobileTargetFrameworks=net10.0-android)

# --- iOS (Mac + Xcode + booted simulator) ---
./rewards-dev mobile ios
# (= dotnet build src/MobileUI/MobileUI.csproj -t:Run -f net10.0-ios \
#      -p:MobileTargetFrameworks=net10.0-ios)
```

`./rewards-dev mobile <android|ios>` (and the dashboard **Build & Run (Android)** / **Build & Run
(iOS)** commands) are the supported single-platform paths. To target a specific Android emulator
with the raw dotnet form, add `-p:AdbTarget="-s <emulator-id>"`.

## Troubleshooting

- **App installs but crashes instantly: `No assemblies found … Fast Deployment`** — you did a raw
  `adb install` of the Debug APK. Use `dotnet build -t:Run` (or `rewards-dev mobile android`) instead.
- **Build error in `ProcessGoogleServicesJson` / XML parse** — `google-services.json` is the
  `// Copy from Keeper` placeholder. Materialize the real one (Step 3).
- **`NETSDK1147: workload "ios" (or "android") must be installed`** — the project multi-targets iOS +
  Android, so a build that restores both demands both workloads. Build one platform at a time with
  `rewards-dev mobile <android|ios>` (it passes `-p:MobileTargetFrameworks=net10.0-<platform>`), and
  install just that workload (`maui-android` or `maui-ios`). This is the intended path when you only
  want Android and don't want to deal with the iOS/Apple toolchain.
- **`NETSDK1005: … doesn't have a target for 'net10.0'` in referenced libs** — don't override the
  well-known `-p:TargetFrameworks=…`; it leaks into referenced projects. Use the custom
  `-p:MobileTargetFrameworks=net10.0-<platform>` (what `rewards-dev mobile <android|ios>` does).
- **API calls fail from the emulator/simulator** — you're pointed at `local` (`localhost`), which the
  device can't reach. Switch with `./rewards-dev api staging` (or tailscale).
