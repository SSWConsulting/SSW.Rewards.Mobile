# Full Backlog Refinement Report

Scope: the live GitHub issue backlog for `SSWConsulting/SSW.Rewards.Mobile` with no date filter.

Short version: the backlog is large, heavily labeled, and generally understandable, but triage quality is inconsistent. The main hygiene issues are label overlap, a large unlabeled open bucket, and a surprising number of closed issues that still carry open checklist items. The backlog is not a singularity, but it is producing its own heat.

## 1. Snapshot

- Total issues: 784
- Open: 232
- Closed: 552
- YakShaver-labeled issues: 75
- `Needs Refinement` issues: 64
- `ready` issues: 38
- Open unlabeled issues: 108
- Closed issues with open checklist items in the body: 162

## 2. Label Hygiene

### YakShaver / Needs Refinement

- Open `YakShaver` issues: 16
- Open `Needs Refinement` issues: 15
- Open issues with both labels: 15
- Open issues with `YakShaver` only: 1
- Open issues with `Needs Refinement` only: 0

Interpretation:

- In the live backlog, `Needs Refinement` is effectively a refinement-stage sublabel for YakShaver intake.
- That is fine if intentional, but the label pairing is inconsistent enough that it should be called out in refinement.

### `ready`

- Open `ready` issues: 22
- The `ready` bucket is a mix of older content work, ad-hoc feature requests, and technical debt.

Interpretation:

- The label probably means “can be scheduled”, but several of the items are old enough that they should be revalidated before anyone assumes they are still current.

## 3. Duplicate Or Overlapping PBIs

### Exact duplicate title clusters

I found 4 exact normalized title clusters:

- `#1250` and `#1251` - `🐛 Bug - Image Upload Failure in Rewards (Mobile app)`
- `#1351` and `#1352` - `✨ Improve Notification System UI and Functionality`
- `#173` and `#496` - `✨ Push notifications` / `Push Notifications`
- `#147` and `#737` - `SSW.Rewards: Sprint 20 Forecast`

### Open overlap cluster

- `#371` and `#870` - `✨ Mobile/Admin | Ad-Hoc questions` / `✨ API/Mobile/Admin | Ad-hoc questions`

### Large thematic overlap

- `#1484` to `#1508` - multi-tenancy / white-labeling / deployment series
- `#1510` to `#1515` - documentation and onboarding follow-ups for the same workstream

Interpretation:

- The multi-tenancy block is not a duplicate set, but it is a single workstream spread across many unlabeled issues.
- That is fine if it is an intentional epic decomposition; it is noisy if people are trying to triage the backlog by label alone.

## 4. Not Ready

### Open issues carrying both `YakShaver` and `Needs Refinement`

- `#1552` - `✨ Managed Identity - Migrate Azure services from connection string secrets to Managed Identity`
- `#1522` - `🐛 Bug - Token Expiry Handling Issue`
- `#1519` - `✨ Multi-Tenancy - Enable Separate Tenant for Done Videos`
- `#1442` - `✨ Leaderboard Enhancements - Improve Display and Refresh Features + video v3 [JK]`
- `#1400` - `💄 Bug - Align Production UI with Design Specifications`
- `#1363` - `✨ Add Pagination to Achievements in Admin Portal`
- `#1338` - `✨ Enhance Quiz Engagement in Rewards (Mobile app)`
- `#1324` - `✨ Skip Email Validation for OAuth Users`
- `#1320` - `✨ Integrate QR Code for SSW Employees in Rewards App`
- `#1319` - `✨ Add Feature to Explore Ways to Earn Points`
- `#1318` - `✨ Implement Role-Based Staff Detection for Multi-Tenancy`
- `#1306` - `✨ Implement Full Offline Support [JK]`
- `#1283` - `✨ Add Twitter Handles to Rewards (Mobile app) Leaderboard [JK]`
- `#1272` - `✨ Add Size Dropdown for Clothing Rewards`
- `#1265` - `✨ Simplify Sign-Up Process for Rewards (Mobile app)`
- `#1184` - `✨ Admin Portal - Implement Pagination and Optimize Leaderboard Data Handling`

Interpretation:

- These are the backlog items most obviously needing grooming before they can be considered planning-ready.
- `#1552` is the one `YakShaver` item that is not yet marked `Needs Refinement`, so it is worth checking whether it should join the same pipeline as the others.

## 5. Unlabeled Open Issues

- Open unlabeled issues: 108
- Open unlabeled issues with no `Acceptance Criteria` text in the body: 36
- Open unlabeled issues with very short or empty bodies: 7

Obvious template omissions:

- `#530` - `✨ API - Location features` - empty body
- `#410` - `⚒️ Testing | Setup unit tests` - checklist-only body
- `#516` - `Admin - Use QR Monkey` - short description only
- `#521` - `Admin - Rewards Grid` - short description only
- `#524` - `Need favicon for admin portal` - short description only
- `#531` - `🐛 API - Broken builds are able to be merged` - one-line pain statement only
- `#1201` - `📄 Content - SSW Rules quiz` - one-line idea only

Interpretation:

- These are the clearest “missing template” candidates in the backlog.
- They do not all need the same treatment, but they do need either labels, acceptance criteria, or closure.

## 6. Already Done But Not Completed

### Closed issues with open checklist items

There are 162 closed issues whose bodies still contain unchecked checklist items.

High-signal examples:

- `#1242` - `⚡ Leaderboard | Only load 50 users at a time`
- `#1230` - `🐛Authentication - User is unexpectedly logged out of the app`
- `#1119` - `🐛 NDC Porto - lots of crashes (from lots of usage!)`
- `#1310` - `✨ Offline Support | Redeem Tab`
- `#1350` - `🐛 Notifications not working on staging`
- `#1435` - `'Me' button on leaderboard page doesn’t scroll to my ranking on first tap`
- `#1531` - `🐛 Bug - App crashes in offline mode when visiting the network tab twice`
- `#1541` - `🐛 Mobile (Android) | Crash`

Interpretation:

- This is the biggest backlog hygiene problem in the repo.
- Closed status is not consistently aligned with completion criteria, which makes historical backlog analysis noisy.

## 7. What To Call Out In Refinement

1. `#1552` - Managed Identity migration
2. `#1522` - Token expiry handling
3. `#1519` - Multi-tenancy done-videos tenant split
4. `#1484` to `#1508` - multi-tenancy / white-label epic cluster
5. `#530` - empty-body API location feature
6. `#1242` - leaderboard pagination item that still shows open checklist items after closure
7. `#1119` and `#1230` - closed stories that still read as incomplete
8. `#371` / `#870` - open duplicate ad-hoc questions

## 8. Recommended Cleanup

1. Decide whether `YakShaver` and `Needs Refinement` are a single pipeline or two labels with distinct meaning.
2. Revalidate the 22 open `ready` issues, especially the oldest items (`#225`, `#307`, `#353`-`#376`, `#526`, `#759`, `#763`, `#770`, `#773`, `#824`).
3. Add labels to the multi-tenancy / white-label cluster so it stops hiding in the unlabeled bucket.
4. Rewrite or close the seven obvious template omissions.
5. Audit the 162 closed issues with open checklist items and either finish the checklist or split the follow-up into new issues.
