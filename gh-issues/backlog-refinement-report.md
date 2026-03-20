# Backlog Refinement Report

Scope: `RD/fy25` candidate PBIs and supporting evidence files in this repo.

Short version: the backlog is mostly healthy, but it has a few places where one story has been split into several smaller stories, a couple of issues that were effectively finished but never fully closed out, and one or two PBIs that are not ready for planning yet. The backlog has managed to reproduce itself just enough to keep everyone employed, which is very on-brand for a living product.

## 1. Duplicated Or Overlapping PBIs

I did not find many exact duplicate issues. Most of the repetition is actually deliberate decomposition of a larger epic into smaller PBIs.

### Overlap cluster A: Performance / leaderboard work

- [R&D Candidate 01: API Performance Optimization & Server-Side Caching Architecture](../RD/fy25/candidates/01-api-performance-caching.md)
- [R&D Candidate 05: Leaderboard Pagination Architecture & Conference Kiosk Mode](../RD/fy25/candidates/05-leaderboard-pagination-kiosk.md)
- [R&D Candidate 08: Application Observability & Telemetry Infrastructure](../RD/fy25/candidates/08-observability-telemetry.md)

Why it overlaps:

- Candidate 01 includes diagnostics, query tuning, caching, and leaderboard optimisation.
- Candidate 05 is a narrower slice of the same leaderboard effort: pagination, incremental loading, and kiosk display.
- Candidate 08 exists largely to support Candidate 01 by making the SQL bottlenecks visible.

Refinement note:

- These are not bad duplicates, but they should stay explicitly parented to the performance epic rather than floating as separate “similar sounding” stories.

### Overlap cluster B: Firebase / notification infrastructure

- [R&D Candidate 02: Push Notification System with HangFire Scheduling](../RD/fy25/candidates/02-push-notification-system.md)
- [R&D Candidate 06: AppCenter to Firebase Crashlytics Migration](../RD/fy25/candidates/06-appcenter-firebase-migration.md)

Why it overlaps:

- Both workstreams touch Firebase, iOS entitlements, and pipeline configuration.
- Candidate 06 is infrastructure migration work; Candidate 02 is notification delivery work.

Refinement note:

- These should remain separate PBIs, but the shared Firebase setup should be called out once so the backlog does not look like two copies of the same work.

### Overlap cluster C: Auth / platform migration

- [R&D Candidate 03: .NET 9 Framework Migration](../RD/fy25/candidates/03-dotnet9-upgrade.md)
- [R&D Candidate 07: MAUI Authentication & Autologin with SSW Identity Server](../RD/fy25/candidates/07-authentication-autologin.md)

Why it overlaps:

- Both involve pipeline/authentication platform changes and long-running dependency work.
- The auth story also crosses into deployment configuration and mobile platform behaviour.

Refinement note:

- Keep them separate, but ensure the auth story is not silently absorbing release-engineering work that belongs in the .NET 9 migration or CI/CD stories.

## 2. Already Done But Not Completed

### Issue #1119: NDC Porto crashes

- Evidence: [RD/fy25/docs/evidence/key-issues.json](../RD/fy25/docs/evidence/key-issues.json)
- Status: closed
- Problem: the issue body still has unchecked follow-ups:
  - check retention policy
  - make sure a year's worth of data is retained / cold-stored
- Result: the incident fix was completed, but the original PBI was not fully completed against its own checklist.

### Issue #1230: Unexpected logout

- Evidence: [RD/fy25/docs/evidence/key-issues.json](../RD/fy25/docs/evidence/key-issues.json)
- Status: closed
- Problem: the original task list still includes unchecked items in the issue body:
  - verify refresh token lifetime
  - check Application Insights for auth errors
  - implement additional logging
- Result: the issue was closed, but the backlog item itself reads like a partial closure rather than a fully completed story.

### Issue #1306: Full offline support

