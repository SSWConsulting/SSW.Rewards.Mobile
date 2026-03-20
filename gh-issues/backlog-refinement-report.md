# Backlog / PBI Source Map

## Summary

The repository’s live PBI source of truth is Beads, stored in `.beads/issues.jsonl` and backed by `.beads/beads.db`.
Everything else is either intake scaffolding, draft issue packs, or evidence/reference material.

Current live backlog items in Beads:

- `SSW.Rewards.Mobile-dnv` - `Auth refresh improvements`
- `SSW.Rewards.Mobile-okv` - `AGENTS.md, .agents and skills update`
- `SSW.Rewards.Mobile-vqj` - `Leaderboard kiosk enhancements`

## Live PBI Store

| Path | Role | Notes |
|------|------|-------|
| `/.beads/issues.jsonl` | Primary issue database export | Contains the active PBI list in JSONL form. |
| `/.beads/beads.db` | Beads SQLite backing store | Database schema includes `issues` and `dependencies`. |
| `/.beads/metadata.json` | Beads export metadata | Points to `issues.jsonl` as the export target. |
| `/.beads/config.yaml` | Beads runtime config | Issue-tracking runtime configuration. |
| `/.beads/README.md` | Beads usage notes | Confirms issues are git-native and synced from `.beads/issues.jsonl`. |

## Intake / Issue Templates

| Path | Role | Notes |
|------|------|-------|
| `/.github/ISSUE_TEMPLATE/bug-report.md` | Bug issue template | Standard issue intake with tasks and acceptance criteria. |
| `/.github/ISSUE_TEMPLATE/feature-request.md` | Feature issue template | Uses emoji-prefixed title and feature labels. |
| `/.github/ISSUE_TEMPLATE/docs.md` | Documentation issue template | Same acceptance-criteria pattern. |
| `/.github/ISSUE_TEMPLATE/refactor.md` | Refactor issue template | For technical-debt style PBIs. |
| `/.github/ISSUE_TEMPLATE/content-change.md` | Content change template | Used for content-only requests. |
| `/.github/ISSUE_TEMPLATE/ssw-rewards--sprint-forecast-template.md` | Sprint forecast issue template | Builds sprint PBI forecasts from a planned PBI list. |
| `/.github/ISSUE_TEMPLATE/ssw-rewards--sprint-review-template.md` | Sprint review issue template | Records which PBIs were done in a sprint. |
| `/.github/pull_request_template.md` | PR template | Requires a PBI link/reason, so backlog items are expected to originate in issues. |
| `/.github/settings.yml` | Repo settings / branch protection | Issues are enabled; branch protection expects review/CI discipline. |

## Draft / Derived Backlog Sources

| Path | Role | Notes |
|------|------|-------|
| `/.sandbox/github-issues/security-overview.md` | Draft issue pack | Proposed security PBIs with priority, dependencies, and recommended bundles. |
| `/.sandbox/github-issues/security/*.md` | Draft security PBIs | Individual proposed issues, not live backlog records. |
| `/RD/fy25/README.md` | FY25 R&D index | Evidence index that maps activities to candidate documents and issue links. |
| `/RD/fy25/candidates/*.md` | R&D candidate PBIs | Candidate activity writeups that reference GitHub issue numbers and PRs. |
| `/RD/fy25/docs/evidence/*.json` | Evidence inputs | Supporting data for candidate analysis, not backlog items themselves. |

## Readiness / Hygiene References

| Path | Role | Notes |
|------|------|-------|
| `/_docs/Definition-of-Ready.md` | Ready criteria | Useful for judging whether a PBI is ready for sprint planning. |
| `/_docs/Definition-of-Done.md` | Done criteria | Useful for distinguishing “implemented” from “completed.” |
| `/_docs/Business.md` and related docs | Business context | Supporting reference material, not backlog storage. |

## Naming Conventions

- Beads issue IDs use the repo prefix plus a short suffix, for example `SSW.Rewards.Mobile-dnv`.
- GitHub issue templates use emoji-prefixed titles such as `🐛`, `✨`, `📝`, and `♻️`.
- Sprint templates use `SSW.Rewards: Sprint {{ XX }}` naming.
- FY25 candidate documents use numbered filenames like `01-...md`, `02-...md`, etc.

## Obvious Backlog Sources

1. `.beads/issues.jsonl` is the only live PBI store found.
2. `.github/ISSUE_TEMPLATE/*` defines how new PBIs enter the system.
3. `.sandbox/github-issues/security-overview.md` is a prepared backlog of proposed security PBIs.
4. `RD/fy25/candidates/*.md` contains research-backed candidate PBIs and issue cross-references.

## Notes

- I did not find a second active backlog database or a separate `backlog/` directory.
- The Beads data is the closest thing to a source of truth; the rest are supporting artifacts.
- The repo is tidy enough that the backlog is mostly behaving itself, which is unsettling but welcome.
