import { test, expect } from '@playwright/test';

test.use({ storageState: '.auth/user.json' });

/**
 * Custom viewports for kiosk leaderboard testing
 * - Portrait TV: 1080x1920 (vertical display)
 * - Landscape FHD Laptop: 1920x1080 (standard Full HD)
 * - Landscape iPad Pro: 1366x1024 (12.9" iPad Pro in landscape)
 * - Portrait iPhone 16 Pro: 393x852 (iPhone 16 Pro dimensions)
 */
const KIOSK_VIEWPORTS = {
  portraitTV: { name: 'Portrait TV', width: 1080, height: 1920 },
  landscapeFHD: { name: 'Landscape FHD Laptop', width: 1920, height: 1080 },
  landscapeIPadPro: { name: 'Landscape iPad Pro', width: 1366, height: 1024 },
  portraitIPhone16Pro: { name: 'Portrait iPhone 16 Pro', width: 393, height: 852 }
};

/**
 * Helper function to take screenshots across all kiosk viewports
 */
async function takeKioskScreenshots(
  page: any,
  testName: string,
  options: {
    fullPage?: boolean;
    waitForNetwork?: boolean;
  } = {}
) {
  const { fullPage = true, waitForNetwork = true } = options;

  for (const [key, viewport] of Object.entries(KIOSK_VIEWPORTS)) {
    console.log(`\n📐 Setting viewport: ${viewport.name} (${viewport.width}x${viewport.height})`);
    
    // Set viewport
    await page.setViewportSize({ width: viewport.width, height: viewport.height });
    
    // Wait for network idle if requested
    if (waitForNetwork) {
      await page.waitForLoadState('networkidle', { timeout: 10000 }).catch(() => {
        console.log(`⚠️  Network idle timeout for ${viewport.name} - continuing anyway`);
      });
    }

    // Additional wait for page to settle and animations to complete
    await page.waitForTimeout(1000);

    const filename = `${key}-${viewport.width}x${viewport.height}-${testName}.png`;
    const fullPath = `screenshots/kiosk-leaderboard/${filename}`;
    
    await page.screenshot({
      path: fullPath,
      fullPage
    });
    
    console.log(`📸 Screenshot saved: ${fullPath}`);
  }
}