- Evidence: [RD/fy25/02-offline-support/timeline.md](../RD/fy25/02-offline-support/timeline.md), [RD/fy25/02-offline-support/README.md](../RD/fy25/02-offline-support/README.md), [RD/fy25/docs/evidence/key-issues.json](../RD/fy25/docs/evidence/key-issues.json)
- Status: open
- Problem: the epic has been decomposed into closed child PBIs `#1308-#1311`, but the parent epic remains open.
- Result: the implementation is partly or largely complete, but the epic is still carrying work that should either be closed, re-scoped, or explicitly marked as remaining follow-up.

## 3. Not Ready

### Issue #1161: Web API | DbContext pool

- Evidence: [RD/fy25/docs/evidence/key-issues.json](../RD/fy25/docs/evidence/key-issues.json)
- Status: open

Why it is not ready:

- The pain statement is incomplete and cuts off mid-sentence.
- The story is solution-first: it jumps straight to `DbContextPool` without defining the user/business outcome.
- There is no effort estimate or business value visible in the issue body.
- Acceptance criteria are too thin to support planning: `Should have DbContextPool` is a technical implementation note, not a measurable outcome.

Refinement note:

- This needs a proper PBI rewrite before it enters sprint planning. It should explain the performance problem, expected benefit, acceptance criteria, and estimate.

## 4. Hygiene Issues

### Duplicate documentation formats

- [RD/fy25/README.md](../RD/fy25/README.md)
- [RD/fy25/index.html](../RD/fy25/index.html)

Observation:

- The same candidate inventory is maintained in both markdown and HTML.
- That increases the chance of one copy drifting out of date.

### Inconsistent issue phrasing and status markers

- [RD/fy25/docs/evidence/ca1-performance-prs.json](../RD/fy25/docs/evidence/ca1-performance-prs.json)
- [RD/fy25/docs/evidence/ca2-offline-prs.json](../RD/fy25/docs/evidence/ca2-offline-prs.json)
- [RD/fy25/docs/evidence/ca3-notifications-prs.json](../RD/fy25/docs/evidence/ca3-notifications-prs.json)
- [RD/fy25/docs/evidence/ca4-qr-scanning-prs.json](../RD/fy25/docs/evidence/ca4-qr-scanning-prs.json)
- [RD/fy25/docs/evidence/ca5-auth-migration-prs.json](../RD/fy25/docs/evidence/ca5-auth-migration-prs.json)

Observation:

- Status wording is inconsistent: `Closes #`, bare `#`, `Done`, and `(closed)` are all used.
- Some items are labelled `part 1`, `part 1.2`, or `part 2`, which makes the narrative harder to scan.
- There are a few obvious typos in the evidence set, including `Leaderbaoard` and `Leadeboard`.

### Story-splitting ambiguity

- [R&D Candidate 02: Push Notification System with HangFire Scheduling](../RD/fy25/candidates/02-push-notification-system.md)
- [RD/fy25/03-push-notifications/README.md](../RD/fy25/03-push-notifications/README.md)
- [RD/fy25/03-push-notifications/experiments.md](../RD/fy25/03-push-notifications/experiments.md)

Observation:

- The docs correctly explain that `#1339` was the UI-only step and `#1346` was the scheduling step.
- Without that explanation, it reads like duplicate notification work.
- The issue was completed, but the backlog trail is easy to misread as duplicated scope.

## 5. Recommended Cleanup

1. Close or explicitly re-scope `#1306` if there is no remaining offline work.
2. Split the remaining `#1119` follow-up items into separate backlog items or mark them as non-blocking operational work.
3. Rewrite `#1161` into a proper PBI with pain, value, estimate, and measurable acceptance criteria.
4. Keep the performance, notification, and auth work grouped as epics so the overlapping stories stay understandable.
5. Normalize the evidence docs so issue status language and naming conventions are consistent.

## 6. Summary

- Exact duplicates: none obvious in the candidate set.
- Overlapping clusters: performance, Firebase/notifications, auth/platform migration.
- Already done but not fully completed: `#1119`, `#1230`, and the open `#1306` epic.
- Not ready: `#1161`.
- Hygiene fixes needed: duplicate doc formats, inconsistent status wording, and a few naming/typo issues.
