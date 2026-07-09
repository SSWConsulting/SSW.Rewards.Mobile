# Tenant Settings Source of Truth

- Status: proposed
- Deciders: @jernejk, @ZacharyKeeping
- Date: 2026-02-20
- Tags: tenant-config, security, database, key-vault

Technical Story: https://github.com/SSWConsulting/SSW.Rewards.Mobile/issues/1485, https://github.com/SSWConsulting/SSW.Rewards.Mobile/pull/1533

## Context and Problem Statement

The tenant configuration system needs a clear strategy for where different types of settings are stored. Some settings (branding, URLs, contact info) are non-sensitive and may change frequently via AdminUI. Other settings (API keys, connection strings, credentials) are highly sensitive and should never be exposed in database queries or admin interfaces.

## Decision Drivers

- Security: Sensitive credentials must not be stored in database or exposed via APIs
- Auditability: Changes to configuration should be tracked
- Performance: Frequently accessed settings need efficient caching
- Operability: Settings should be easy to manage for different environments
- Flexibility: Support runtime updates for non-sensitive settings without redeployment

## Considered Options

1. Everything in database
2. Everything in Azure Key Vault
3. Everything in appsettings.json / environment variables
4. Split by sensitivity: Public config in DB + secrets in Key Vault
5. Hybrid: DB + Key Vault references (reference pattern)

## Decision Outcome

Chosen option: **"Split by sensitivity: Public config in DB + secrets in Key Vault"**, because it provides the best balance of security, flexibility, and operational simplicity.

**Implementation approach:**

- **Database**: Branding, URLs, colors, public contact info, feature toggles, social media links
- **Key Vault**: API keys, credentials, connection strings, secrets for external services
- **Environment/appsettings**: Infrastructure-level config (Key Vault URL, connection strings for DB itself)

## Consequences

- ✅ Sensitive values never appear in database backups or query logs
- ✅ AdminUI can safely expose tenant settings editing without exposing secrets
- ✅ Database settings support proper audit trails via BaseAuditableEntity
- ✅ Clear separation of concerns: business config vs operational secrets
- ✅ Non-sensitive settings can be updated at runtime without redeployment
- ❌ Need to maintain two distinct configuration sources
- ❌ Requires Key Vault setup and access policies for all environments
- ❌ More complex initial setup and documentation burden

## Pros and Cons of the Options

### Everything in database

- ✅ Single source of truth
- ✅ Easy to query and manage from AdminUI
- ✅ Built-in audit trail
- ❌ Exposes sensitive credentials in database
- ❌ Harder to secure against SQL injection or insider threats
- ❌ Database backups contain secrets

### Everything in Azure Key Vault

- ✅ Maximum security for all values
- ✅ Fine-grained access control
- ✅ Azure-native secret rotation
- ❌ Overkill for non-sensitive branding config
- ❌ No audit trail for non-secret changes
- ❌ Can't easily edit from AdminUI
- ❌ Higher latency and API call costs

### Everything in appsettings.json / environment variables

- ✅ Simple and fast
- ✅ No runtime dependencies
- ❌ Requires redeployment for any change
- ❌ No audit trail
- ❌ Secrets checked into source control (if not careful)
- ❌ No multi-tenant support

### Split by sensitivity (CHOSEN)

- ✅ Right tool for each use case
- ✅ Secure by default for sensitive data
- ✅ Flexible and manageable for business config
- ✅ Supports runtime updates for non-secrets
- ✅ Clear documentation boundary
- ❌ Two systems to maintain
- ❌ Need provider abstraction layer

### Hybrid (Key Vault references in DB)

- ✅ Single source of truth in DB
- ✅ References make audit trail complete
- ❌ More complex to implement
- ❌ Still requires Key Vault integration
- ❌ Adds indirection layer

## Configuration Split Details

### Database (TenantSettings table)

**Branding:**

- CompanyName, CompanyLegalName, CompanyWebsiteUrl
- ApplicationName, ApplicationShortName, ApplicationTagline
- LogoUrl, FaviconUrl

**Colors:**

- PrimaryColor, SecondaryColor, AccentColor, BackgroundColor, TextColor

**Public Contact:**

- SupportEmail, MarketingEmail, StaffEmailDomain
- DefaultSenderEmail, DefaultSenderName
- ProfileDeletionRecipient

**External Services (public URLs):**

- ApiBaseUrl, IdentityServerUrl, QuizServiceUrl, AdminPortalUrl

**Social Media:**

- LinkedInUrl, TwitterUrl, FacebookUrl, InstagramUrl, YouTubeUrl

### Azure Key Vault

**Authentication & Authorization:**

- Auth0 ClientId, ClientSecret
- Azure AD ClientId, ClientSecret, TenantId

**External Service Credentials:**

- SendGrid API Key
- Azure Notification Hub connection strings
- Azure Maps API Key
- Application Insights instrumentation key

**Database:**

- Connection strings (if not using Managed Identity)

### Environment Variables / appsettings.json

**Infrastructure:**

- Key Vault URL/name
- Database connection string (for bootstrapping)
- ASP.NET Core environment settings
- CORS origins

## Implementation Plan

1. Create `ITenantSettingsProvider` interface in Application layer
2. Implement `TenantSettingsProvider` in Infrastructure with:
   - Database read for public settings
   - Key Vault client for secrets
   - Memory cache with configurable TTL
3. Document clearly in `TenantSettings` entity which fields are DB-backed vs Key Vault-backed
4. Add startup validation that fails fast if required secrets are missing
5. Create admin UI workflows for editing DB-backed settings only

## Links

- [Proposal: Tenant Configuration Roadmap](../Proposal-Tenant-Configuration-Roadmap.md)
- [Azure Key Vault Best Practices](https://learn.microsoft.com/en-us/azure/key-vault/general/best-practices)
- [SSW Rule: Do you know where to store your secrets?](https://www.ssw.com.au/rules/where-to-store-secrets)
