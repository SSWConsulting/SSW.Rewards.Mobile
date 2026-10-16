import { test, expect } from '@playwright/test';
import * as dotenv from 'dotenv';

dotenv.config();

const TEST_USERNAME = process.env.TEST_USERNAME;
const TEST_PASSWORD = process.env.TEST_PASSWORD;

/**
 * Refresh Token Verification
 *
 * Verifies SSW.Rewards.Mobile#1522 fix: the Admin Portal OIDC login should
 * now request `offline_access` and SSW.Identity should issue a refresh token.
 *
 * Forces a fresh login (ignores saved storage state), intercepts the
 * /connect/token response, and asserts `refresh_token` is present.
 *
 * Depends on:
 *   - SSW.IdentityServer staging deployed with offline_access in the
 *     ssw-rewards-admin-portal client's AllowedScopes (PR #348)
 *   - AdminUI running locally on https://localhost:7137 against staging identity
 *   - TEST_PASSWORD env var set
 */
test.describe('Refresh token issuance', () => {
  // Force a fresh login — don't reuse the shared storageState
  test.use({ storageState: { cookies: [], origins: [] } });

  test('/connect/token response contains refresh_token after login', async ({ page }) => {
    test.skip(!TEST_USERNAME || !TEST_PASSWORD, 'TEST_USERNAME / TEST_PASSWORD env vars not set');

    const tokenResponse = page.waitForResponse(
      (res) => res.url().includes('/connect/token') && res.request().method() === 'POST',
      { timeout: 60_000 }
    );

    await page.goto('https://localhost:7137');
    await page.waitForURL(/app-ssw-ident-staging-api\.azurewebsites\.net/, { timeout: 30_000 });

    await page.fill('input[type="email"]#Input_Username', TEST_USERNAME!);
    await page.fill('input[type="password"]#Input_Password', TEST_PASSWORD!);
    await page.click('button[name="Input.Button"][value="login"]');

    const response = await tokenResponse;
    expect(response.ok(), '/connect/token should return 2xx').toBeTruthy();

    const body = await response.json();
    console.log('📋 /connect/token response keys:', Object.keys(body));

    expect(body.access_token, 'access_token should be present').toBeTruthy();
    expect(
      body.refresh_token,
      'refresh_token should be present — confirms offline_access is granted and silent renewal will work without iframe fallback'
    ).toBeTruthy();

    // Sanity: the returned scope should include offline_access
    expect(body.scope, 'scope should include offline_access').toContain('offline_access');

    console.log('✅ refresh_token issued — #1522 fix verified end-to-end');

    // --- Exercise the refresh flow directly ---
    // Take the refresh_token we just got and POST it to /connect/token with
    // grant_type=refresh_token. This is exactly what oidc-client-ts does under
    // the hood when the access token nears expiry. Proves the full exchange
    // works server-side without waiting for the real access token to expire.
    const tokenEndpoint = new URL('/connect/token', response.url()).toString();
    const clientId = 'ssw-rewards-admin-portal';

    const refreshed = await page.request.post(tokenEndpoint, {
      form: {
        grant_type: 'refresh_token',
        client_id: clientId,
        refresh_token: body.refresh_token,
      },
    });

    expect(refreshed.ok(), `refresh_token grant should return 2xx (got ${refreshed.status()})`).toBeTruthy();
    const refreshedBody = await refreshed.json();
    console.log('🔄 refresh_token grant response keys:', Object.keys(refreshedBody));

    expect(refreshedBody.access_token, 'new access_token on refresh').toBeTruthy();
    expect(refreshedBody.refresh_token, 'refresh_token still present after refresh').toBeTruthy();
    expect(refreshedBody.token_type?.toLowerCase()).toBe('bearer');

    // Rotation is a server-side config concern (Duende grace period vs Reuse vs
    // OneTimeOnly) — not something we're verifying here. What matters is that
    // the refresh_token grant returns a usable access_token.
    const rotated = refreshedBody.refresh_token !== body.refresh_token;
    console.log(`🔄 refresh_token rotated: ${rotated} (informational — not asserted)`);
    console.log('✅ refresh_token grant exchange works — full refresh flow verified');
  });
});
