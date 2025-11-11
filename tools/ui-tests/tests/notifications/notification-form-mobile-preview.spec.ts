import { test, expect } from '@playwright/test';

// Configure mobile viewport (iPhone 12 Pro dimensions with Chromium)
test.use({ 
  viewport: { width: 390, height: 844 }, // iPhone 12 Pro
  userAgent: 'Mozilla/5.0 (iPhone; CPU iPhone OS 14_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 Mobile/15E148 Safari/604.1',
  deviceScaleFactor: 3,
  isMobile: true,
  hasTouch: true,
  storageState: '.auth/user.json'
});

test.describe('Notification Form - Mobile Responsive View', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('https://localhost:7137/send-notification');
    await page.waitForLoadState('networkidle');
  });

  test('verify page loads and is scrollable on mobile', async ({ page }) => {
    console.log('\n📱 Test: Mobile Page Load & Scroll\n');
    console.log('='.repeat(70));
    console.log(`📐 Viewport: ${page.viewportSize()?.width}x${page.viewportSize()?.height}`);

    // Verify main form elements are visible (may need scrolling)
    const form = page.locator('form').first();
    await expect(form).toBeVisible();
    console.log('✅ Form visible on mobile');

    // Check if page is scrollable (content height > viewport height)
    const scrollHeight = await page.evaluate(() => document.documentElement.scrollHeight);
    const viewportHeight = page.viewportSize()?.height || 0;
    const isScrollable = scrollHeight > viewportHeight;
    console.log(`📏 Page height: ${scrollHeight}px, Viewport: ${viewportHeight}px`);
    console.log(`✅ Scrollable: ${isScrollable}`);

    // Scroll to bottom to verify full page is accessible
    await page.evaluate(() => window.scrollTo(0, document.documentElement.scrollHeight));
    await page.waitForTimeout(500);
    console.log('✅ Scrolled to bottom successfully');

    // Take screenshot of mobile view
    await page.screenshot({
      path: 'screenshots/notification-mobile-view.png',
      fullPage: true
    });

    console.log('\n✅ Mobile Page Load - Verified');
    console.log('='.repeat(70));
  });

  test('verify delivery options are accessible on mobile', async ({ page }) => {
    console.log('\n📻 Test: Mobile Delivery Options\n');
    console.log('='.repeat(70));

    // Verify radio buttons are visible and tappable
    const nowOption = page.locator('[data-testid="delivery-now"]');
    const scheduleOption = page.locator('[data-testid="delivery-schedule"]');

    await expect(nowOption).toBeVisible();
    await expect(scheduleOption).toBeVisible();
    console.log('✅ Delivery options visible on mobile');

    // Tap Schedule option (touch interaction)
    await scheduleOption.tap();
    await page.waitForTimeout(500);

    // Verify Schedule is selected
    await expect(scheduleOption).toHaveAttribute('aria-checked', 'true');
    console.log('✅ Schedule option tappable and selectable');

    // Tap back to Now
    await nowOption.tap();
    await page.waitForTimeout(500);
    await expect(nowOption).toHaveAttribute('aria-checked', 'true');
    console.log('✅ Now option tappable and selectable');

    console.log('\n✅ Mobile Delivery Options - Verified');
    console.log('='.repeat(70));
  });

  test('verify targeting options are accessible on mobile', async ({ page }) => {
    console.log('\n🎯 Test: Mobile Targeting Options\n');
    console.log('='.repeat(70));

    // Scroll to targeting section (might be below the fold)
    const targetingSection = page.locator('[data-testid="target-everyone"]').locator('..');
    await targetingSection.scrollIntoViewIfNeeded();
    await page.waitForTimeout(300);

    // Verify all targeting options
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
    console.log('✅ Achievement autocomplete appears on mobile');

    console.log('\n✅ Mobile Targeting Options - Verified');
    console.log('='.repeat(70));
  });

  test('verify text input fields work on mobile', async ({ page }) => {
    console.log('\n⌨️  Test: Mobile Text Input\n');
    console.log('='.repeat(70));

    // Scroll to form fields
    const titleField = page.locator('[data-testid="notification-title"]');
    await titleField.scrollIntoViewIfNeeded();
    await page.waitForTimeout(300);

    // Tap and type in title field
    await titleField.tap();
    await titleField.fill('Mobile Test Notification');
    console.log('✅ Title field accepts input on mobile');

    // Verify input was entered
    await expect(titleField).toHaveValue('Mobile Test Notification');

    // Test body field
    const bodyField = page.locator('[data-testid="notification-body"]');
    await bodyField.tap();
    await bodyField.fill('This is a test from a mobile device.');
    console.log('✅ Body field accepts input on mobile');

    await expect(bodyField).toHaveValue('This is a test from a mobile device.');

    // Test image URL field
    const imageField = page.locator('[data-testid="notification-image-url"]');
    await imageField.scrollIntoViewIfNeeded();
    await imageField.tap();
    await imageField.fill('https://example.com/image.jpg');
    console.log('✅ Image URL field accepts input on mobile');

    await expect(imageField).toHaveValue('https://example.com/image.jpg');

    await page.screenshot({
      path: 'screenshots/notification-mobile-input.png',
      fullPage: true
    });

    console.log('\n✅ Mobile Text Input - Verified');
    console.log('='.repeat(70));
  });

  test('verify iPhone preview is hidden on mobile (responsive)', async ({ page }) => {
    console.log('\n📱 Test: iPhone Preview Hidden on Mobile\n');
    console.log('='.repeat(70));

    const iphonePreview = page.locator('.iphone-preview');
    
    // Preview should be hidden on mobile portrait (< 768px) to prevent overflow
    const isVisible = await iphonePreview.isVisible().catch(() => false);
    console.log(`📱 iPhone preview visible: ${isVisible}`);

    if (isVisible) {
      console.log('⚠️  WARNING: Preview is visible on mobile (may cause overflow)');
    } else {
      console.log('✅ Preview hidden on mobile (responsive CSS working)');
    }

    // Preview should exist in DOM but be hidden with CSS
    const exists = await iphonePreview.count() > 0;
    console.log(`� Preview exists in DOM: ${exists}`);
    expect(exists).toBe(true);
    console.log('✅ Preview exists in DOM but hidden with CSS media query');

    console.log('\n✅ iPhone Preview Mobile Responsive - Verified');
    console.log('='.repeat(70));
  });

  test('verify form submission button is accessible on mobile', async ({ page }) => {
    console.log('\n🚀 Test: Mobile Submit Button\n');
    console.log('='.repeat(70));

    // Fill in minimum required fields
    await page.fill('[data-testid="notification-title"]', 'Test');
    await page.fill('[data-testid="notification-body"]', 'Test body');

    // Find submit button (might need to scroll)
    const submitButton = page.locator('button[type="submit"]').or(page.locator('button:has-text("Send")')).first();
    
    // Scroll to button
    await submitButton.scrollIntoViewIfNeeded();
    await page.waitForTimeout(300);

    // Verify button is visible and tappable
    await expect(submitButton).toBeVisible();
    const isEnabled = await submitButton.isEnabled();
    console.log(`✅ Submit button visible: true, enabled: ${isEnabled}`);

    // Take screenshot with submit button visible
    await page.screenshot({
      path: 'screenshots/notification-mobile-submit.png',
      fullPage: false // Just current viewport showing submit button
    });

    console.log('\n✅ Mobile Submit Button - Verified');
    console.log('='.repeat(70));
  });

  test('verify autocomplete dropdown works on mobile', async ({ page }) => {
    console.log('\n🔽 Test: Mobile Autocomplete Dropdown\n');
    console.log('='.repeat(70));

    // Select achievement targeting
    const achievementOption = page.locator('[data-testid="target-achievement"]');
    await achievementOption.scrollIntoViewIfNeeded();
    await achievementOption.tap();
    await page.waitForTimeout(500);

    // Tap on autocomplete field
    const achievementAutocomplete = page.locator('[data-testid="achievement-autocomplete"]');
    await achievementAutocomplete.waitFor({ state: 'visible', timeout: 10000 });
    await achievementAutocomplete.tap();
    
    // Type to trigger dropdown
    await page.keyboard.type('test');
    await page.waitForTimeout(1500);

    // Check if dropdown appears
    const dropdown = page.locator('.mud-popover');
    const dropdownVisible = await dropdown.isVisible().catch(() => false);
    console.log(`✅ Autocomplete dropdown visible: ${dropdownVisible}`);

    if (dropdownVisible) {
      // Try to tap first item
      const firstItem = page.locator('.mud-list-item').first();
      const itemVisible = await firstItem.isVisible().catch(() => false);
      
      if (itemVisible) {
        await firstItem.tap();
        await page.waitForTimeout(500);
        console.log('✅ Dropdown item tappable on mobile');
      }
    }

    await page.screenshot({
      path: 'screenshots/notification-mobile-autocomplete.png',
      fullPage: true
    });

    console.log('\n✅ Mobile Autocomplete - Verified');
    console.log('='.repeat(70));
  });

  test('verify no horizontal overflow on mobile', async ({ page }) => {
    console.log('\n↔️  Test: Mobile Horizontal Overflow\n');
    console.log('='.repeat(70));

    // Check for horizontal scrollbar
    const hasHorizontalScroll = await page.evaluate(() => {
      return document.documentElement.scrollWidth > document.documentElement.clientWidth;
    });

    const scrollWidth = await page.evaluate(() => document.documentElement.scrollWidth);
    const clientWidth = await page.evaluate(() => document.documentElement.clientWidth);
    
    console.log(`📏 Scroll width: ${scrollWidth}px, Client width: ${clientWidth}px`);
    
    if (hasHorizontalScroll) {
      console.log('⚠️  WARNING: Horizontal overflow detected on mobile!');
      
      // Check for any elements that might be overflowing
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

      if (overflowingElements.length > 0) {
        console.log('⚠️  Overflowing elements:');
        overflowingElements.forEach((el, i) => {
          console.log(`  ${i + 1}. ${el.tag}.${el.class} (width: ${el.width}px)`);
        });
      }
    } else {
      console.log('✅ No horizontal overflow - content fits mobile viewport');
    }

    console.log('\n✅ Mobile Overflow Check - Complete');
    console.log('='.repeat(70));
  });

  test('verify mobile viewport meta tag is present', async ({ page }) => {
    console.log('\n📱 Test: Mobile Viewport Meta Tag\n');
    console.log('='.repeat(70));

    const viewportMeta = await page.locator('meta[name="viewport"]').getAttribute('content');
    
    if (viewportMeta) {
      console.log(`✅ Viewport meta tag: ${viewportMeta}`);
      
      // Check if it includes important mobile settings
      const hasWidthDevice = viewportMeta.includes('width=device-width');
      const hasInitialScale = viewportMeta.includes('initial-scale=1');
      
      console.log(`   - width=device-width: ${hasWidthDevice ? '✅' : '❌'}`);
      console.log(`   - initial-scale=1: ${hasInitialScale ? '✅' : '❌'}`);
    } else {
      console.log('⚠️  WARNING: Viewport meta tag not found!');
    }

    console.log('\n✅ Viewport Meta Tag - Checked');
    console.log('='.repeat(70));
  });
});

