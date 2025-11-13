import { Page } from '@playwright/test';

export type Device = 'mobile' | 'tablet' | 'desktop';

export interface ScreenshotConfig {
  device: Device;
  width: number;
  height: number;
}

export const VIEWPORTS: Record<Device, ScreenshotConfig> = {
  mobile: { device: 'mobile', width: 375, height: 667 },   // iPhone SE
  tablet: { device: 'tablet', width: 1024, height: 768 },  // iPad Landscape
  desktop: { device: 'desktop', width: 1920, height: 1080 } // FHD (Full HD)
};

/**
 * Takes responsive screenshots across all devices with consistent naming
 * Format: {device}-{resolution}-{name}.png
 * Example: mobile-375x667-page-load.png
 * 
 * Automatically strips device prefixes (mobile-, tablet-, desktop-) from name to avoid redundancy
 * 
 * @param page - Playwright page object
 * @param basePath - Base path for screenshots (e.g., 'screenshots/notifications')
 * @param name - Screenshot name (e.g., 'page-load' or 'desktop-page-load' - both work)
 * @param options - Additional options
 */
export async function takeResponsiveScreenshots(
  page: Page,
  basePath: string,
  name: string,
  options: {
    fullPage?: boolean;
    collapseSidebar?: boolean;
    waitForNetwork?: boolean;
  } = {}
) {
  const { fullPage = true, collapseSidebar = true, waitForNetwork = false } = options;

  // Strip any device prefix from name to avoid redundancy (e.g., 'desktop-page-load' → 'page-load')
  const cleanName = name.replace(/^(mobile|tablet|desktop)-/, '');

  for (const [deviceName, config] of Object.entries(VIEWPORTS)) {
    const { device, width, height } = config;
    
    // Set viewport
    await page.setViewportSize({ width, height });
    
    // Collapse sidebar on mobile/tablet
    if (collapseSidebar && (device === 'mobile' || device === 'tablet')) {
      await collapseSidebarIfExpanded(page);
    }

    // Wait for network idle if requested
    if (waitForNetwork) {
      await page.waitForLoadState('networkidle', { timeout: 5000 }).catch(() => {
        console.log(`⚠️  Network idle timeout for ${device} - continuing anyway`);
      });
    }

    // Additional wait for page to settle
    await page.waitForTimeout(500);

    const filename = `${device}-${width}x${height}-${cleanName}.png`;
    const fullPath = `${basePath}/${filename}`;
    
    await page.screenshot({
      path: fullPath,
      fullPage
    });
    
    console.log(`📸 ${device.padEnd(7)} screenshot saved: ${fullPath}`);
  }
}

/**
 * Collapses the sidebar if it's currently expanded
 */
async function collapseSidebarIfExpanded(page: Page) {
  try {
    // Check if drawer is visible (expanded)
    const drawer = page.locator('.mud-drawer--open, .mud-drawer.mud-drawer-open');
    const isExpanded = await drawer.isVisible().catch(() => false);
    
    if (isExpanded) {
      // Click the hamburger menu to collapse
      const menuButton = page.locator('button[aria-label*="menu" i], .mud-icon-button:has(.mud-icon-root:text-matches("menu", "i"))').first();
      if (await menuButton.isVisible().catch(() => false)) {
        await menuButton.click();
        await page.waitForTimeout(300); // Wait for collapse animation
        console.log('🔽 Sidebar collapsed');
      }
    }
  } catch (error) {
    console.log('⚠️  Could not collapse sidebar:', error);
  }
}
