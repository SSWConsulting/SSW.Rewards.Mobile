# Architecture Decision Records

This folder contains Architecture Decision Records (ADRs) for SSW.Rewards.Mobile.

## Purpose

Use ADRs to document important technical decisions, their context, and tradeoffs.

## When to write an ADR

1. High-impact decisions.
2. Hard-to-reverse decisions.
3. Decisions with multiple viable options.
4. Decisions that need cross-team visibility.

## File naming

Use date-first filenames:

- `YYYYMMDD-short-kebab-case-title.md`
- Example: `20260209-tenant-settings-source-of-truth.md`

## Status lifecycle

1. `proposed`
2. `accepted`
3. `deprecated`
4. `superseded` (with link to replacement ADR)

## Minimum required fields

1. Status
2. Deciders
3. Date
4. Context
5. Considered options
6. Decision outcome
7. Consequences

## Suggested CLI workflow (Log4brains style)

```bash
npm install -g log4brains
log4brains adr new
log4brains preview
```

## Initial ADR backlog for tenant configuration

1. Tenant settings source of truth (DB vs Key Vault vs appsettings).
2. Tenant resolution strategy (host/header/claim/fallback).
3. Cache invalidation strategy for tenant settings.
4. Rollout and migration strategy for existing environments.
