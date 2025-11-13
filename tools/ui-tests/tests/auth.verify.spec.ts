import { test, expect } from '@playwright/test';

/**
 * Authentication Verification Tests
 * 
 * These tests verify that authentication is working correctly.
 * They run with the saved authentication state.
 */

test.describe('Authentication Verification', () => {
  test.use({ storageState: '.auth/user.json' });

  test('should be authenticated and access protected pages', async ({ page }) => {
    console.log('\n🔍 Verifying authentication status...');
    
    // Navigate to a protected page
    await page.goto('https://localhost:7137/notifications');
    await page.waitForLoadState('networkidle');
    
    // Should NOT be redirected to login
    expect(page.url()).not.toContain('app-ssw-ident-staging-api.azurewebsites.net');
    expect(page.url()).toContain('localhost:7137');
    console.log('✅ Not redirected to login - authentication valid');
    
    // Should see page content
    await expect(page.getByRole('heading', { name: /notifications/i })).toBeVisible({ timeout: 10000 });
    console.log('✅ Protected page content visible');
    
    // Should see MudBlazor navigation or main content
    await expect(page.locator('.mud-appbar, .mud-main-content, main').first()).toBeVisible();
    console.log('✅ Main content visible - user authenticated');
    
    console.log('🎉 Authentication verification passed!\n');
  });

  test('should have valid session cookies', async ({ page }) => {
    console.log('\n🍪 Verifying session cookies...');
    
    await page.goto('https://localhost:7137');
    await page.waitForLoadState('networkidle');
    
    // Get cookies
    const cookies = await page.context().cookies();
    
    // Check for authentication cookies
    const hasAuthCookies = cookies.some(c => 
      c.name.includes('Identity') || 
      c.name.includes('AspNetCore') ||
      c.name.includes('idsrv')
    );
    
    expect(hasAuthCookies).toBeTruthy();
    console.log(`✅ Found ${cookies.length} cookies, including auth cookies`);
    console.log('🎉 Cookie verification passed!\n');
  });
});
