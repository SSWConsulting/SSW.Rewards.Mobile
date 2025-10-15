# Beta Testing – Mobile App (Android & iOS)

## Goal

Give developers, QA, Product Owner, and other stakeholders access to pre‑release builds so we can find issues early before releasing to Production stores.

## High-Level Flow

1. Developer merges PR into `main`.
2. CI pipeline builds Android (AAB) + iOS (IPA) artifacts.
3. A manual approval gate (SSW Rewards Maintainers) in the CD pipeline is evaluated.
4. On approval the pipeline automatically:
   - Uploads Android AAB to Google Play (Beta / Internal Testing).
   - Uploads iOS build to App Store Connect and enables it for designated TestFlight groups.
5. Testers on those beta tracks receive updates automatically (Play Store auto-update / TestFlight notification depending on their settings).
6. Sanity / regression tests executed (see "Sanity / Smoke Test Checklist").
7. When criteria met, the same (or a subsequent) build is promoted to Production (separate approval gate – see `Instructions-Deployment.md`).

## CI/CD Pipeline Overview

| Stage                 | Trigger            | Action                                            | Manual Gate            | Outputs                    |
| --------------------- | ------------------ | ------------------------------------------------- | ---------------------- | -------------------------- |
| Build                 | Push to `main`     | Compile, run unit tests, produce AAB + IPA        | No                     | Build artifacts            |
| Beta Deploy (Android) | Post-build success | (Pending until gate) Upload AAB to Play track     | Yes (Approve)          | Play Store beta available  |
| Beta Deploy (iOS)     | Post-build success | (Pending until gate) Upload to App Store Connect  | Yes (Approve)          | TestFlight build available |
| Prod Release          | Manual trigger     | Promotes approved beta build to Production stores | Yes (Release approval) | Public store release       |

## Terminology

- Internal Testing (Play Console) / Beta Track: Private track targeted by the pipeline; testers auto-update.
- TestFlight Internal: Up to 100 members (no Apple review required) – auto-enabled by pipeline.
- TestFlight External: Up to 10,000 testers; if pipeline configured, first build per version requires Apple beta review before external testers see it.
- Firebase Analytics: Event & usage telemetry for mobile clients.
- Firebase Crashlytics: Primary mobile crash reporting surface (real-time crash triage).

## Requesting Beta Access (New Users)

If you are not yet a tester but need beta access:

1. Confirm you actually need pre-release features (otherwise use Production store build).
2. Email the Product Owner (PO) or post in the Teams channel with:
   - Subject: "Beta Access Request – SSW Rewards (Android/iOS)"
   - Platforms required (Android, iOS, or both)
   - Relevant account email (Google, Apple, etc.)
3. PO approves and adds you to the relevant tester lists (see Adding a New Tester) and confirms back.
4. If you have not received confirmation within 2 business days, follow up with PO / team.

## Adding a New Tester

### 1. Gather Info

- Full name
- Platform(s) required (Android, iOS, or both)
- Email address

### 2. Android – Add to Play Beta

1. Google Play Console > `SSW Rewards` > Testing > Internal testing.
2. Add tester email to the Testers list.
3. Provide tester the opt‑in link if first time.

### 3. iOS – Add to TestFlight Group

1. App Store Connect > Users and Access: add internal user if needed.
2. TestFlight tab > Add tester to internal testing group.
3. After build processing finishes the tester automatically gets access.

## Promotion Steps (Automated)

### Android

1. Merge -> build pipeline runs.
2. Approver reviews change summary & test results at the approval gate.
3. On approval pipeline uploads AAB to configured track and starts rollout (usually 100% instantly for test tracks).
4. Monitor Play Console release for status / crashes (supplement with Firebase Crashlytics).

### iOS

1. Merge -> build pipeline runs.
2. Approver grants beta deployment stage.
3. Pipeline uploads build to App Store Connect (processing may take several minutes).
4. Monitor TestFlight metrics & Firebase Crashlytics for issues.

## Release Notes Template (Keep concise)

```
Title: <Platform> Beta <Version> (<Build>)
Changes:
- Feature: ...
- Improvement: ...
- Fix: ...
Known Issues:
- ...
Testing Focus:
- Validate achievement completion flow.
- Try purchase of a reward.
```

## Sanity / Smoke Test Checklist (Each New Beta Build)

Use a physical device where possible.

- Launch app (no crash) and reaches sign-in screen.
- Sign in succeeds.
- App resumes from background without error.

Gold plating:

- Scan a QR code and points increment.
- Complete an achievement & see updated progress.
- Purchase (or simulate) a reward.
- Push notification received.

## Versioning & Build Numbers

- Marketing version (e.g. 3.10.0) must be incremented manually each store release.
- Build number auto-increments per CI run.

## Telemetry & Crash Reporting

- Firebase Analytics: Confirm sessions & key events (login, scan, purchase) appearing for the new build within 5–10 minutes.
- Firebase Crashlytics: Verify build (version + build number) shows as a new release; monitor for new crash groups.

## Offboarding a Tester

1. Remove from Play track tester list / Google Group.
2. Remove from TestFlight group (and optionally from Users & Access).

## References

- [Deployment Steps](Instructions-Deployment.md)
- Apple TestFlight docs: https://developer.apple.com/testflight/
- Google Play testing tracks: https://support.google.com/googleplay/android-developer/answer/9845334
- Firebase Analytics: https://firebase.google.com/docs/analytics
- Firebase Crashlytics: https://firebase.google.com/docs/crashlytics
