import { test, expect } from '@playwright/test';

/**
 * DOM Structure Inspection Tests
 * 
 * These tests help understand the DOM structure and verify
 * that CSS selectors and component structures are correct.
 * Useful for debugging styling issues and understanding MudBlazor components.
 */

test.describe('DOM Inspection', () => {
  test.use({ storageState: '.auth/user.json' });

  test('inspect SendNotification page structure', async ({ page }) => {
    console.log('\n🔍 Inspecting SendNotification page DOM...');
    
    await page.goto('https://localhost:7137/send-notification');
    await page.waitForLoadState('networkidle');
    
    // Verify page loaded
    await expect(page.getByRole('heading', { name: /create notification/i })).toBeVisible();
    console.log('✅ Page loaded successfully');
    
    // Inspect form sections
    const sections = await page.locator('h5, h3').allTextContents();
    console.log('\n📋 Form sections found:', sections);
    
    // Inspect radio buttons
    const radioButtons = await page.locator('input[type="radio"]').count();
    console.log(`📻 Radio buttons: ${radioButtons}`);
    
    // Inspect text inputs
    const textInputs = await page.locator('input[type="text"], input.mud-input-slot').count();
    console.log(`📝 Text inputs: ${textInputs}`);
    
    // Inspect autocomplete fields
    const autocompletes = await page.locator('.mud-autocomplete').count();
    console.log(`🔎 Autocomplete fields: ${autocompletes}`);
    
    // Inspect date/time pickers
    const pickers = await page.locator('.mud-picker').count();
    console.log(`📅 Date/Time pickers: ${pickers}`);
    
    console.log('\n🎉 DOM inspection complete!\n');
  });

  test('inspect input field classes and structure', async ({ page }) => {
    console.log('\n🔍 Inspecting input field structure...');
    
    await page.goto('https://localhost:7137/send-notification');
    await page.waitForLoadState('networkidle');
    
    // Select Achievement to enable fields
    await page.getByTestId('target-achievement').click();
    await page.waitForTimeout(500);
    
    // Inspect title input structure
    const titleInput = page.locator('input[aria-label*="notification title"]').first();
    
    const structure = await titleInput.evaluate((el) => {
      const getClasses = (element: Element | null, depth: number = 0): string => {
        if (!element || depth > 4) return '';
        const tag = element.tagName.toLowerCase();
        const classes = element.className ? `.${element.className.replace(/\s+/g, '.')}` : '';
        const result = `${'  '.repeat(depth)}${tag}${classes}\n`;
        return result + getClasses(element.parentElement, depth + 1);
      };
      return getClasses(el);
    });
    
    console.log('\n📦 Title Input DOM Structure:');
    console.log(structure);
    
    // Check CSS variable
    const smokeyWhite = await page.evaluate(() => {
      return getComputedStyle(document.documentElement).getPropertyValue('--smokey-white').trim();
    });
    console.log(`🎨 CSS Variable --smokey-white: ${smokeyWhite}`);
    
    console.log('\n🎉 Structure inspection complete!\n');
  });

  test('verify CSS styling on focused inputs', async ({ page }) => {
    console.log('\n🎨 Verifying CSS styling...');
    
    await page.goto('https://localhost:7137/send-notification');
    await page.waitForLoadState('networkidle');
    
    // Select Achievement
    await page.getByTestId('target-achievement').click();
    await page.waitForTimeout(300);
    
    // Focus title input
    const titleInput = page.locator('input[aria-label*="notification title"]').first();
    await titleInput.click();
    await page.waitForTimeout(300);
    
    // Get computed styles
    const bgColor = await titleInput.evaluate((el) => window.getComputedStyle(el).backgroundColor);
    const textColor = await titleInput.evaluate((el) => window.getComputedStyle(el).color);
    
    console.log(`📝 Title input (focused):`);
    console.log(`   Background: ${bgColor}`);
    console.log(`   Text color: ${textColor}`);
    
    // Verify smokey white background
    expect(bgColor).toBe('rgb(247, 247, 247)');
    expect(textColor).toBe('rgb(0, 0, 0)');
    console.log('✅ Smokey white styling verified!');
    
    // Test autocomplete styling
    const achievementInput = page.locator('input[aria-label*="required achievement"]').first();
    await achievementInput.click();
    await page.waitForTimeout(300);
    
    const autocompleteBg = await achievementInput.evaluate((el) => window.getComputedStyle(el).backgroundColor);
    const autocompleteText = await achievementInput.evaluate((el) => window.getComputedStyle(el).color);
    
    console.log(`\n🔎 Autocomplete input (focused):`);
    console.log(`   Background: ${autocompleteBg}`);
    console.log(`   Text color: ${autocompleteText}`);
    
    expect(autocompleteBg).toBe('rgb(247, 247, 247)');
    expect(autocompleteText).toBe('rgb(0, 0, 0)');
    console.log('✅ Autocomplete styling verified!');
    
    console.log('\n🎉 CSS verification complete!\n');
  });
});
