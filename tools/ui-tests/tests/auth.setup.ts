import { test as setup, expect } from '@playwright/test';
import * as dotenv from 'dotenv';

dotenv.config();

const authFile = '.auth/user.json';
const TEST_USERNAME = process.env.TEST_USERNAME || 'jernej.kavka@gmail.com';
const TEST_PASSWORD = process.env.TEST_PASSWORD;

/**
 * Authentication Setup
 * 
 * This setup test authenticates once and saves the session state.
 * All other tests will reuse this authenticated state.
 * 
 * Run this manually if authentication expires:
 *   npx playwright test auth.setup.ts
 */
setup('authenticate and save session', async ({ page }) => {
  console.log('\n🔐 Starting authentication...');
  
  // Navigate to the app
  await page.goto('https://localhost:7137');
  
  // Wait for redirect to SSW Identity login page
  await page.waitForURL(/app-ssw-ident-staging-api\.azurewebsites\.net/, { timeout: 30000 });
  console.log('✅ Redirected to SSW Identity login page');
  
  // Fill in login credentials
  await page.fill('input[type="email"]#Input_Username', TEST_USERNAME);
  await page.fill('input[type="password"]#Input_Password', TEST_PASSWORD);
  console.log(`✅ Filled credentials for: ${TEST_USERNAME}`);
  
  // Click login button
  await page.click('button[name="Input.Button"][value="login"]');
  
  // Wait for redirect back to the app
  await page.waitForURL(/localhost:7137/, { timeout: 30000 });
  console.log('✅ Successfully authenticated and redirected back');
  
  // Verify we're logged in by checking for MudBlazor AppBar or main content
  // AdminUI uses MudBlazor, so look for .mud-appbar or main content area
  const appBarOrMain = page.locator('.mud-appbar, .mud-main-content, main').first();
  await expect(appBarOrMain).toBeVisible({ timeout: 10000 });
  console.log('✅ Main content visible - user is authenticated');
  
  // Save the authenticated state
  await page.context().storageState({ path: authFile });
  console.log(`✅ Session saved to: ${authFile}`);
  
  console.log('🎉 Authentication setup complete!\n');
});
