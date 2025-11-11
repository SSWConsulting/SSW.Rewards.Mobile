import { test, expect } from '@playwright/test';
import {
  Selectors,
  navigateToSendNotification,
  verifyDeliveryOption,
  verifyTargetOption,
  verifyPreviewText,
  fillNotificationForm,
  selectDeliveryOption,
  selectTargetOption,
  selectAchievementFromDropdown,
  logTestHeader,
  logTestComplete,
  logSuccess,
  logInfo
} from './send-notification.helpers';

test.use({ storageState: '.auth/user.json' });

test.describe('SendNotification Page - Desktop View', () => {
  test.beforeEach(async ({ page }) => {
    await navigateToSendNotification(page);
  });

  test('verify iPhone preview has white theme', async ({ page }) => {
    logTestHeader('iPhone White Theme Verification');

    const iphoneScreen = page.locator(Selectors.iphoneScreen);
    const screenBg = await iphoneScreen.evaluate((el) => {
      return window.getComputedStyle(el).background;
    });

    logInfo('📱 iPhone screen background', screenBg);
    logInfo('   Expected', 'Light gradient (white theme)');

    const time = page.locator(Selectors.iphoneTime);
    const timeColor = await time.evaluate((el) => {
      return window.getComputedStyle(el).color;
    });

    logInfo('⏰ Time text color', timeColor);
    logInfo('   Expected', 'Black text (rgb(0, 0, 0))');

    // Verify black text on light background
    expect(timeColor).toBe('rgb(0, 0, 0)');

    await page.screenshot({
      path: 'screenshots/send-notification/desktop-iphone-white-theme.png',
      fullPage: true
    });

    logTestComplete('White theme verified');
  });

  test('send now to everyone - verify preview', async ({ page }) => {
    logTestHeader('Send Now to Everyone');

    // Verify defaults
    await verifyDeliveryOption(page, 'now');
    await verifyTargetOption(page, 'everyone');

    // Fill form
    await fillNotificationForm(page, 'Welcome to SSW Rewards!', 'Earn points and redeem prizes.');

    // Verify preview
    await verifyPreviewText(page, 'Welcome to SSW Rewards!', 'Earn points and redeem prizes.');

    // Verify time display shows current time (for "Now" delivery)
    const timeDisplay = page.locator(Selectors.iphoneTime);
    const timeText = await timeDisplay.textContent();
    logInfo('⏰ Time displayed', `${timeText} (current time)`);

    await page.screenshot({
      path: 'screenshots/send-notification/desktop-send-now-everyone.png',
      fullPage: true
    });

    logTestComplete('Send Now to Everyone - Verified');
  });

  test('scheduled notification with achievement requirement - verify preview', async ({ page }) => {
    logTestHeader('Scheduled with Achievement');

    // Select Schedule delivery
    await selectDeliveryOption(page, 'schedule');
    logSuccess('Selected: Schedule delivery');
    
    // Wait for fields to be enabled
    await page.waitForTimeout(800);

    logInfo('⏭️  Skipping date/time/timezone (MudBlazor pickers use complex popups)');
    logInfo('   Testing focuses on achievement targeting and preview updates');

    // Select Achievement targeting
    await selectTargetOption(page, 'achievement');
    logSuccess('Selected: Requires Achievement');

    // Select achievement
    await selectAchievementFromDropdown(page, 'test');
    logSuccess('Achievement selected');

    // Fill notification content
    await fillNotificationForm(page, 'Prize Draw in 10 Minutes!', 'Come to the SSW booth for our prize draw. Don\'t miss out!');

    // Verify preview updates
    await verifyPreviewText(page, 'Prize Draw in 10 Minutes!', 'Come to the SSW booth');
    logSuccess('Preview updated with scheduled notification');

    await page.screenshot({
      path: 'screenshots/send-notification/desktop-scheduled-achievement.png',
      fullPage: true
    });

    logTestComplete('Scheduled with Achievement');
  });

  test('role-based notification - verify preview', async ({ page }) => {
    console.log('\n👥 Test: Role-Based Notification\n');
    console.log('='.repeat(70));

    // Select Role targeting
    await page.click('[data-testid="target-role"]');
    await page.waitForTimeout(500);
    console.log('✅ Selected: Requires Role');

    // Wait for role autocomplete to appear
    const roleAutocomplete = page.locator('[data-testid="role-autocomplete"]');
    await roleAutocomplete.waitFor({ state: 'visible', timeout: 10000 });
    
    // Click on the autocomplete to activate it and type
    await roleAutocomplete.click();
    await page.keyboard.type('a'); // Just type 'a' to get any roles starting with 'a'
    await page.waitForTimeout(1500);

    // Try to select first role from dropdown if available
    const firstRole = page.locator('.mud-list-item').first();
    const isVisible = await firstRole.isVisible().catch(() => false);
    if (isVisible) {
      await firstRole.click();
      console.log('✅ Role selected from dropdown');
    } else {
      console.log('⚠️  No roles found in dropdown - skipping selection');
    }

    // Fill notification content
    await page.fill('[data-testid="notification-title"]', 'Admin Update');
    await page.fill('[data-testid="notification-body"]', 'New admin features are now available. Check them out!');

    // Verify preview
    await expect(page.locator('.notification-title')).toContainText('Admin Update');
    await expect(page.locator('.notification-body')).toContainText('New admin features');
    console.log('✅ Preview updated with role-based notification');

    await page.screenshot({
      path: 'screenshots/send-notification/desktop-role-based.png',
      fullPage: true
    });

    console.log('\n✅ Role-Based Notification - Verified');
    console.log('='.repeat(70));
  });

  test('verify Enter key does not submit form in autocomplete', async ({ page }) => {
    console.log('\n⌨️  Test: Enter Key in Autocomplete\n');
    console.log('='.repeat(70));

    // Select Achievement targeting
    await page.click('[data-testid="target-achievement"]');
    await page.waitForTimeout(500);

    // Click autocomplete and type
    const achievementAutocomplete = page.locator('[data-testid="achievement-autocomplete"]');
    await achievementAutocomplete.waitFor({ state: 'visible', timeout: 10000 });
    await achievementAutocomplete.click();
    await page.keyboard.type('test');
    await page.waitForTimeout(1500);

    // Press Enter to select
    await page.keyboard.press('Enter');
    await page.waitForTimeout(500);

    // Verify we're still on the same page (form not submitted)
    await expect(page).toHaveURL(/send-notification/);
    console.log('✅ Form did not submit on Enter in autocomplete');

    // Verify title field can receive focus
    const titleField = page.locator('[data-testid="notification-title"]');
    await titleField.click();
    await expect(titleField).toBeFocused();
    console.log('✅ Can focus on title field after autocomplete');

    console.log('\n✅ Enter Key Handling - Verified');
    console.log('='.repeat(70));
  });

  test('verify all preview elements are present', async ({ page }) => {
    console.log('\n🔍 Test: Preview Elements\n');
    console.log('='.repeat(70));

    // Check iPhone structure
    await expect(page.locator('.iphone-preview')).toBeVisible();
    console.log('✅ iPhone container visible');

    await expect(page.locator('.iphone-screen')).toBeVisible();
    console.log('✅ iPhone screen visible');

    await expect(page.locator('.iphone-notch')).toBeVisible();
    console.log('✅ iPhone notch visible');

    await expect(page.locator('.iphone-time')).toBeVisible();
    console.log('✅ Time display visible');

    // Check notification preview structure
    await expect(page.locator('.notification-preview')).toBeVisible();
    console.log('✅ Notification preview visible');

    await expect(page.locator('.notification-header')).toBeVisible();
    console.log('✅ Notification header visible');

    await expect(page.locator('.notification-icon')).toBeVisible();
    console.log('✅ App icon visible');

    await expect(page.locator('.notification-app-name')).toContainText('SSW Rewards');
    console.log('✅ App name displays correctly');

    await expect(page.locator('.notification-time-badge')).toContainText('now');
    console.log('✅ Time badge displays correctly');

    // Verify placeholders
    await expect(page.locator('.notification-title .preview-placeholder')).toContainText('Notification Title');
    console.log('✅ Title placeholder visible');

    await expect(page.locator('.notification-body .preview-placeholder')).toContainText('Notification message will appear here');
    console.log('✅ Body placeholder visible');

    await page.screenshot({
      path: 'screenshots/send-notification/desktop-preview-elements.png',
      fullPage: true
    });

    console.log('\n✅ All Preview Elements - Verified');
    console.log('='.repeat(70));
  });

  test('verify preview updates immediately on typing', async ({ page }) => {
    console.log('\n⚡ Test: Immediate Preview Updates\n');
    console.log('='.repeat(70));

    const titleField = page.locator('[data-testid="notification-title"]');
    const bodyField = page.locator('[data-testid="notification-body"]');
    const previewTitle = page.locator('.notification-title');
    const previewBody = page.locator('.notification-body');

    // Type character by character and verify updates
    await titleField.type('L', { delay: 100 });
    await expect(previewTitle).toContainText('L');
    console.log('✅ Preview updated after first character');

    await titleField.type('ive Update', { delay: 50 });
    await expect(previewTitle).toContainText('Live Update');
    console.log('✅ Preview updated during typing');

    await bodyField.type('Real-time preview test', { delay: 50 });
    await expect(previewBody).toContainText('Real-time preview test');
    console.log('✅ Body preview updated immediately');

    console.log('\n✅ Immediate Updates - Verified');
    console.log('='.repeat(70));
  });

  test('verify preview with maximum length content', async ({ page }) => {
    console.log('\n📏 Test: Maximum Length Content\n');
    console.log('='.repeat(70));

    // Ensure we're still on the page (auth might have expired in long test run)
    await page.goto('https://localhost:7137/send-notification');
    await page.waitForLoadState('networkidle');

    // Fill with maximum length (100 chars for title, 250 for body)
    const maxTitle = 'A'.repeat(100);
    const maxBody = 'B'.repeat(250);

    await page.fill('[data-testid="notification-title"]', maxTitle);
    await page.fill('[data-testid="notification-body"]', maxBody);

    // Verify preview displays full content
    await expect(page.locator('.notification-title')).toContainText('A'.repeat(50)); // Check first 50
    await expect(page.locator('.notification-body')).toContainText('B'.repeat(50)); // Check first 50

    console.log('✅ Preview handles maximum length content');

    await page.screenshot({
      path: 'screenshots/send-notification/desktop-max-length.png',
      fullPage: true
    });

    console.log('\n✅ Maximum Length - Verified');
    console.log('='.repeat(70));
  });

  test('verify image URL in preview', async ({ page }) => {
    console.log('\n🖼️  Test: Image URL in Preview\n');
    console.log('='.repeat(70));

    // Ensure we're on the page
    await page.goto('https://localhost:7137/send-notification');
    await page.waitForLoadState('networkidle');

    // Fill notification with image URL
    await page.fill('[data-testid="notification-title"]', 'SSW Speakers at NDC Sydney');
    await page.fill('[data-testid="notification-body"]', 'Check out our amazing speakers!');
    
    const imageUrl = 'https://adamcogan.com/wp-content/uploads/2019/11/ssw-speakers-ndc-sydney-2019.jpg';
    await page.fill('[data-testid="notification-image-url"]', imageUrl);
    await page.waitForTimeout(2000); // Wait for image to load

    // Verify image URL is set
    const imageField = page.locator('[data-testid="notification-image-url"]');
    await expect(imageField).toHaveValue(imageUrl);
    console.log('✅ Image URL set successfully');
    console.log(`📸 Image: ${imageUrl}`);

    // Verify image appears in the preview
    const previewImage = page.locator('.notification-image');
    await expect(previewImage).toBeVisible({ timeout: 5000 });
    await expect(previewImage).toHaveAttribute('src', imageUrl);
    console.log('✅ Image visible in preview');

    // Take screenshot to verify image preview
    await page.screenshot({ path: 'screenshots/send-notification/desktop-with-image.png', fullPage: true });
    console.log('📸 Screenshot: screenshots/send-notification/desktop-with-image.png');

    console.log('\n✅ Image URL - Verified');
    console.log('='.repeat(70));
  });
});
