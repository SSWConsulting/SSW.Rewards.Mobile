# Use ADRs For Architecture Decisions

- Status: proposed
- Deciders: @jernejk
- Date: 2026-02-09
- Tags: process documentation architecture

Technical Story: https://github.com/SSWConsulting/SSW.Rewards.Mobile/issues/1485

## Context and Problem Statement

Major technical choices are currently scattered across issue comments, PR discussions, and ad-hoc documentation. This makes it hard to understand why decisions were made and increases the risk of re-litigating or reversing decisions without context.

## Decision Drivers

- Improve clarity and historical traceability.
- Reduce rework from repeated architecture debates.
- Align with SSW guidance and templates.

## Considered Options

1. Keep decisions in PRs/issues only.
2. Use markdown ADRs in repository.
3. Use wiki pages for decisions.

## Decision Outcome

Chosen option: "Use markdown ADRs in repository", because it keeps decisions versioned with code, searchable in PRs, and lightweight for contributors.

## Consequences

- ✅ Decisions become explicit and discoverable in one place.
- ✅ New team members can understand historical context faster.
- ❌ Adds a small process overhead for high-impact changes.

## Pros and Cons of the Options

### Keep decisions in PRs/issues only

- ✅ No additional process.
- ❌ Context becomes fragmented and hard to track.

### Use markdown ADRs in repository

- ✅ Versioned, reviewable, and easy to reference from code/PRs.
- ❌ Requires discipline to keep status updated.

### Use wiki pages for decisions

- ✅ Familiar for non-developers.
- ❌ Drifts away from code and PR review workflows.

## Links

- https://github.com/SSWConsulting/SSW.VerticalSliceArchitecture/blob/main/docs/adr/README.md
- https://github.com/SSWConsulting/SSW.VerticalSliceArchitecture/blob/main/docs/adr/template.md
- https://github.com/SSWConsulting/SSW.Rules.Content/blob/main/public/uploads/rules/architectural-decision-records/rule.mdx