test.describe('Kiosk Leaderboard - Visual Tests', () => {

  test('kiosk leaderboard - default view with data', async ({ page }) => {
    console.log('\n🏆 Test: Kiosk Leaderboard Default View\n');
    console.log('='.repeat(70));

    await page.goto('https://localhost:7137/kiosk-leaderboard');
    
    // Wait for the leaderboard table to load
    await page.waitForSelector('.kiosk-leaderboard-table', { timeout: 15000 });
    console.log('✅ Kiosk leaderboard table loaded');
    
    // Wait for data to populate
    await page.waitForSelector('.mud-table-row', { timeout: 10000 });
    
    // Count rows
    const rows = await page.locator('.mud-table-row').count();
    console.log(`📊 Leaderboard showing ${rows} user rows`);
    
    // Verify logo is present
    const logo = page.locator('img[src="/images/ssw-rewards-logo.svg"]');
    const logoVisible = await logo.isVisible().catch(() => false);
    console.log(`✅ SSW Rewards logo visible: ${logoVisible ? '✅' : '❌'}`);
    
    // Verify tabs are present
    const tabs = page.locator('.kiosk-tabs .mud-tab');
    const tabCount = await tabs.count();
    console.log(`📑 Found ${tabCount} tab(s)`);
    
    // Verify table columns
    const headers = await page.locator('.mud-table-head .mud-table-cell').allTextContents();
    console.log('📋 Table columns:', headers);
    
    // Take screenshots across all viewports
    await takeKioskScreenshots(page, 'default-view');
    
    console.log('\n✅ Kiosk leaderboard default view test complete');
    console.log('='.repeat(70));
  });

  test('kiosk leaderboard - scrolled view', async ({ page }) => {
    console.log('\n🏆 Test: Kiosk Leaderboard Scrolled View\n');
    console.log('='.repeat(70));

    await page.goto('https://localhost:7137/kiosk-leaderboard');
    await page.waitForSelector('.kiosk-leaderboard-table', { timeout: 15000 });
    await page.waitForSelector('.mud-table-row', { timeout: 10000 });
    
    console.log('✅ Kiosk leaderboard loaded');
    
    // Scroll down to see more entries (if pagination exists)
    const tableContainer = page.locator('.mud-table-container').first();
    
    // Check if scrollable
    const isScrollable = await tableContainer.evaluate((el) => {
      return el.scrollHeight > el.clientHeight;
    });
    
    if (isScrollable) {
      await tableContainer.evaluate((el) => {
        el.scrollTop = el.scrollHeight / 2; // Scroll halfway
      });
      console.log('📜 Scrolled table to middle position');
      await page.waitForTimeout(500);
    } else {
      console.log('ℹ️  Table not scrollable (all data fits on screen)');
    }
    
    // Take screenshots
    await takeKioskScreenshots(page, 'scrolled-view');
    
    console.log('\n✅ Kiosk leaderboard scrolled view test complete');
    console.log('='.repeat(70));
  });

  test('kiosk leaderboard - layout responsiveness', async ({ page }) => {
    console.log('\n🏆 Test: Kiosk Leaderboard Layout Responsiveness\n');
    console.log('='.repeat(70));

    await page.goto('https://localhost:7137/kiosk-leaderboard');
    await page.waitForSelector('.kiosk-leaderboard-table', { timeout: 15000 });
    await page.waitForSelector('.mud-table-row', { timeout: 10000 });
    
    console.log('✅ Kiosk leaderboard loaded');
    
    // Check layout elements at each viewport
    for (const [key, viewport] of Object.entries(KIOSK_VIEWPORTS)) {
      console.log(`\n📐 Testing ${viewport.name} (${viewport.width}x${viewport.height})`);
      
      await page.setViewportSize({ width: viewport.width, height: viewport.height });
      await page.waitForTimeout(500);
      
      // Verify logo is visible
      const logoVisible = await page.locator('img[src="/images/ssw-rewards-logo.svg"]').isVisible();
      console.log(`   �️  Logo visible: ${logoVisible ? '✅' : '❌'}`);
      
      // Verify table is visible
      const tableVisible = await page.locator('.kiosk-leaderboard-table').isVisible();
      console.log(`   📊 Table visible: ${tableVisible ? '✅' : '❌'}`);
      
      // Check if header is present
      const headerVisible = await page.locator('.mud-table-head').isVisible();
      console.log(`   📋 Header visible: ${headerVisible ? '✅' : '❌'}`);
      
      // Count visible rows
      const visibleRows = await page.locator('.mud-table-row:visible').count();
      console.log(`   👥 Visible rows: ${visibleRows}`);
      
      // Get table dimensions
      const tableBounds = await page.locator('.kiosk-leaderboard-table').boundingBox();
      if (tableBounds) {
        console.log(`   📏 Table size: ${Math.round(tableBounds.width)}x${Math.round(tableBounds.height)}px`);
      }
    }
    
    // Take screenshots showing responsiveness
    await takeKioskScreenshots(page, 'responsiveness');
    
    console.log('\n✅ Kiosk leaderboard responsiveness test complete');
    console.log('='.repeat(70));
  });

  test('kiosk leaderboard - dark theme styling', async ({ page }) => {
    console.log('\n🏆 Test: Kiosk Leaderboard Dark Theme\n');
    console.log('='.repeat(70));

    await page.goto('https://localhost:7137/kiosk-leaderboard');
    await page.waitForSelector('.kiosk-leaderboard-table', { timeout: 15000 });
    await page.waitForSelector('.mud-table-row', { timeout: 10000 });
    
    console.log('✅ Kiosk leaderboard loaded');
    
    // Verify dark theme styling
    const kioskPaper = page.locator('.kiosk-leaderboard');
    
    // Get computed styles
    const bgColor = await kioskPaper.evaluate((el) => {
      return window.getComputedStyle(el).backgroundColor;
    });
    
    const textColor = await kioskPaper.evaluate((el) => {
      return window.getComputedStyle(el).color;
    });
    
    console.log('🎨 Theme colors:');
    console.log(`   Background: ${bgColor}`);
    console.log(`   Text: ${textColor}`);
    
    // Verify table exists and get basic info
    const tableExists = await page.locator('.kiosk-leaderboard-table').isVisible();
    console.log(`📊 Table visible: ${tableExists}`);
    
    if (tableExists) {
      const tableBg = await page.locator('.kiosk-leaderboard-table').evaluate((el) => {
        const styles = window.getComputedStyle(el);
        return styles.backgroundColor;
      }).catch(() => 'Unable to retrieve');
      
      console.log(`   Table background: ${tableBg}`);
    }
    
    // Take screenshots
    await takeKioskScreenshots(page, 'dark-theme');
    
    console.log('\n✅ Kiosk leaderboard dark theme test complete');
    console.log('='.repeat(70));
  });

  test('kiosk leaderboard - full page comparison', async ({ page }) => {
    console.log('\n🏆 Test: Kiosk Leaderboard Full Page Comparison\n');
    console.log('='.repeat(70));

    await page.goto('https://localhost:7137/kiosk-leaderboard');
    await page.waitForSelector('.kiosk-leaderboard-table', { timeout: 15000 });
    await page.waitForSelector('.mud-table-row', { timeout: 10000 });
    
    console.log('✅ Kiosk leaderboard loaded');
    
    // Wait for any animations
    await page.waitForTimeout(1500);
    
    // Get overall page metrics
    console.log('\n📊 Page Metrics:');
    
    const metrics = await page.evaluate(() => {
      return {
        totalRows: document.querySelectorAll('.mud-table-row').length,
        hasScrollbar: document.documentElement.scrollHeight > window.innerHeight,
        viewportHeight: window.innerHeight,
        documentHeight: document.documentElement.scrollHeight
      };
    });
    
    console.log(`   Total rows: ${metrics.totalRows}`);
    console.log(`   Has vertical scrollbar: ${metrics.hasScrollbar}`);
    console.log(`   Viewport height: ${metrics.viewportHeight}px`);
    console.log(`   Document height: ${metrics.documentHeight}px`);
    
    // Take full page screenshots
    await takeKioskScreenshots(page, 'full-page', { fullPage: true });
    
    console.log('\n✅ Kiosk leaderboard full page comparison complete');
    console.log('='.repeat(70));
  });

  test('kiosk leaderboard - portrait orientation page size', async ({ page }) => {
    console.log('\n🏆 Test: Kiosk Leaderboard Portrait Page Size\n');
    console.log('='.repeat(70));

    // Test Portrait TV (height > width)
    console.log('\n📱 Testing Portrait TV (1080×1920)');
    await page.setViewportSize({ width: 1080, height: 1920 });
    await page.goto('https://localhost:7137/kiosk-leaderboard');
    await page.waitForSelector('.kiosk-leaderboard-table', { timeout: 15000 });
    await page.waitForSelector('.mud-table-row', { timeout: 15000 });
    await page.waitForTimeout(1500);
    
    let rows = await page.locator('.mud-table-row').count();
    console.log(`   Rows displayed: ${rows}`);
    console.log(`   Expected: ≤25 (portrait page size)`);
    console.log(`   ${rows <= 25 ? '✅' : '❌'} Portrait page size verified`);
    
    // Test Portrait iPhone 16 Pro (height > width)
    console.log('\n📱 Testing Portrait iPhone 16 Pro (393×852)');
    await page.setViewportSize({ width: 393, height: 852 });
    await page.waitForTimeout(2000); // Wait for page to adjust
    
    rows = await page.locator('.mud-table-row').count();
    console.log(`   Rows displayed: ${rows}`);
    console.log(`   Expected: ≤25 (portrait page size)`);
    console.log(`   ${rows <= 25 ? '✅' : '❌'} Portrait page size verified`);
    
    // Test Landscape FHD (width > height)  
    console.log('\n🖥️  Testing Landscape FHD Laptop (1920×1080)');
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto('https://localhost:7137/kiosk-leaderboard');
    await page.waitForSelector('.kiosk-leaderboard-table', { timeout: 15000 });
    await page.waitForSelector('.mud-table-row', { timeout: 15000 });
    await page.waitForTimeout(1500);
    
    rows = await page.locator('.mud-table-row').count();
    console.log(`   Rows displayed: ${rows}`);
    console.log(`   Expected: ≤30 (landscape page size)`);
    console.log(`   ${rows <= 30 ? '✅' : '❌'} Landscape page size verified`);
    
    // Test Landscape iPad Pro (width > height)
    console.log('\n💻 Testing Landscape iPad Pro (1366×1024)');
    await page.setViewportSize({ width: 1366, height: 1024 });
    await page.waitForTimeout(2000); // Wait for page to adjust
    
    rows = await page.locator('.mud-table-row').count();
    console.log(`   Rows displayed: ${rows}`);
    console.log(`   Expected: ≤30 (landscape page size)`);
    console.log(`   ${rows <= 30 ? '✅' : '❌'} Landscape page size verified`);
    
    console.log('\n✅ Portrait orientation page size test complete');
    console.log('='.repeat(70));
  });
});
