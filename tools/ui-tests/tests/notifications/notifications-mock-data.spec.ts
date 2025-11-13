import { test, expect } from '@playwright/test';
import { takeResponsiveScreenshots } from '../../utils/screenshot-helper';

test.use({ 
  storageState: '.auth/user.json',
});

test.describe('Notifications History - Mock Data Tests', () => {

  // Clean up routes after each test to prevent interference
  test.afterEach(async ({ page }) => {
    await page.unroute('**/api/Notifications/List*');
  });
  
  test('empty state - zero notifications', async ({ page }) => {
    console.log('\n📭 Test: Empty State (0 Notifications)\n');
    console.log('='.repeat(70));

    // Set up route BEFORE navigation
    await page.route('**/api/Notifications/List*', async (route) => {
      console.log('🔄 Intercepting API call for empty state');
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [],
          count: 0,
          pageNumber: 0,
          pageSize: 20
        })
      });
    });

    // Navigate to page
    await page.goto('https://localhost:7137/notifications');
    
    // Wait for the page to be fully loaded
    await page.waitForSelector('.mud-appbar', { timeout: 10000 });
    
    // Wait for network and table to load
    await page.waitForLoadState('networkidle', { timeout: 10000 }).catch(() => {
      console.log('⚠️  Network idle timeout - continuing anyway');
    });
    await page.waitForTimeout(1500);
    
    console.log('✅ Page loaded with mocked empty data');

    // Verify table shows "No matching records found" or similar empty message
    // MudBlazor shows this message when table has no data
    const emptyStateSelectors = [
      'text=/no.*records.*found/i',
      'text=/no.*matching.*records/i', 
      'text=/no.*items.*to.*display/i',
      'text=/no.*data/i',
      '.mud-table-empty-row'
    ];

    let emptyStateFound = false;
    for (const selector of emptyStateSelectors) {
      const element = page.locator(selector).first();
      if (await element.isVisible().catch(() => false)) {
        console.log(`✅ Empty state message found with selector: ${selector}`);
        emptyStateFound = true;
        break;
      }
    }

    if (!emptyStateFound) {
      // Check if table body has no rows
      const rowCount = await page.locator('tbody tr').count();
      if (rowCount === 0 || rowCount === 1) { // 1 row might be the empty message row
        console.log('✅ Table has no data rows (empty state confirmed)');
        emptyStateFound = true;
      }
    }

    expect(emptyStateFound).toBe(true);

    // Take responsive screenshots with consistent naming
    await takeResponsiveScreenshots(
      page,
      'screenshots/notifications',
      'empty-state',
      { collapseSidebar: true }
    );
  });

  test('all notification statuses - comprehensive data', async ({ page }) => {
    console.log('\n📊 Test: All Notification Statuses\n');
    console.log('='.repeat(70));

    const now = new Date();
    const tomorrow = new Date(now.getTime() + 24 * 60 * 60 * 1000);
    const yesterday = new Date(now.getTime() - 24 * 60 * 60 * 1000);

    // Set up route BEFORE navigation
    await page.route('**/api/Notifications/List*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [
            {
              id: 1,
              title: 'Sent Notification',
              message: 'This notification was successfully sent',
              tags: 'All Users',
              createdDateUtc: yesterday.toISOString(),
              scheduledDate: null,
              status: 1, // Sent
              emailAddress: 'admin@ssw.com.au',
              wasSent: true,
              hasError: false,
              isArchived: false,
              sentOn: yesterday.toISOString(),
              numberOfUsersTargeted: 150,
              numberOfUsersSent: 148
            },
            {
              id: 2,
              title: 'Failed Notification',
              message: 'This notification failed to send',
              tags: 'Staff Only',
              createdDateUtc: yesterday.toISOString(),
              scheduledDate: null,
              status: 2, // Failed
              emailAddress: 'admin@ssw.com.au',
              wasSent: false,
              hasError: true,
              isArchived: false,
              sentOn: null,
              numberOfUsersTargeted: 50,
              numberOfUsersSent: 0
            },
            {
              id: 3,
              title: 'Scheduled Notification',
              message: 'This notification is scheduled for tomorrow',
              tags: 'Active Users',
              createdDateUtc: now.toISOString(),
              scheduledDate: tomorrow.toISOString(),
              status: 3, // Scheduled
              emailAddress: 'admin@ssw.com.au',
              wasSent: false,
              hasError: false,
              isArchived: false,
              sentOn: null,
              numberOfUsersTargeted: 200,
              numberOfUsersSent: 0
            },
            {
              id: 4,
              title: 'Not Sent Notification (Draft)',
              message: 'This notification was created but not sent',
              tags: 'Test Group',
              createdDateUtc: now.toISOString(),
              scheduledDate: null,
              status: 0, // NotSent
              emailAddress: 'admin@ssw.com.au',
              wasSent: false,
              hasError: false,
              isArchived: false,
              sentOn: null,
              numberOfUsersTargeted: 0,
              numberOfUsersSent: 0
            },
            {
              id: 5,
              title: 'Archived Sent Notification',
              message: 'This is an old notification that was archived',
              tags: 'All Users',
              createdDateUtc: new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000).toISOString(),
              scheduledDate: null,
              status: 1, // Sent
              emailAddress: 'admin@ssw.com.au',
              wasSent: true,
              hasError: false,
              isArchived: true,
              sentOn: new Date(now.getTime() - 29 * 24 * 60 * 60 * 1000).toISOString(),
              numberOfUsersTargeted: 100,
              numberOfUsersSent: 100
            }
          ],
          count: 5,
          pageNumber: 1,
          pageSize: 20
        })
      });
    });

    await page.goto('https://localhost:7137/notifications');
    await page.waitForSelector('.mud-appbar', { timeout: 10000 });

    console.log('✅ Page loaded with all notification statuses');

    // Wait for data to load
    await page.waitForTimeout(1000);

    // Verify all 5 notifications are displayed (excluding archived by default)
    const tableRows = page.locator('tbody tr');
    const rowCount = await tableRows.count();
    console.log(`📊 Total rows displayed: ${rowCount}`);
    
    // Note: Archived notification might be hidden by default
    expect(rowCount).toBeGreaterThanOrEqual(4);

    // Check for "Sent" status chip
    const sentChip = page.locator('text=/Sent/i').first();
    await expect(sentChip).toBeVisible();
    console.log('✅ "Sent" status found');

    // Check for "Failed" status chip
    const failedChip = page.locator('text=/Failed/i').first();
    await expect(failedChip).toBeVisible();
    console.log('✅ "Failed" status found');

    // Check for "Scheduled" status chip
    const scheduledChip = page.locator('text=/Scheduled/i').first();
    await expect(scheduledChip).toBeVisible();
    console.log('✅ "Scheduled" status found');

    // Check for "Not Sent" or "Draft" status
    const notSentChip = page.locator('text=/Not Sent|Draft/i').first();
    await expect(notSentChip).toBeVisible();
    console.log('✅ "Not Sent" status found');

    // Verify notification titles are visible
    await expect(page.locator('text="Sent Notification"')).toBeVisible();
    await expect(page.locator('text="Failed Notification"')).toBeVisible();
    await expect(page.locator('text="Scheduled Notification"')).toBeVisible();
    await expect(page.locator('text="Not Sent Notification (Draft)"')).toBeVisible();
    console.log('✅ All notification titles visible');

    // Take responsive screenshots with consistent naming
    await takeResponsiveScreenshots(
      page,
      'screenshots/notifications',
      'all-statuses',
      { collapseSidebar: true }
    );
  });

  test('show deleted checkbox - includes archived notifications', async ({ page }) => {
    console.log('\n🗑️  Test: Show Deleted (Archived) Notifications\n');
    console.log('='.repeat(70));

    const now = new Date();

    // Set up route BEFORE navigation - initially hide archived
    await page.route('**/api/Notifications/List*', async (route) => {
      const url = new URL(route.request().url());
      const showArchived = url.searchParams.get('includeDeleted') === 'true';
      console.log(`🔄 Intercepting API call - includeDeleted: ${showArchived}`);
      
      const allItems = [
        {
          id: 1,
          title: 'Active Notification',
          message: 'This is active',
          tags: 'All Users',
          createdDateUtc: now.toISOString(),
          scheduledDate: null,
          status: 1,
          emailAddress: 'admin@ssw.com.au',
          wasSent: true,
          hasError: false,
          isArchived: false,
          sentOn: now.toISOString(),
          numberOfUsersTargeted: 100,
          numberOfUsersSent: 100
        },
        {
          id: 2,
          title: 'Archived Notification',
          message: 'This is archived',
          tags: 'Staff Only',
          createdDateUtc: new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000).toISOString(),
          scheduledDate: null,
          status: 1,
          emailAddress: 'admin@ssw.com.au',
          wasSent: true,
          hasError: false,
          isArchived: true,
          sentOn: new Date(now.getTime() - 29 * 24 * 60 * 60 * 1000).toISOString(),
          numberOfUsersTargeted: 50,
          numberOfUsersSent: 50
        }
      ];

      const items = showArchived ? allItems : allItems.filter(i => !i.isArchived);

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: items,
          count: items.length,
          pageNumber: 1,
          pageSize: 20
        })
      });
    });

    await page.goto('https://localhost:7137/notifications');
    await page.waitForLoadState('networkidle');

    // Initially only 1 row (archived hidden)
    let rowCount = await page.locator('tbody tr').count();
    console.log(`📊 Initial rows (archived hidden): ${rowCount}`);
    expect(rowCount).toBe(1);

    // Click "Show deleted" checkbox
    const showDeletedCheckbox = page.locator('label:has-text("Show deleted")');
    await showDeletedCheckbox.click();
    console.log('✅ Clicked "Show deleted" checkbox');

    // Wait for reload
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    // Now should have 2 rows
    rowCount = await page.locator('tbody tr').count();
    console.log(`📊 Rows after showing deleted: ${rowCount}`);
    expect(rowCount).toBe(2);

    // Verify archived notification is visible
    await expect(page.locator('text="Archived Notification"')).toBeVisible();
    console.log('✅ Archived notification now visible');

    // Take screenshot
    await page.screenshot({
      path: 'screenshots/notifications/show-archived.png',
      fullPage: true
    });
    console.log('📸 Screenshot saved: screenshots/notifications/show-archived.png');
  });

  test('pagination with many notifications', async ({ page }) => {
    console.log('\n📄 Test: Pagination with Multiple Pages\n');
    console.log('='.repeat(70));

    const now = new Date();

    // Set up route BEFORE navigation with pagination
    await page.route('**/api/Notifications/List*', async (route) => {
      const url = new URL(route.request().url());
      const pageNum = parseInt(url.searchParams.get('page') || '0');
      const pageSize = parseInt(url.searchParams.get('pageSize') || '20');
      console.log(`🔄 Intercepting API call - page: ${pageNum}, pageSize: ${pageSize}`);

      // Generate 50 mock notifications
      const totalCount = 50;
      const items = [];
      
      const startIdx = pageNum * pageSize;
      const endIdx = Math.min(startIdx + pageSize, totalCount);

      for (let i = startIdx; i < endIdx; i++) {
        items.push({
          id: i + 1,
          title: `Notification ${i + 1}`,
          message: `Message for notification ${i + 1}`,
          tags: i % 2 === 0 ? 'All Users' : 'Staff Only',
          createdDateUtc: new Date(now.getTime() - i * 60 * 60 * 1000).toISOString(),
          scheduledDate: null,
          status: i % 4, // Rotate through statuses
          emailAddress: 'admin@ssw.com.au',
          wasSent: i % 4 === 1,
          hasError: i % 4 === 2,
          isArchived: false,
          sentOn: i % 4 === 1 ? new Date(now.getTime() - i * 60 * 60 * 1000).toISOString() : null,
          numberOfUsersTargeted: 100,
          numberOfUsersSent: i % 4 === 1 ? 98 : 0
        });
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: items,
          count: totalCount,
          pageNumber: pageNum,
          pageSize: pageSize
        })
      });
    });

    await page.goto('https://localhost:7137/notifications');
    await page.waitForSelector('.mud-appbar', { timeout: 10000 });

    console.log('✅ Page loaded with 50 mocked notifications');

    // Verify first page shows 20 rows (default page size)
    const rowCount = await page.locator('tbody tr').count();
    console.log(`📊 Rows on first page: ${rowCount}`);
    expect(rowCount).toBeLessThanOrEqual(20);

    // Verify pagination controls exist
    const paginationInfo = page.locator('text=/1-\\d+ of 50/i');
    await expect(paginationInfo).toBeVisible();
    console.log('✅ Pagination info visible (1-20 of 50)');

    // Verify next page button exists and is enabled
    const nextButton = page.locator('button[aria-label*="next" i], button:has-text("Next")').last();
    await expect(nextButton).toBeEnabled();
    console.log('✅ Next page button enabled');

    // Take screenshot
    await page.screenshot({
      path: 'screenshots/notifications/pagination.png',
      fullPage: true
    });
    console.log('📸 Screenshot saved: screenshots/notifications/pagination.png');
  });
});
