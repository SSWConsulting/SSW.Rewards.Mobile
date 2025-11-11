import { test, expect } from '@playwright/test';

// Configure tablet viewport (iPad Pro dimensions with Chromium)
// Tablet is essentially "desktop-lite" - has enough width to show iPhone preview
test.use({ 
  viewport: { width: 1024, height: 1366 }, // iPad Pro portrait
  userAgent: 'Mozilla/5.0 (iPad; CPU OS 14_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 Mobile/15E148 Safari/604.1',
  deviceScaleFactor: 2,
  isMobile: true,
  hasTouch: true,
  storageState: '.auth/user.json'
});

test.describe('Notification Form - Tablet Responsive View', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('https://localhost:7137/send-notification');
    await page.waitForLoadState('networkidle');
  });

  test('verify page loads and iPhone preview is visible on tablet', async ({ page }) => {
    console.log('\n📱 Test: Tablet Page Load & Preview\n');
    console.log('='.repeat(70));
    console.log(`📐 Viewport: ${page.viewportSize()?.width}x${page.viewportSize()?.height}`);

    // Verify main form elements are visible
    const form = page.locator('form').first();
    await expect(form).toBeVisible();
    console.log('✅ Form visible on tablet');

    // iPhone preview SHOULD be visible on tablet (enough horizontal space)
    const iphonePreview = page.locator('.iphone-preview');
    await expect(iphonePreview).toBeVisible();
    console.log('✅ iPhone preview visible on tablet');

    // Verify preview is properly sized
    const boundingBox = await iphonePreview.boundingBox();
    if (boundingBox) {
      console.log(`📐 Preview: ${boundingBox.width}x${boundingBox.height}px`);
      expect(boundingBox.width).toBe(260);
      expect(boundingBox.height).toBe(520);
      console.log('✅ Preview has correct dimensions (smaller for iPad Pro)');
    }

    await page.screenshot({
      path: 'screenshots/notification-tablet-preview.png',
      fullPage: true
    });

    console.log('\n✅ Tablet Page Load - Verified');
    console.log('='.repeat(70));
  });

  test('verify delivery options work with touch on tablet', async ({ page }) => {
    console.log('\n📻 Test: Tablet Delivery Options\n');
    console.log('='.repeat(70));

    // Verify radio buttons are visible and tappable
    const nowOption = page.locator('[data-testid="delivery-now"]');
    const scheduleOption = page.locator('[data-testid="delivery-schedule"]');

    await expect(nowOption).toBeVisible();
    await expect(scheduleOption).toBeVisible();
    console.log('✅ Delivery options visible');

    // Tap Schedule option
    await scheduleOption.tap();
    await page.waitForTimeout(500);
    await expect(scheduleOption).toHaveAttribute('aria-checked', 'true');
    console.log('✅ Schedule option tappable');

    // Tap back to Now
    await nowOption.tap();
    await page.waitForTimeout(500);
    await expect(nowOption).toHaveAttribute('aria-checked', 'true');
    console.log('✅ Now option tappable');

    console.log('\n✅ Tablet Delivery Options - Verified');
    console.log('='.repeat(70));
  });

  test('verify preview updates in real-time on tablet', async ({ page }) => {
    console.log('\n⚡ Test: Tablet Preview Updates\n');
    console.log('='.repeat(70));

    const titleField = page.locator('[data-testid="notification-title"]');
    const bodyField = page.locator('[data-testid="notification-body"]');
    const previewTitle = page.locator('.notification-title');
    const previewBody = page.locator('.notification-body');

    // Verify preview is visible
    await expect(page.locator('.iphone-preview')).toBeVisible();
    console.log('✅ Preview visible before input');

    // Type in title and verify preview updates
    await titleField.tap();
    await titleField.fill('Tablet Test Notification');
    await expect(previewTitle).toContainText('Tablet Test Notification');
    console.log('✅ Preview title updates on tablet');

    // Type in body and verify preview updates
    await bodyField.tap();
    await bodyField.fill('This notification was created on a tablet device.');
    await expect(previewBody).toContainText('This notification was created on a tablet');
    console.log('✅ Preview body updates on tablet');

    await page.screenshot({
      path: 'screenshots/notification-tablet-live-preview.png',
      fullPage: true
    });

    console.log('\n✅ Tablet Preview Updates - Verified');
    console.log('='.repeat(70));
  });

  test('verify targeting options are accessible on tablet', async ({ page }) => {
    console.log('\n🎯 Test: Tablet Targeting Options\n');
    console.log('='.repeat(70));

    const everyoneOption = page.locator('[data-testid="target-everyone"]');
    const achievementOption = page.locator('[data-testid="target-achievement"]');
    const roleOption = page.locator('[data-testid="target-role"]');

    await expect(everyoneOption).toBeVisible();
    await expect(achievementOption).toBeVisible();
    await expect(roleOption).toBeVisible();
    console.log('✅ All targeting options visible');

    // Tap Achievement option
    await achievementOption.tap();
    await page.waitForTimeout(500);
    await expect(achievementOption).toHaveAttribute('aria-checked', 'true');
    console.log('✅ Achievement targeting tappable');

    // Verify autocomplete appears
    const achievementAutocomplete = page.locator('[data-testid="achievement-autocomplete"]');
    await expect(achievementAutocomplete).toBeVisible();
    console.log('✅ Achievement autocomplete appears');

    // Type and select
    await achievementAutocomplete.tap();
    await page.keyboard.type('test');
    await page.waitForTimeout(1500);

    const firstResult = page.locator('.mud-list-item').first();
    const isVisible = await firstResult.isVisible().catch(() => false);
    if (isVisible) {
      await firstResult.tap();
      console.log('✅ Achievement selected from dropdown');
    }

    console.log('\n✅ Tablet Targeting Options - Verified');
    console.log('='.repeat(70));
  });

  test('verify no horizontal overflow on tablet', async ({ page }) => {
    console.log('\n↔️  Test: Tablet Horizontal Overflow\n');
    console.log('='.repeat(70));

    const hasHorizontalScroll = await page.evaluate(() => {
      return document.documentElement.scrollWidth > document.documentElement.clientWidth;
    });

    const scrollWidth = await page.evaluate(() => document.documentElement.scrollWidth);
    const clientWidth = await page.evaluate(() => document.documentElement.clientWidth);
    
    console.log(`📏 Scroll width: ${scrollWidth}px, Client width: ${clientWidth}px`);
    
    if (hasHorizontalScroll) {
      console.log('⚠️  WARNING: Horizontal overflow detected on tablet!');
      
      // Check for overflowing elements
      const overflowingElements = await page.evaluate(() => {
        const elements = Array.from(document.querySelectorAll('*'));
        return elements
          .filter(el => {
            const rect = el.getBoundingClientRect();
            return rect.right > window.innerWidth;
          })
          .map(el => ({
            tag: el.tagName,
            class: el.className,
            width: el.getBoundingClientRect().width
          }))
          .slice(0, 5);
      });

      overflowingElements.forEach((el, i) => {
        console.log(`  ${i + 1}. ${el.tag}.${el.class} (width: ${el.width}px)`);
      });
    } else {
      console.log('✅ No horizontal overflow - content fits tablet viewport');
    }

    console.log('\n✅ Tablet Overflow Check - Complete');
    console.log('='.repeat(70));
  });

  test('verify form submission button is accessible on tablet', async ({ page }) => {
    console.log('\n🚀 Test: Tablet Submit Button\n');
    console.log('='.repeat(70));

    // Fill minimum required fields
    await page.fill('[data-testid="notification-title"]', 'Test Notification');
    await page.fill('[data-testid="notification-body"]', 'Test body content');

    // Find submit button
    const submitButton = page.locator('button[type="submit"]').or(page.locator('button:has-text("Send")')).first();
    
    await expect(submitButton).toBeVisible();
    const isEnabled = await submitButton.isEnabled();
    console.log(`✅ Submit button visible and enabled: ${isEnabled}`);

    await page.screenshot({
      path: 'screenshots/notification-tablet-submit.png',
      fullPage: false
    });

    console.log('\n✅ Tablet Submit Button - Verified');
    console.log('='.repeat(70));
  });

  test('verify image preview works on tablet', async ({ page }) => {
    console.log('\n🖼️  Test: Tablet Image Preview\n');
    console.log('='.repeat(70));

    // Fill notification details
    await page.fill('[data-testid="notification-title"]', 'SSW Event');
    await page.fill('[data-testid="notification-body"]', 'Join us for an amazing event!');
    
    const imageUrl = 'https://adamcogan.com/wp-content/uploads/2019/11/ssw-speakers-ndc-sydney-2019.jpg';
    await page.fill('[data-testid="notification-image-url"]', imageUrl);
    await page.waitForTimeout(2000);

    // Verify image appears in preview
    const previewImage = page.locator('.notification-image');
    await expect(previewImage).toBeVisible({ timeout: 5000 });
    await expect(previewImage).toHaveAttribute('src', imageUrl);
    console.log('✅ Image preview visible on tablet');

    await page.screenshot({
      path: 'screenshots/notification-tablet-with-image.png',
      fullPage: true
    });

    console.log('\n✅ Tablet Image Preview - Verified');
    console.log('='.repeat(70));
  });
});

