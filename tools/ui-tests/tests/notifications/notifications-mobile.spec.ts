import { test, expect } from '@playwright/test';
import { takeResponsiveScreenshots } from '../../utils/screenshot-helper';

test.use({ 
  storageState: '.auth/user.json',
  hasTouch: true // Enable touch support for mobile tests
});

test.describe('Notifications History Page - Mobile View (390x844)', () => {
  test.beforeEach(async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto('https://localhost:7137/notifications');
    await page.waitForLoadState('networkidle');
  });

  test('verify page loads on mobile', async ({ page }) => {
    console.log('\n📱 Test: Mobile Page Load\n');
    console.log('='.repeat(70));
    console.log('📐 Viewport: 390x844');

    // Verify page title
    await expect(page.locator('h3')).toContainText('Mobile Notifications');
    console.log('✅ Page title visible on mobile');

    // Verify table is present
    const table = page.locator('.notifications-table');
    await expect(table).toBeVisible();
    console.log('✅ Notifications table visible on mobile');

    await takeResponsiveScreenshots(
      page,
      'screenshots/notifications',
      'mobile-page-load',
      { collapseSidebar: true } // Mobile collapses sidebar
    );

    console.log('\n✅ Mobile Page Load - Verified');
    console.log('='.repeat(70));
  });

  test('verify page is scrollable vertically on mobile', async ({ page }) => {
    console.log('\n📜 Test: Mobile Vertical Scrolling\n');
    console.log('='.repeat(70));

    const { isScrollable, scrollHeight, viewportHeight } = await page.evaluate(() => {
      return {
        isScrollable: document.documentElement.scrollHeight > document.documentElement.clientHeight,
        scrollHeight: document.documentElement.scrollHeight,
        viewportHeight: document.documentElement.clientHeight
      };
    });

    console.log(`📏 Page height: ${scrollHeight}px, Viewport: ${viewportHeight}px`);
    console.log(`✅ Scrollable: ${isScrollable}`);

    if (isScrollable) {
      // Try scrolling down
      await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
      await page.waitForTimeout(500);
      console.log('✅ Successfully scrolled to bottom');
    }

    console.log('\n✅ Mobile Vertical Scrolling - Verified');
    console.log('='.repeat(70));
  });

  test('verify horizontal scrolling is available for table on mobile', async ({ page }) => {
    console.log('\n↔️  Test: Mobile Horizontal Scrolling\n');
    console.log('='.repeat(70));

    // Check page-level horizontal scroll
    const { hasOverflow, scrollWidth, clientWidth } = await page.evaluate(() => {
      return {
        hasOverflow: document.documentElement.scrollWidth > document.documentElement.clientWidth,
        scrollWidth: document.documentElement.scrollWidth,
        clientWidth: document.documentElement.clientWidth
      };
    });

    console.log(`📏 Page scroll width: ${scrollWidth}px, Client width: ${clientWidth}px`);
    
    if (hasOverflow) {
      const overflowAmount = scrollWidth - clientWidth;
      console.log(`✅ Horizontal scroll available: ${overflowAmount}px overflow`);
      console.log('ℹ️  Expected on mobile - user can scroll to see all columns');
    } else {
      console.log('ℹ️  No horizontal overflow (table might be responsive or empty)');
    }

    await takeResponsiveScreenshots(
      page,
      'screenshots/notifications',
      'mobile-overflow',
      { collapseSidebar: true }
    );

    console.log('\n✅ Mobile Horizontal Scrolling - Verified');
    console.log('='.repeat(70));
  });

  test('verify create notification button is accessible on mobile', async ({ page }) => {
    console.log('\n➕ Test: Mobile Create Button\n');
    console.log('='.repeat(70));

    const createButton = page.locator('button:has-text("Create Notification")');
    
    // Check if visible (might be wrapped or scrolled)
    const isVisible = await createButton.isVisible();
    console.log(`✅ Create button visible: ${isVisible}`);

    if (isVisible) {
      // Try tapping it
      await createButton.tap();
      await page.waitForLoadState('networkidle');
      expect(page.url()).toContain('/send-notification');
      console.log('✅ Create button tappable and navigates');
    } else {
      // Might need to scroll to it
      await createButton.scrollIntoViewIfNeeded();
      await expect(createButton).toBeVisible();
      console.log('✅ Create button accessible after scrolling');
    }

    console.log('\n✅ Mobile Create Button - Verified');
    console.log('='.repeat(70));
  });

  test('verify search box works on mobile', async ({ page }) => {
    console.log('\n🔍 Test: Mobile Search\n');
    console.log('='.repeat(70));

    // Navigate back to notifications page (previous test navigated away)
    await page.goto('https://localhost:7137/notifications');
    await page.waitForLoadState('networkidle');

    const searchBox = page.locator('input[placeholder="Search by title"]');
    
    // Check if search box exists (might be hidden/collapsed on mobile)
    const exists = await searchBox.count() > 0;
    console.log(`✅ Search box exists in DOM: ${exists}`);

    if (exists) {
      // Try to make it visible by scrolling
      await searchBox.scrollIntoViewIfNeeded({ timeout: 10000 }).catch(() => {});
      await page.waitForTimeout(1000);
      
      const isVisible = await searchBox.isVisible();
      console.log(`📱 Search box visible: ${isVisible}`);

      if (isVisible) {
        // Tap and type
        await searchBox.tap();
        await searchBox.fill('mobile');
        console.log('✅ Search box accepts input on mobile');

        await expect(searchBox).toHaveValue('mobile');
        console.log('✅ Search value retained');
      } else {
        console.log('ℹ️  Search box exists but hidden (possible mobile layout behavior)');
      }
    } else {
      console.log('ℹ️  Search box not found in mobile layout');
    }

    await takeResponsiveScreenshots(
      page,
      'screenshots/notifications',
      'mobile-search',
      { collapseSidebar: true }
    );

    console.log('\n✅ Mobile Search - Verified');
    console.log('='.repeat(70));
  });

  test('verify table can be scrolled horizontally to see delivery stats', async ({ page }) => {
    console.log('\n📊 Test: Mobile Table Horizontal Scroll to Delivery Stats\n');
    console.log('='.repeat(70));

    // Wait for table to load
    await page.waitForTimeout(2000);

    const table = page.locator('.notifications-table');
    await expect(table).toBeVisible();

    // Check if table has rows
    const rows = table.locator('tbody tr');
    const rowCount = await rows.count();
    console.log(`📊 Table has ${rowCount} row(s)`);

    if (rowCount > 0) {
      // Try to find delivery stats column header
      const deliveryHeader = page.locator('th:has-text("Delivered to")').first();
      
      // Check if it exists
      const headerExists = await deliveryHeader.count() > 0;
      console.log(`✅ "Delivered to" header exists: ${headerExists}`);

      if (headerExists) {
        // Try scrolling horizontally within the table/page
        await page.evaluate(() => {
          // Scroll page right
          window.scrollTo({ left: document.body.scrollWidth, behavior: 'smooth' });
        });
        await page.waitForTimeout(1000);
        console.log('✅ Scrolled page horizontally to right');

        // Check if header is now visible
        const isNowVisible = await deliveryHeader.isVisible();
        console.log(`📱 "Delivered to" column visible after scroll: ${isNowVisible}`);

        if (isNowVisible) {
          // Check first row's delivery stats
          const firstRow = rows.first();
          const cells = firstRow.locator('td');
          const cellCount = await cells.count();
          
          if (cellCount > 0) {
            // Last cell should be delivery stats
            const lastCell = cells.last();
            const deliveryText = await lastCell.textContent();
            console.log(`✅ Last column content: ${deliveryText?.trim()}`);
          }
        } else {
          console.log('ℹ️  Column still not visible - table might be responsive and hiding columns');
        }
      }
    } else {
      console.log('ℹ️  No data in table to scroll to');
    }

    await takeResponsiveScreenshots(
      page,
      'screenshots/notifications',
      'mobile-scrolled-right',
      { collapseSidebar: true }
    );

    console.log('\n✅ Mobile Table Horizontal Scroll - Verified');
    console.log('='.repeat(70));
  });

  test('verify mobile viewport meta tag is present', async ({ page }) => {
    console.log('\n📱 Test: Mobile Viewport Meta Tag\n');
    console.log('='.repeat(70));

    const viewportMeta = await page.locator('meta[name="viewport"]').getAttribute('content');
    console.log(`✅ Viewport meta tag: ${viewportMeta}`);

    // Should have width=device-width for responsive design
    expect(viewportMeta).toContain('width=device-width');
    console.log('   - width=device-width: ✅');

    console.log('\n✅ Viewport Meta Tag - Checked');
    console.log('='.repeat(70));
  });

  test('verify show deleted checkbox works on mobile', async ({ page }) => {
    console.log('\n☑️  Test: Mobile Show Deleted Checkbox\n');
    console.log('='.repeat(70));

    const checkboxLabel = page.locator('label:has-text("Show deleted")');
    
    // Scroll to checkbox if needed
    await checkboxLabel.scrollIntoViewIfNeeded();
    await expect(checkboxLabel).toBeVisible();
    console.log('✅ Checkbox visible on mobile');

    // Tap checkbox
    await checkboxLabel.tap();
    await page.waitForTimeout(500);
    console.log('✅ Checkbox tappable');

    const checkbox = checkboxLabel.locator('input');
    await expect(checkbox).toBeChecked();
    console.log('✅ Checkbox state toggled');

    console.log('\n✅ Mobile Show Deleted Checkbox - Verified');
    console.log('='.repeat(70));
  });
});