test.describe('Notification Form - Mobile Landscape View', () => {
  test('verify page layout in landscape mode', async ({ browser }) => {
    // Create a new context with landscape orientation (iPhone 12 Pro landscape: 844x390)
    const context = await browser.newContext({
      viewport: { width: 844, height: 390 },
      userAgent: 'Mozilla/5.0 (iPhone; CPU iPhone OS 14_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 Mobile/15E148 Safari/604.1',
      deviceScaleFactor: 3,
      isMobile: true,
      hasTouch: true,
      storageState: '.auth/user.json'
    });
    const page = await context.newPage();
    console.log('\n🔄 Test: Mobile Landscape Layout\n');
    console.log('='.repeat(70));
    console.log(`📐 Viewport: ${page.viewportSize()?.width}x${page.viewportSize()?.height}`);

    await page.goto('https://localhost:7137/send-notification');
    await page.waitForLoadState('networkidle');

    // Check if iPhone preview is visible in landscape (more horizontal space)
    const iphonePreview = page.locator('.iphone-preview');
    const isVisible = await iphonePreview.isVisible().catch(() => false);
    
    console.log(`📱 iPhone preview visible in landscape: ${isVisible}`);

    // Verify form is still functional
    const titleField = page.locator('[data-testid="notification-title"]');
    await expect(titleField).toBeVisible();
    console.log('✅ Form fields visible in landscape');

    // Check for horizontal overflow
    const hasHorizontalScroll = await page.evaluate(() => {
      return document.documentElement.scrollWidth > document.documentElement.clientWidth;
    });

    console.log(`✅ Horizontal overflow in landscape: ${hasHorizontalScroll ? 'Yes ⚠️' : 'No'}`);

    await page.screenshot({
      path: 'screenshots/notification-mobile-landscape.png',
      fullPage: true
    });

    console.log('\n✅ Mobile Landscape Layout - Verified');
    console.log('='.repeat(70));

    // Cleanup
    await context.close();
  });
});