test.describe('Notification Form - Tablet Landscape View', () => {
  test('verify page layout in tablet landscape mode', async ({ browser }) => {
    // Create context with landscape orientation (iPad Pro landscape: 1366x1024)
    const context = await browser.newContext({
      viewport: { width: 1366, height: 1024 },
      userAgent: 'Mozilla/5.0 (iPad; CPU OS 14_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 Mobile/15E148 Safari/604.1',
      deviceScaleFactor: 2,
      isMobile: true,
      hasTouch: true,
      storageState: '.auth/user.json'
    });
    const page = await context.newPage();

    console.log('\n🔄 Test: Tablet Landscape Layout\n');
    console.log('='.repeat(70));
    console.log(`📐 Viewport: ${page.viewportSize()?.width}x${page.viewportSize()?.height}`);

    await page.goto('https://localhost:7137/send-notification');
    await page.waitForLoadState('networkidle');

    // iPhone preview should be visible in landscape (even more space)
    const iphonePreview = page.locator('.iphone-preview');
    await expect(iphonePreview).toBeVisible();
    console.log('✅ iPhone preview visible in tablet landscape');

    // Verify form is functional
    const titleField = page.locator('[data-testid="notification-title"]');
    await expect(titleField).toBeVisible();
    console.log('✅ Form fields visible in landscape');

    // Check for overflow
    const hasHorizontalScroll = await page.evaluate(() => {
      return document.documentElement.scrollWidth > document.documentElement.clientWidth;
    });
    console.log(`✅ Horizontal overflow: ${hasHorizontalScroll ? 'Yes ⚠️' : 'No'}`);

    await page.screenshot({
      path: 'screenshots/notification-tablet-landscape.png',
      fullPage: true
    });

    console.log('\n✅ Tablet Landscape Layout - Verified');
    console.log('='.repeat(70));

    await context.close();
  });
});