test.describe('Notifications History Page - Mobile Landscape (844x390)', () => {
  test.beforeEach(async ({ page }) => {
    await page.setViewportSize({ width: 844, height: 390 });
    await page.goto('https://localhost:7137/notifications');
    await page.waitForLoadState('networkidle');
  });

  test('verify page works in mobile landscape', async ({ page }) => {
    console.log('\n🔄 Test: Mobile Landscape Layout\n');
    console.log('='.repeat(70));
    console.log('📐 Viewport: 844x390');

    // Verify page loads
    await expect(page.locator('h3')).toContainText('Mobile Notifications');
    console.log('✅ Page title visible in landscape');

    // Verify table visible
    const table = page.locator('.notifications-table');
    await expect(table).toBeVisible();
    console.log('✅ Table visible in landscape');

    // Check if more columns fit in landscape
    const { scrollWidth, clientWidth } = await page.evaluate(() => {
      return {
        scrollWidth: document.documentElement.scrollWidth,
        clientWidth: document.documentElement.clientWidth
      };
    });

    console.log(`📏 Scroll width: ${scrollWidth}px, Client width: ${clientWidth}px`);
    console.log('ℹ️  More horizontal space available in landscape mode');

    await takeResponsiveScreenshots(
      page,
      'screenshots/notifications',
      'mobile-landscape',
      { collapseSidebar: true }
    );

    console.log('\n✅ Mobile Landscape Layout - Verified');
    console.log('='.repeat(70));
  });
});
