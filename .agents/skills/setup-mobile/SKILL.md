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
dotnet run --project tools/RewardsDev -- env staging      # API + identity → staging
# or: ... -- api tailscale   (stable phone URL; see _docs/Aspire-Local-Dev.md → Tailscale)
```

## Step 3 — Firebase config (git-ignored)

`google-services.json` (Android) and `GoogleService-Info.plist` (iOS) are git-ignored; only
`*.template` placeholders are committed. The Android build **fails** if `google-services.json` is the
placeholder. Get the real files from Keeper, or use the dashboard **Mobile: Materialize Firebase
secrets** command.

## Step 4 — build & deploy to a running emulator

```bash
# list emulators / boot one
~/Library/Android/sdk/emulator/emulator -list-avds
~/Library/Android/sdk/emulator/emulator -avd <avd-name> &

# build + deploy + launch (-t:Run pushes the Fast-Deployment assemblies)
dotnet build src/MobileUI/MobileUI.csproj -t:Run -f net10.0-android -c Debug -p:AdbTarget="-s <emulator-id>"
```

## Troubleshooting

- **App installs but crashes instantly: `No assemblies found … Fast Deployment`** — you did a raw
  `adb install` of the Debug APK. Use `dotnet build -t:Run` instead (it pushes the override assemblies).
- **Build error in `ProcessGoogleServicesJson` / XML parse** — `google-services.json` is the
  `// Copy from Keeper` placeholder. Materialize the real one (Step 3).
- **`NETSDK1147: workload "ios" must be installed`** — the project multi-targets iOS + Android.
  For an Android-only build, pass `-f net10.0-android` (already above). iOS needs the `maui-ios`
  workload and Mac signing.
- **`NETSDK1005: … doesn't have a target for 'net10.0'` in referenced libs** — don't pass
  `-p:TargetFrameworks=…` globally; it leaks into referenced projects. Use `-f net10.0-android`.
- **API calls fail from the emulator** — you're pointed at `local` (`localhost`), which the emulator
  can't reach. Switch with `… rewards-dev -- api staging` (or tailscale).
