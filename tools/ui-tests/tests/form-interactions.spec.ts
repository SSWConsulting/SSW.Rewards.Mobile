import { test, expect } from '@playwright/test';

/**
 * Form Interaction Tests (Non-Destructive)
 * 
 * These tests demonstrate how to interact with forms and UI elements
 * without actually submitting or creating data.
 * Useful for verifying form behavior, validation, and user interactions.
 */

test.describe('Form Interactions', () => {
  test.use({ storageState: '.auth/user.json' });

  test('populate create notification form (no submit)', async ({ page }) => {
    console.log('\n📝 Testing notification form population...');
    
    await page.goto('https://localhost:7137/send-notification');
    await page.waitForLoadState('networkidle');
    
    // Verify form loaded
    await expect(page.getByRole('heading', { name: /create notification/i })).toBeVisible();
    console.log('✅ Form loaded');
    
    // Step 1: Select delivery option
    console.log('\n1️⃣  Selecting delivery option...');
    await page.getByTestId('delivery-schedule').click();
    await page.waitForTimeout(300);
    console.log('✅ Selected: Schedule');
    
    // Verify date/time fields are visible (they're always visible, just disabled/enabled)
    // MudDatePicker renders as a complex component, just verify it's visible
    const datePicker = page.getByTestId('schedule-date');
    await expect(datePicker).toBeVisible();
    console.log('✅ Date picker visible');
    
    // Step 2: Select targeting option
    console.log('\n2️⃣  Selecting target audience...');
    await page.getByTestId('target-achievement').click();
    await page.waitForTimeout(300);
    console.log('✅ Selected: Requires Achievement');
    
    // Step 3: Fill achievement autocomplete
    console.log('\n3️⃣  Searching for achievement...');
    const achievementInput = page.locator('input[aria-label*="required achievement"]').first();
    await achievementInput.click();
    await achievementInput.fill('attended');
    await page.waitForTimeout(500);
    
    // Check if dropdown appeared
    const dropdownVisible = await page.locator('.mud-popover-open').isVisible().catch(() => false);
    if (dropdownVisible) {
      console.log('✅ Autocomplete dropdown appeared');
    }
    console.log('✅ Achievement search: "attended"');
    
    // Step 4: Fill notification details
    console.log('\n4️⃣  Filling notification details...');
    
    const titleInput = page.locator('input[aria-label*="notification title"]').first();
    await titleInput.fill('Test Notification Title');
    console.log('✅ Title filled');
    
    const bodyInput = page.locator('textarea[aria-label*="notification body"]').first();
    await bodyInput.fill('This is a test notification body for verification purposes.');
    console.log('✅ Body filled');
    
    const imageInput = page.getByTestId('notification-image-url');
    await imageInput.fill('https://example.com/image.png');
    console.log('✅ Image URL filled');
    
    // Step 5: Verify form state (DO NOT SUBMIT)
    console.log('\n5️⃣  Verifying form state...');
    const submitButton = page.getByTestId('submit-notification');
    const isSubmitEnabled = await submitButton.isEnabled();
    
    console.log(`Submit button enabled: ${isSubmitEnabled}`);
    
    // Take screenshot of filled form
    await page.screenshot({ path: 'screenshots/form-filled.png', fullPage: true });
    console.log('✅ Screenshot saved: screenshots/form-filled.png');
    
    console.log('\n🎉 Form interaction test complete (no data created)!\n');
  });

  test('test form validation behavior', async ({ page }) => {
    console.log('\n✅ Testing form validation...');
    
    await page.goto('https://localhost:7137/send-notification');
    await page.waitForLoadState('networkidle');
    
    // Try to submit empty form
    const submitButton = page.getByTestId('submit-notification');
    
    // Click submit without filling required fields
    await submitButton.click();
    await page.waitForTimeout(500);
    
    // Should NOT navigate away (form validation should prevent it)
    expect(page.url()).toContain('/send-notification');
    console.log('✅ Form validation prevented empty submission');
    
    // Check for validation messages
    const hasValidationErrors = await page.locator('.mud-input-error, .mud-error-text').count() > 0;
    console.log(`Validation errors visible: ${hasValidationErrors}`);
    
    console.log('\n🎉 Validation test complete!\n');
  });

  test('test radio button and conditional fields', async ({ page }) => {
    console.log('\n🔘 Testing conditional field visibility...');
    
    await page.goto('https://localhost:7137/send-notification');
    await page.waitForLoadState('networkidle');
    
    // Test Delivery options
    console.log('\n📅 Testing Schedule fields...');
    await page.getByTestId('delivery-now').click();
    await page.waitForTimeout(200);
    
    // Date/time pickers should have disabled attribute
    // MudBlazor adds 'disabled' class or attribute to the component
    let datePicker = page.getByTestId('schedule-date');
    // Check if parent has disabled class or aria-disabled
    const hasDisabled = await datePicker.evaluate((el) => {
      return el.hasAttribute('disabled') || 
             el.classList.contains('mud-disabled') ||
             el.querySelector('.mud-disabled') !== null;
    });
    expect(hasDisabled).toBeTruthy();
    console.log('✅ Schedule fields disabled when "Now" selected');
    
    await page.getByTestId('delivery-schedule').click();
    await page.waitForTimeout(200);
    
    // Select "Schedule" option again
    console.log('\n3️⃣  Switching to Schedule...');
    await page.getByTestId('delivery-schedule').click();
    await page.waitForTimeout(300);
    console.log('✅ Selected: Schedule');
    
    // Now date/time should be enabled (not disabled)
    datePicker = page.getByTestId('schedule-date');
    const hasDisabledAfterSchedule = await datePicker.evaluate((el) => {
      return el.hasAttribute('disabled') || 
             el.classList.contains('mud-disabled') ||
             el.querySelector('.mud-disabled') !== null;
    });
    expect(hasDisabledAfterSchedule).toBeFalsy();
    console.log('✅ Schedule fields enabled when "Schedule" selected');
    
    // Test Target options
    console.log('\n🎯 Testing Target fields...');
    await page.getByTestId('target-everyone').click();
    await page.waitForTimeout(200);
    
    const achievementInput = page.locator('input[aria-label*="required achievement"]').first();
    let achievementDisabled = await achievementInput.isDisabled();
    expect(achievementDisabled).toBeTruthy();
    console.log('✅ Achievement field disabled when "Everyone" selected');
    
    await page.getByTestId('target-achievement').click();
    await page.waitForTimeout(200);
    
    achievementDisabled = await achievementInput.isDisabled();
    expect(achievementDisabled).toBeFalsy();
    console.log('✅ Achievement field enabled when "Requires Achievement" selected');
    
    await page.getByTestId('target-role').click();
    await page.waitForTimeout(200);
    
    const roleInput = page.locator('input[aria-label*="role"]').first();
    const roleDisabled = await roleInput.isDisabled();
    expect(roleDisabled).toBeFalsy();
    console.log('✅ Role field enabled when "Requires Role" selected');
    
    console.log('\n🎉 Conditional field test complete!\n');
  });
});
